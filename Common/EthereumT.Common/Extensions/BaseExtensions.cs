using System.Numerics;

namespace EthereumT.Common.Extensions
{
    public static class BaseExtensions
    {
        public static decimal ConvertToDecimalWithDecimalPlaces(this BigInteger value, int place)
        {
            BigInteger divisor = BigInteger.Pow(10, place);
            decimal result = (decimal)value / (decimal)divisor;
            return result;
        }
    }
}