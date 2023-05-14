using AutoMapper;
using EthereumT.Api.Controllers.Models.Dto;
using EthereumT.Domain.Entities;

namespace EthereumT.Api.Controllers.Models.Mapping
{
    public class WalletProfile : Profile
    {
        public WalletProfile()
        {
            CreateMap<Wallet, WalletDto>();
        }
    }
}