using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.Premium.Tests.Mocks
{
    public class MockDbContext : DbContext, IDbContext
    {
        public List<ApplicationUser> ApplicationUsers { get; set; }

        public List<Office> Offices { get; set; }

        public List<Floor> Floors { get; set; }

        public List<Room> Rooms { get; set; }

        public List<RoomType> RoomTypes { get; set; }

        public List<Project> Projects { get; set; }

        public List<QualificationLevel> QualificationLevels { get; set; }

        public List<Organization> Organizations { get; set; }

        public List<Page> Pages { get; set; }

        public List<Permission> Permissions { get; set; }

        public List<Comment> Comments { get; set; }

        public List<Post> Posts { get; set; }

        public List<Like> Likes { get; set; }

        public List<ApplicationRole> Roles { get; set; }

        public List<KudosType> Types { get; set; }

        public List<KudosLog> Logs { get; set; }

        public List<ServiceRequest> ServiceRequests { get; set; }

        public List<Certificate> Certificates { get; set; }

        public List<Exam> Exams { get; set; }

        public List<Skill> Skills { get; set; }

        public string ConnectionName => "SimoonaTest";

        public MockDbContext()
        {
            CreateOrganizations();

            CreateApplicationUsers();

            CreateRoles();

            #region Rooms

            Room room1 = new Room
            {
                Id = 1,
                Name = "A-Room",
                Number = "1",
                FloorId = 1,
                Floor = new Floor
                {
                    Id = 1,
                    Name = "A-Floor",
                    OfficeId = 1,
                    OrganizationId = TestConstants.DefaultOrganizationId
                },
                RoomTypeId = 1,
                OrganizationId = TestConstants.DefaultOrganizationId,
                ApplicationUsers = new List<ApplicationUser> { ApplicationUsers[0], ApplicationUsers[1] }
            };

            Room room2 = new Room
            {
                Id = 2,
                Name = "Z-Room",
                FloorId = 1,
                OrganizationId = TestConstants.DefaultOrganizationId,
                Floor = new Floor
                {
                    Id = 1,
                    Name = "A-Floor",
                    OfficeId = 1,
                    OrganizationId = TestConstants.DefaultOrganizationId
                },
                Number = "2",
                ApplicationUsers = new List<ApplicationUser> { ApplicationUsers[2] }
            };

            Room room3 = new Room
            {
                Id = 3,
                Name = "B-Room",
                Number = "3",
                FloorId = 2,
                OrganizationId = TestConstants.DefaultOrganizationId,
                Floor = new Floor
                {
                    Id = 2,
                    Name = "B-Floor",
                    OfficeId = 1,
                    OrganizationId = TestConstants.DefaultOrganizationId
                },
                ApplicationUsers = new List<ApplicationUser>()
            };

            Room room4 = new Room
            {
                Id = 4,
                Name = "D-Room",
                FloorId = 1,
                OrganizationId = TestConstants.DefaultOrganizationId,
                Floor = new Floor
                {
                    Id = 1,
                    Name = "A-Floor",
                    OfficeId = 1,
                    OrganizationId = TestConstants.DefaultOrganizationId
                },
                ApplicationUsers = new List<ApplicationUser>()
            };

            Room room5 = new Room
            {
                Id = 5,
                Name = "5-Room",
                FloorId = 2,
                OrganizationId = TestConstants.DefaultOrganizationId,
                Floor = new Floor
                {
                    Id = 2,
                    Name = "B-Floor",
                    OfficeId = 1,
                    OrganizationId = TestConstants.DefaultOrganizationId
                },
                ApplicationUsers = new List<ApplicationUser>()
            };

            #endregion Rooms

            #region Floors

            Floor floor1 = new Floor
            {
                Id = 1,
                Name = "A-Floor",
                OfficeId = 1,
                Office = new Office { Id = 1 },
                OrganizationId = TestConstants.DefaultOrganizationId,
                Rooms = new List<Room> { room1, room2, room4 }
            };

            Floor floor2 = new Floor
            {
                Id = 2,
                Name = "Z-Floor",
                OfficeId = 1,
                Office = new Office { Id = 1 },
                Rooms = new List<Room> { room3 },
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            Floor floor3 = new Floor
            {
                Id = 3,
                Name = "D-Floor",
                OfficeId = 2,
                Office = new Office { Id = 2 },
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            Floor floor4 = new Floor
            {
                Id = 4,
                Name = "4floor",
                OfficeId = 3,
                Office = new Office { Id = 3 },
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            #endregion Floors

            #region Offices

            Office office1 = new Office
            {
                Id = 1,
                Name = "B-Office",
                Address = new Address
                {
                    Country = "Lithuania",
                    City = "Vilnius",
                    Street = "Lvovo",
                    Building = "25"
                },
                Floors = new List<Floor> { floor1, floor3 },
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            Office office2 = new Office
            {
                Id = 2,
                Name = "Z-Office",
                Address = new Address
                {
                    Country = "Lithuania",
                    City = "Vilnius",
                    Street = "Lvovo",
                    Building = "25"
                },
                Floors = new List<Floor> { floor2 },
                IsDefault = true,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            Office office3 = new Office
            {
                Id = 3,
                Name = "A-Office",
                OrganizationId = TestConstants.DefaultOrganizationId,
                Address = new Address
                {
                    Country = "Lithuania",
                    City = "Kaunas",
                    Street = "Lvovo",
                    Building = "25"
                }
            };

            Office office4 = new Office
            {
                Id = 4,
                Name = "W-Office",
                Address = new Address
                {
                    Country = "Lithuania",
                    City = "Kaunas",
                    Street = "Lvovo",
                    Building = "25"
                },
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            #endregion Offices

            #region RoomTypes

            RoomTypes = new List<RoomType>
            {
                new RoomType
                    {
                        Id = 1,
                        Name = "Room",
                        Color = "#FFFFFF",
                        Rooms = new List<Room> { room1, room2 },
                        OrganizationId = TestConstants.DefaultOrganizationId
                    },
                new RoomType
                    {
                        Id = 2,
                        Name = "Meeting Room",
                        Color = "#FF0000",
                        Rooms = new List<Room>(),
                        OrganizationId = TestConstants.DefaultOrganizationId
                    },
                new RoomType
                    {
                        Id = 3,
                        Name = "Kitchen",
                        Color = "#00FF00",
                        Rooms = new List<Room> { room3, room4 },
                        OrganizationId = TestConstants.DefaultOrganizationId
                    },
                new RoomType
                    {
                        Id = 4,
                        Name = "WC",
                        Color = "#808080",
                        Rooms = new List<Room> { room5 },
                        OrganizationId = TestConstants.DefaultOrganizationId
                    },
                new RoomType
                    {
                        Id = 10,
                        Name = "Unknown",
                        Color = "#000000",
                        Rooms = new List<Room>(),
                        OrganizationId = TestConstants.DefaultOrganizationId
                    }
            };

            #endregion RoomTypes

            #region QualificationLevels
            var qualificationLevel1 = new QualificationLevel
            {
                Id = 1,
                Name = "Junior",
                ApplicationUsers = new List<ApplicationUser> { ApplicationUsers[0], ApplicationUsers[1] },
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var qualificationLevel2 = new QualificationLevel
            {
                Id = 2,
                Name = "Senior",
                ApplicationUsers = new List<ApplicationUser> { ApplicationUsers[2], ApplicationUsers[3] },
                OrganizationId = TestConstants.DefaultOrganizationId
            };
            #endregion

            Rooms = new List<Room> { room1, room2, room3, room4, room5 };
            Floors = new List<Floor> { floor1, floor2, floor3, floor4 };
            Offices = new List<Office> { office1, office2, office3, office4 };
            QualificationLevels = new List<QualificationLevel> { qualificationLevel1, qualificationLevel2 };
        }

        public void CreateRoles()
        {
            Roles = new List<ApplicationRole>
            {
                new ApplicationRole
                {
                    Name = "Admin"
                }
            };
        }

        private void CreateOrganizations()
        {
            var organizations = new[]
            {
                new Organization
                {
                    Id = 1,
                    Name = "Test Full Name",
                    ShortName = "Test"
                },
                new Organization
                {
                    Id = 2,
                    Name = "Visma Lietuva",
                    ShortName = "Visma"
                }
            };

            Organizations = new List<Organization>(organizations);
        }

        private void CreateApplicationUsers()
        {
            var skills = new[]
            {
                new Skill { Title = "JAVA.NET" },
                new Skill { Title = ".NET" },
                new Skill { Title = "Photoshop Master" }
            };

            var applicationUser1 = new ApplicationUser
            {
                Id = "1",
                UserName = "applicationUser1",
                Email = "applicationUser@test.test.test",
                FirstName = "Aistis",
                LastName = "Aistietis",
                Skills = new[] { skills[0] },
                RoomId = 1,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var applicationUser2 = new ApplicationUser
            {
                Id = "2",
                UserName = "applicationUser2",
                Email = "applicationUser@test.test.test",
                FirstName = "Zita",
                LastName = "Zitaviciute",
                Skills = new[] { skills[1] },
                RoomId = 1,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var applicationUser3 = new ApplicationUser
            {
                Id = "3",
                UserName = "applicationUser3",
                Email = "applicationUser@test.test.test",
                FirstName = "Bronius",
                LastName = "Bronislavicius",
                Skills = new[] { new Skill { Title = "PHP" } },
                RoomId = 2,
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var applicationUser4 = new ApplicationUser
            {
                Id = "4",
                UserName = "applicationUser4",
                Email = "applicationUser@test.test.test",
                FirstName = "Tadas",
                LastName = "Tadauskas",
                Skills = new[] { skills[2] },
                RoomId = 1,
                OrganizationId = TestConstants.DefaultOrganizationId,
                Room = new Room
                {
                    Id = 4,
                    FloorId = 1,
                    Name = "D-Room",
                    OrganizationId = TestConstants.DefaultOrganizationId,
                    Floor = new Floor
                    {
                        Id = 1,
                        Name = "A-Floor",
                        OfficeId = 1,
                        OrganizationId = TestConstants.DefaultOrganizationId,
                        Office = new Office
                        {
                            Id = 1,
                            Name = "B-Office",
                            Address = new Address
                            {
                                Country = "Lithuania",
                                City = "Vilnius",
                                Street = "Lvovo",
                                Building = "25"
                            },
                            OrganizationId = TestConstants.DefaultOrganizationId
                        }
                    }
                }
            };

            var applicationUser5 = new ApplicationUser
            {
                Id = "5",
                UserName = "applicationUser5",
                Email = "applicationUser@test.test.test",
                FirstName = "Giedrius",
                LastName = "Giedriausias",
                RoomId = null,
                Skills = new[] { skills[1] },
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            var applicationUser6 = new ApplicationUser
            {
                Id = "6",
                UserName = "applicationUser6",
                Email = "applicationUser@test.test.test",
                FirstName = "Tomas",
                LastName = "Tomasautis",
                RoomId = null,
                Skills = new[] { skills[1] },
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            ApplicationUsers = new List<ApplicationUser> { applicationUser1, applicationUser2, applicationUser3, applicationUser4, applicationUser5, applicationUser6 };
        }

        public List<T> GetList<T>()
            where T : class
        {
            var prop = GetType().GetProperties().FirstOrDefault(p => p.PropertyType == typeof(List<T>));
            List<T> list = null;

            if (prop != null)
            {
                list = prop.GetValue(this, null) as List<T>;
            }

            return list;
        }

        private DbSet<T> CreateMock<T>(IQueryable<T> list)
            where T : class
        {
            var mockSet = Substitute.For<DbSet<T>, IQueryable<T>>();
            ((IQueryable<T>)mockSet).Provider.Returns(list.Provider);
            ((IQueryable<T>)mockSet).Expression.Returns(list.Expression);
            ((IQueryable<T>)mockSet).ElementType.Returns(list.ElementType);
            ((IQueryable<T>)mockSet).GetEnumerator().Returns(e => list.GetEnumerator());
            mockSet.Include(Arg.Any<string>()).Returns(mockSet);

            return mockSet;
        }

        public new DbSet<T> Set<T>()
            where T : class
        {
            DbSet<T> dbSet = null;

            var list = GetList<T>();

            if (list != null)
            {
                dbSet = CreateMock(list.AsQueryable());
            }

            return dbSet;
        }

        public int SaveChanges(bool useMetaTracking = true)
        {
            throw new NotImplementedException();
        }

        public int SaveChanges(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(bool useMetaTracking)
        {
            throw new NotImplementedException();
        }
    }
}