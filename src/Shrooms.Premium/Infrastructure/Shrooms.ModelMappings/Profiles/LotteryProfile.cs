using AutoMapper;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.WebViewModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.ModelMappings.Profiles
{
    public class LotteryProfile : Profile
    {
        protected override void Configure()
        {
            CreateViewModelToDtoMappings();
            CreateDtoMappings();
        }
        private void CreateViewModelToDtoMappings()
        {
            CreateMap<CreateLotteryDTO, CreateLotteryViewModel>();
            CreateMap<CreateLotteryViewModel, CreateLotteryDTO>();

            CreateMap<LotteryDetailsDTO, LotteryDetailsViewModel>();
            CreateMap<LotteryDetailsViewModel, LotteryDetailsDTO>();
        }
        private void CreateDtoMappings()
        {
            CreateMap<CreateLotteryDTO, Lottery>();
            CreateMap<Lottery, CreateLotteryDTO>();
        }
    }
}
