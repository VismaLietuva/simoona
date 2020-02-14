using System.Collections.Generic;
using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.Notification;
using Shrooms.DataLayer.EntityModels.Models.Notifications;
using Shrooms.Presentation.WebViewModels.Models.Notifications;
using Shrooms.Presentation.WebViewModels.Models.Wall.Posts;

namespace Shrooms.Presentation.ModelMappings.Profiles
{
    public class Notifications : Profile
    {
        protected override void Configure()
        {
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.SourceIds, opt => opt.MapFrom(u => u.Sources));
            CreateMap<NotificationDto, NotificationViewModel>()
                .ForMember(dest => dest.stackedIds, opt => opt.UseValue(new List<int>()))
                .ForMember(dest => dest.others, opt => opt.UseValue(0));

            CreateMap<WallPostViewModel, NotificationDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(u => u.MessageBody))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(u => u.Author.FullName));

            CreateMap<Sources, SourcesDto>();
            CreateMap<SourcesDto, SourcesViewModel>();
        }
    }
}
