using AutoMapper;
using EthereumT.Api.Infrastructure.Nethereum;
using EthereumT.Api.Models.Dto;
using EthereumT.Common.Extensions;
using EthereumT.DAL.Repositories.Base;
using EthereumT.Domain.Base.Repositories.Base;
using EthereumT.Domain.Entities;
using Nethereum.Web3;
using System.Numerics;

namespace EthereumT.Api.Services
{
    public class ObserverETHWallets : BackgroundService
    {
        private readonly IMapper _mapper;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<ObserverETHWallets> _logger;

        private readonly string _infuraApiKey;

        private RepositoryAsync<Wallet> _walletsRepository;

        public ObserverETHWallets(ILogger<ObserverETHWallets> logger, IMapper mapper, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _infuraApiKey = configuration.GetValue<string>("InfuraApiKey") ?? throw new InvalidOperationException("'InfuraApiKey' not found.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            _walletsRepository = scope.ServiceProvider.GetRequiredService<RepositoryAsync<Wallet>>();

            await InitWalletsETH(stoppingToken);


            while (!stoppingToken.IsCancellationRequested)
            {

            }
        }

        private async Task InitWalletsETH(CancellationToken cancel = default)
        {
            Data.Wallets.Clear();

            const int pageSize = 10;

            int currentPageIndex = 0;
            int totalPagesCount = -1;

            Web3 web3 = new($"https://mainnet.infura.io/v3/{_infuraApiKey}");

            do
            {
                IPage<Wallet> page = await _walletsRepository.GetPageAsync(currentPageIndex, pageSize, cancel);
                if (totalPagesCount == -1)
                    totalPagesCount = 1;//page.TotalPagesCount;

                foreach(var wallet in page.Items)
                {
                    if (!Data.Wallets.ContainsKey(wallet.Address))
                    {
                        WalletDto walletDto = _mapper.Map<WalletDto>(wallet);
                        try
                        {
                            BigInteger balance = await NethereumClient.GetBalance(wallet.Address, web3);
                            walletDto.Balance = balance.ConvertToDecimalWithDecimalPlaces(18);
                            Data.Wallets.Add(walletDto.Address, walletDto);
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError($"Error getting balance ETH: {ex.Message}");
                        }
                    }
                }

                currentPageIndex++;

            } while (currentPageIndex < totalPagesCount);
        }
    }
}
