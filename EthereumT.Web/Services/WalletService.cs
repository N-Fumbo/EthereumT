using EthereumT.DAL.Repositories.Base;
using EthereumT.Domain.Base.Repositories.Base;
using EthereumT.Web.Model;
using System.Text.Json;

namespace EthereumT.Web.Services
{
    public class WalletService
    {
        private readonly HttpClient _httpClient;

        public WalletService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IPage<WalletModel>> GetPageWallets(int pageIndex, int pageSize)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7282/api/Wallets/GetPageWallersSortBalance?pageIndex={pageIndex}&pageSize={pageSize}");

            var content = await response.Content.ReadAsStringAsync();

            Page<WalletModel> page = JsonSerializer.Deserialize<Page<WalletModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

            return page;
        }
    }
}