using EthereumT.Domain.Base.Entities;
using EthereumT.Domain.Entities.Base;

namespace EthereumT.Domain.Entities
{
    public class Wallet : Entity, IWallet
    {
        public string Address { get; set; }
    }
}