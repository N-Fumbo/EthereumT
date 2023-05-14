using Nethereum.ABI.FunctionEncoding.Attributes;

namespace EthereumT.Api.Infrastructure.Nethereum.Dto
{
    [Event("Transfer")]
    public class TransferEventDTO : IEventDTO
    {
        [Parameter("address", "_from", 1, true)]
        public virtual string From { get; set; }

        [Parameter("address", "_to", 2, true)]
        public virtual string To { get; set; }
    }
}