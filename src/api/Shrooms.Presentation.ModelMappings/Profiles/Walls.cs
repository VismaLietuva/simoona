using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Models.Wall;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Moderator;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.DataTransferObjects.Wall.Comments;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Presentation.WebViewModels.Models.Wall;
using Shrooms.Presentation.WebViewModels.Models.Wall.Moderator;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Walls : Profile
    {
        protected override void Configure()
        {
            CreateViewModelToDtoMappings();
            CreateDtoToViewModelMappings();
            CreateEntitiesToDtoMappings();
        }

        private void CreateDtoToViewModelMappings()
        {
            CreateMap<WallDto, WallListViewModel>();
            CreateMap<UserWallDTO, UserWallViewModel>();
            CreateMap<ModeratorDto, ModeratorViewModel>();
            CreateMap<WallMemberDto, WallMemberViewModel>();
        }

        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateWallViewModel, CreateWallDto>()
                .ForMember(dest => dest.ModeratorsIds, opt => opt.MapFrom(src => src.Moderators.Select(x => x.Id)))
                .Ignore(x => x.Access)
                .Ignore(x => x.MembersIds)
                .Ignore(x => x.Type)
                .IgnoreUserOrgDto();
            CreateMap<UpdateWallViewModel, UpdateWallDto>()
                .ForMember(dest => dest.ModeratorsIds, opt => opt.MapFrom(src => src.Moderators.Select(x => x.Id)))
                .IgnoreUserOrgDto();
        }

        private void CreateEntitiesToDtoMappings()
        {
            CreateMap<Post, PostDTO>()
                .ForMember(m => m.Author, opt => opt.Ignore())
                .ForMember(m => m.Likes, opt => opt.Ignore())
                .ForMember(m => m.IsLiked, opt => opt.Ignore())
                .ForMember(m => m.IsEdited, opt => opt.Ignore())
                .ForMember(m => m.CanModerate, opt => opt.Ignore())
                .ForMember(m => m.Comments, opt => opt.UseValue(new List<CommentDto>()))
                .ForMember(m => m.WallType, opt => opt.MapFrom(s => s.Wall.Type))
                .ForMember(m => m.WallName, opt => opt.MapFrom(s => s.Wall.Name))
                .ForMember(m => m.IsWatched, opt => opt.Ignore());

            CreateMap<Comment, CommentDto>()
                .ForMember(m => m.Author, opt => opt.Ignore())
                .ForMember(m => m.Likes, opt => opt.Ignore())
                .ForMember(m => m.IsLiked, opt => opt.Ignore())
                .ForMember(m => m.IsEdited, opt => opt.Ignore())
                .ForMember(m => m.CanModerate, opt => opt.Ignore());

            CreateMap<ApplicationUser, UserDto>()
                .ForMember(m => m.UserId, opt => opt.MapFrom(s => s.Id))
                .ForMember(m => m.FullName, opt => opt.MapFrom(s => s.FullName))
                .ForMember(m => m.PictureId, opt => opt.MapFrom(s => s.PictureId));
        }
    }
}
