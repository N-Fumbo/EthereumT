using EthereumT.Domain.Base.Entities.Base;

namespace EthereumT.Domain.Base.Entities
{
    public interface IWallet : IEntity
    {
        public string Address { get; set; }
    }
}