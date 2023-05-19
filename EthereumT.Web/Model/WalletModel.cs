using System.Text.Json.Serialization;

namespace EthereumT.Web.Model
{
    public class WalletModel
    {
        public int Id { get; set; }

        public string Address { get; set; }

        public decimal Balance { get; set; }
    }
}