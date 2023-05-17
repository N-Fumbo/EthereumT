using AutoMapper;
using EthereumT.Api.Infrastructure.Nethereum;
using EthereumT.Api.Infrastructure.Nethereum.Dto;
using EthereumT.Api.Models.Dto;
using EthereumT.Common.Extensions;
using EthereumT.DAL.Repositories.Base;
using EthereumT.Domain.Base.Repositories.Base;
using EthereumT.Domain.Entities;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Numerics;

namespace EthereumT.Api.Services
{
    public class ObserverETHWallets : BackgroundService
    {
        private const int NUMBER_PLACES_ETH_CURRENCY = 18;

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

            await using NethereumClient client = new(_infuraApiKey);

            string[] address = Data.Wallets.Select(x => x.Key).ToArray();

            NewFilterInput filterTransactions = new()
            {
                Address = address,
                FromBlock = BlockParameter.CreateEarliest(),
                ToBlock = BlockParameter.CreateLatest(),
            };

            Web3 web3 = new($"https://mainnet.infura.io/v3/{_infuraApiKey}");

            await client.SubscribeToReceiveTokenTransferLogsAsync(filterTransactions, async log =>
            {
                try
                {
                    var decoder = Event<TransferEventDTO>.DecodeEvent(log);
                    if (decoder != null)
                    {
                        if (Data.Wallets.TryGetValue(decoder.Event.To, out WalletDto walletTo))
                        {
                            var newBalance = await NethereumClient.GetBalance(walletTo.Address, web3);
                            walletTo.Balance = newBalance.ConvertToDecimalWithDecimalPlaces(NUMBER_PLACES_ETH_CURRENCY);
                        }

                        if (Data.Wallets.TryGetValue(decoder.Event.From, out WalletDto walletFrom))
                        {
                            var newBalance = await NethereumClient.GetBalance(walletFrom.Address, web3);
                            walletFrom.Balance = newBalance.ConvertToDecimalWithDecimalPlaces(NUMBER_PLACES_ETH_CURRENCY);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task InitWalletsETH(CancellationToken cancel = default)
        {
            Data.Wallets.Clear();

            const int pageSize = 250;

            int currentPageIndex = 0;
            int totalPagesCount = -1;

            Web3 web3 = new($"https://mainnet.infura.io/v3/{_infuraApiKey}");

            do
            {
                IPage<Wallet> page = await _walletsRepository.GetPageAsync(currentPageIndex, pageSize, cancel);
                if (totalPagesCount == -1)
                    totalPagesCount = page.TotalPagesCount;

                foreach (var wallet in page.Items)
                {
                    if (!Data.Wallets.ContainsKey(wallet.Address))
                    {
                        WalletDto walletDto = _mapper.Map<WalletDto>(wallet);
                        try
                        {
                            BigInteger balance = await NethereumClient.GetBalance(wallet.Address, web3);
                            walletDto.Balance = balance.ConvertToDecimalWithDecimalPlaces(NUMBER_PLACES_ETH_CURRENCY);
                            Data.Wallets.Add(walletDto.Address, walletDto);
                        }
                        catch (Exception ex)
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
