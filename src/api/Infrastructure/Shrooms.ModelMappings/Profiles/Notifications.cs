using System.Collections.Generic;
using AutoMapper;
using Shrooms.DataTransferObjects.Models.Notification;
using Shrooms.EntityModels.Models.Notifications;
using Shrooms.WebViewModels.Models.Notifications;
using Shrooms.WebViewModels.Models.Wall.Posts;

namespace Shrooms.ModelMappings.Profiles
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
