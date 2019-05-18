using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.DataLayer;
using Shrooms.Constants.ErrorCodes;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Projects;
using Shrooms.DataTransferObjects.Models.Users;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Domain.Services.Projects
{
    public class ProjectsService : IProjectsService
    {
        private readonly IDbSet<Skill> _skillsDbSet;
        private readonly IDbSet<Project> _projectsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        private readonly IUnitOfWork2 _uow;
        private readonly IWallService _wallService;
        private readonly IPermissionService _permissionService;

        public ProjectsService(IUnitOfWork2 uow, IWallService wallService, IPermissionService permissionService)
        {
            _uow = uow;
            _skillsDbSet = uow.GetDbSet<Skill>();
            _projectsDbSet = uow.GetDbSet<Project>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();

            _wallService = wallService;
            _permissionService = permissionService;
        }

        public async Task<IEnumerable<ProjectsListItemDto>> GetProjects(UserAndOrganizationDTO userOrg)
        {
            var projects = await _projectsDbSet
                .Include(p => p.Attributes)
                .Include(p => p.Members)
                .Where(p => p.OrganizationId == userOrg.OrganizationId)
                .Select(p => new ProjectsListItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Attributes = p.Attributes.Select(t => t.Title),
                    Members = p.Members.Select(m => m.FirstName + " " + m.LastName),
                    IsCurrentUserAMember = p.Members.Any(m => m.Id == userOrg.UserId)
                })
                .ToListAsync();

            return projects;
        }

        public async Task<IEnumerable<ProjectsAutoCompleteDto>> GetProjectsForAutocomplete(string name, int organizationId)
        {
            var nameInLowerCase = name.ToLower();

            var projects = await _projectsDbSet
                .Where(x => x.OrganizationId == organizationId &&
                            x.Name.ToLower().StartsWith(nameInLowerCase))
                .Select(x => new ProjectsAutoCompleteDto()
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();

            return projects;
        }

        public async Task<ProjectDetailsDto> GetProjectDetails(int projectId, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            var project = await _projectsDbSet
                .Include(x => x.Members)
                .Include(x => x.Owner)
                .Include(x => x.Attributes)
                .Where(x => x.OrganizationId == userAndOrganizationDTO.OrganizationId &&
                            x.Id == projectId)
                .Select(x => new ProjectDetailsDto()
                {
                    Name = x.Name,
                    Desc = x.Desc,
                    WallId = x.WallId,
                    LogoId = x.Logo,
                    Owner = new ApplicationUserMinimalViewModelDto()
                    {
                        Id = x.Owner.Id,
                        FirstName = x.Owner.FirstName,
                        LastName = x.Owner.LastName,
                        PictureId = x.Owner.PictureId
                    },
                    Attributes = x.Attributes.Select(t => t.Title),
                    Members = x.Members
                        .Select(m => new ApplicationUserMinimalViewModelDto()
                        {
                            Id = m.Id,
                            FirstName = m.FirstName,
                            LastName = m.LastName,
                            PictureId = m.PictureId
                        })
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist);
            }

            return project;
        }

        public async Task<EditProjectDisplayDto> GetProjectById(int projectId, UserAndOrganizationDTO userOrg)
        {
            string projectOwnerId = await _projectsDbSet.Where(p =>
                    p.Id == projectId &&
                    p.OrganizationId == userOrg.OrganizationId).Select(s => s.OwnerId)
                    .SingleAsync();

            ValidateOwnershipPermissions(projectOwnerId, userOrg);

            var project = await _projectsDbSet
                .Include(p => p.Members)
                .Include(p => p.Attributes)
                .Include(p => p.Owner)
                .Where(p =>
                    p.Id == projectId &&
                    p.OrganizationId == userOrg.OrganizationId)
                .Select(p => new EditProjectDisplayDto
                {
                    Id = p.Id,
                    Description = p.Desc,
                    Logo = p.Logo,
                    Title = p.Name,
                    Owner = new UserDto
                    {
                        UserId = p.Owner.Id,
                        FullName = p.Owner.FirstName + " " + p.Owner.LastName
                    },
                    Members = p.Members.Select(m => new UserDto
                    {
                        UserId = m.Id,
                        FullName = m.FirstName + " " + m.LastName
                    }),
                    Attributes = p.Attributes.Select(t => t.Title).ToList()
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Project not found");
            }

            return project;
        }

        public async Task NewProject(NewProjectDto dto)
        {
            var owningUserExists = await _usersDbSet
                .AnyAsync(u => u.Id == dto.OwningUserId && u.OrganizationId == dto.OrganizationId);

            if (!owningUserExists)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Incorrect user");
            }

            var members = await _usersDbSet
                .Where(u => dto.MembersIds.Contains(u.Id))
                .ToListAsync();

            var completeListOfAttributes = await ManageProjectAttributes(dto.Attributes);

            var project = new Project
            {
                Name = dto.Title,
                Desc = dto.Description,
                OwnerId = dto.OwningUserId,
                OrganizationId = dto.OrganizationId,
                Logo = dto.Logo,
                Attributes = completeListOfAttributes.ToList(),
                Members = members
            };

            var wall = new CreateWallDto
            {
                Name = dto.Title,
                UserId = dto.OwningUserId,
                OrganizationId = dto.OrganizationId,
                Type = WallType.Project,
                Description = dto.Description,
                Logo = dto.Logo,
                Access = WallAccess.Public,
                MembersIds = members.Select(m => m.Id).Concat(new List<string> { dto.OwningUserId }),
                ModeratorsIds = new List<string> { dto.OwningUserId }
            };

            _projectsDbSet.Add(project);
            await _wallService.CreateNewWall(wall);

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task EditProject(EditProjectDto dto)
        {
            var project = await _projectsDbSet
                .Include(p => p.Members)
                .Include(p => p.Wall.Members)
                .Include(p => p.Wall.Moderators)
                .Include(p => p.Attributes)
                .FirstOrDefaultAsync(p =>
                    p.Id == dto.Id &&
                    p.OrganizationId == dto.OrganizationId);

            if (project == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Project not found");
            }

            ValidateOwnershipPermissions(project.OwnerId, dto);

            if (project.OwnerId != dto.OwningUserId)
            {
                var owningUserExists = await _usersDbSet.AnyAsync(u => u.Id == dto.OwningUserId && u.OrganizationId == dto.OrganizationId);
                if (!owningUserExists)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "User not found");
                }
            }

            project.Members = await _usersDbSet.Where(x => dto.MembersIds.Contains(x.Id)).ToListAsync();

            _wallService.ReplaceMembersInWall(project.Members.ToList(), project.WallId, dto.UserId);

            var completeListOfAttributes = await ManageProjectAttributes(dto.Attributes);

            project.Name = dto.Title;
            project.Desc = dto.Description;
            project.Logo = dto.Logo;
            project.OwnerId = dto.OwningUserId;
            project.Attributes = completeListOfAttributes.ToList();
            UpdateProjectWallModerator(dto, project);

            UpdateWall(dto, project.WallId);

            await _uow.SaveChangesAsync(dto.UserId);
        }

        public async Task Delete(int id, UserAndOrganizationDTO userOrg)
        {
            var project = await _projectsDbSet
                .Include(p => p.Members)
                .Include(p => p.Wall.Members)
                .Include(p => p.Wall.Moderators)
                .Include(p => p.Attributes)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.OrganizationId == userOrg.OrganizationId);

            if (project == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Project not found");
            }

            ValidateOwnershipPermissions(project.OwnerId, userOrg);

            _projectsDbSet.Remove(project);
            _wallService.DeleteWall(project.WallId, userOrg, WallType.Project);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task ExpelMember(UserAndOrganizationDTO userAndOrg, int projectId, string expelUserId)
        {
            var project = await _projectsDbSet
                .Include(x => x.Members)
                .FirstOrDefaultAsync(x => x.Id == projectId && 
                                     x.OrganizationId == userAndOrg.OrganizationId);

            ValidateExpelMember(project, userAndOrg, expelUserId);

            project.Members.Remove(project.Members.FirstOrDefault(x => x.Id == expelUserId));

            _wallService.RemoveMemberFromWall(expelUserId, project.WallId);

            await _uow.SaveChangesAsync(userAndOrg.UserId);
        }

        public void AddProjectsToUser(string userId, IEnumerable<int> newProjectIds, UserAndOrganizationDTO userOrg)
        {
            var user = _usersDbSet
                .Include(x => x.Projects)
                .First(x => x.Id == userId && 
                            x.OrganizationId == userOrg.OrganizationId);

            var wallsThatShouldBeRemovedFromUser = user.Projects
                .Where(x => !newProjectIds.Contains(x.Id))
                .Select(x => x.WallId)
                .ToList();

            var wallsThatShouldBeAddedToUser = _projectsDbSet.Where(x => newProjectIds.Contains(x.Id)).Select(x => x.WallId).ToList();

            _wallService.AddMemberToWalls(userId, wallsThatShouldBeAddedToUser);
            _wallService.RemoveMemberFromWalls(userId, wallsThatShouldBeRemovedFromUser);

            user.Projects = _projectsDbSet.Where(p => newProjectIds.Contains(p.Id)).ToList();

            _uow.SaveChanges(userOrg.UserId);
        }

        public bool ValidateManagerId(string userId, string managerId)
        {
            var user = _usersDbSet.Find(userId);
            var manager = _usersDbSet.Find(managerId);

            if (ConstDataLayer.OrganizationManagerUsername.Equals(user.UserName, StringComparison.InvariantCultureIgnoreCase) && user.Id == manager.Id)
            {
                return true;
            }

            while (manager.Id != user.Id && manager.ManagerId != null && manager.ManagerId != manager.Id)
            {
                manager = _usersDbSet.Find(manager.ManagerId);
            }

            if (manager.Id == user.Id)
            {
                return false;
            }

            if (manager.ManagerId == null || (manager.Id == manager.ManagerId && manager.UserName == ConstDataLayer.OrganizationManagerUsername))
            {
                return true;
            }

            return false;
        }

        private void UpdateWall(EditProjectDto dto, int wallId)
        {
            var updateWallDto = new UpdateWallDto()
            {
                Id = wallId,
                Description = dto.Description,
                Logo = dto.Logo,
                Name = dto.Title,
                OrganizationId = dto.OrganizationId,
                UserId = dto.UserId
            };

            _wallService.UpdateWall(updateWallDto);
        }

        private void UpdateProjectWallModerator(EditProjectDto dto, Project project)
        {
            if (project.Wall.Moderators.Any())
            {
                var currentModeratorId = project.Wall.Moderators.First().UserId;

                if (currentModeratorId != dto.OwningUserId)
                {
                    if (!dto.MembersIds.Contains(currentModeratorId))
                    {
                        _wallService.RemoveMemberFromWall(currentModeratorId, project.WallId);
                    }

                    _wallService.RemoveModerator(project.WallId, project.Wall.Moderators.First().UserId, dto);
                    _wallService.AddModerator(project.WallId, dto.OwningUserId, dto);
                }
            }
            else
            {
                _wallService.AddModerator(project.WallId, dto.OwningUserId, dto);
            }
        }

        private async Task<IEnumerable<Skill>> ManageProjectAttributes(IEnumerable<string> submittedAttribute)
        {
            var projectAttributes = new List<Skill>();

            var attributes = submittedAttribute
                .Select(t => t.ToLowerInvariant())
                .Where(t => !string.IsNullOrEmpty(t));

            var existingAttributes = await _skillsDbSet
                .Where(s => attributes.Contains(s.Title.ToLower()))
                .ToListAsync();

            var newAttributes = attributes
                .Except(existingAttributes
                    .Select(t => t.Title.ToLowerInvariant())
                    .Where(attr => !string.IsNullOrEmpty(attr)))
                .Select(attr => new Skill
                {
                    Title = attr,
                    ShowInAutoComplete = true,
                });

            foreach (var attr in newAttributes)
            {
                _skillsDbSet.Add(attr);
                projectAttributes.Add(attr);
            }

            return projectAttributes.Union(existingAttributes);
        }

        private void ValidateExpelMember(Project project, UserAndOrganizationDTO userAndOrg, string expelUserId)
        {
            if (project == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Project does not exist");
            }

            var hasProjectAdminPermission = _permissionService.UserHasPermission(userAndOrg, AdministrationPermissions.Project);
            var isProjectOwner = project.OwnerId == userAndOrg.UserId;

            if (!(isProjectOwner || hasProjectAdminPermission))
            {
                throw new UnauthorizedException();
            }
        }

        private void ValidateOwnershipPermissions(string ownerId, UserAndOrganizationDTO userOrg)
        {
            var isAdministrator = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Project);

            if (ownerId != userOrg.UserId && !isAdministrator)
            {
                throw new UnauthorizedException();
            }
        }
    }
}
