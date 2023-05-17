using AutoMapper;
using EthereumT.Api.Models.Dto;
using EthereumT.Domain.Entities;

namespace EthereumT.Api.Models.Mapping
{
    public class WalletProfile : Profile
    {
        public WalletProfile()
        {
            CreateMap<Wallet, WalletDto>();
        }
    }
}