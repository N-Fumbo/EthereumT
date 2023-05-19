using AutoMapper;
using EthereumT.Api.Infrastructure.Nethereum;
using EthereumT.Api.Infrastructure.Nethereum.Dto;
using EthereumT.Api.Models.Dto;
using EthereumT.Common.Extensions;
using EthereumT.DAL.Repositories.Base;
using EthereumT.Domain.Base.Repositories.Base;
using EthereumT.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Diagnostics;
using System.Numerics;

namespace EthereumT.Api.Services
{
    public class ObserverETHWallets : BackgroundService
    {
        private static readonly object _lockWallet = new();

        private const int NUMBER_PLACES_ETH_CURRENCY = 18;

        private readonly ILogger<ObserverETHWallets> _logger;

        private readonly IMemoryCache _cache;

        private readonly IMapper _mapper;

        private readonly IServiceProvider _serviceProvider;

        private readonly string _infuraApiKey;

        private RepositoryAsync<Wallet> _walletsRepository;

        public ObserverETHWallets(ILogger<ObserverETHWallets> logger, IMemoryCache cache, IMapper mapper, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _cache = cache;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _infuraApiKey = configuration.GetValue<string>("InfuraApiKey") ?? throw new InvalidOperationException("'InfuraApiKey' not found.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            _walletsRepository = scope.ServiceProvider.GetRequiredService<RepositoryAsync<Wallet>>();

            await InitWalletsETH(stoppingToken);

            await using NethereumClient client = new(_infuraApiKey);

            string[] address = _cache.Get<Dictionary<string, WalletDto>>("Wallets").Select(x => x.Key).ToArray();

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
                        if (_cache.TryGetValue("Wallets", out Dictionary<string, WalletDto> wallets))
                        {
                            if (wallets.TryGetValue(decoder.Event.To, out WalletDto walletTo))
                            {
                                var newBalance = await NethereumClient.GetBalance(walletTo.Address, web3);
                                walletTo.Balance = newBalance.ConvertToDecimalWithDecimalPlaces(NUMBER_PLACES_ETH_CURRENCY);
                            }

                            if (wallets.TryGetValue(decoder.Event.From, out WalletDto walletFrom))
                            {
                                var newBalance = await NethereumClient.GetBalance(walletFrom.Address, web3);
                                walletFrom.Balance = newBalance.ConvertToDecimalWithDecimalPlaces(NUMBER_PLACES_ETH_CURRENCY);
                            }
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
            Dictionary<string, WalletDto> wallets = new();

            const int pageSize = 200;

            int currentPageIndex = 0;
            int totalPagesCount = -1;

            Web3 web3 = new($"https://mainnet.infura.io/v3/{_infuraApiKey}");

            List<Task> getBalanceTasks = new();

            do
            {
                IPage<Wallet> page = await _walletsRepository.GetPageAsync(currentPageIndex, pageSize, cancel);
                if (totalPagesCount == -1)
                    totalPagesCount = page.TotalPagesCount;

                foreach (var wallet in page.Items)
                {
                    getBalanceTasks.Add(Task.Run(async () =>
                    {
                        bool isContainsKey = false;
                        lock (_lockWallet)
                        {
                            isContainsKey = wallets.ContainsKey(wallet.Address);
                        }

                        if (!isContainsKey)
                        {
                            WalletDto walletDto = _mapper.Map<WalletDto>(wallet);
                            try
                            {
                                BigInteger balance = await NethereumClient.GetBalance(wallet.Address, web3);
                                walletDto.Balance = balance.ConvertToDecimalWithDecimalPlaces(NUMBER_PLACES_ETH_CURRENCY);
                                lock (_lockWallet)
                                {
                                    wallets.Add(walletDto.Address, walletDto);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error getting balance ETH: {ex.Message}");
                            }
                        }
                    }, cancel));
                }

                currentPageIndex++;

            } while (currentPageIndex < totalPagesCount);

            await Task.WhenAll(getBalanceTasks);

            IEnumerable<WalletDto> walletsSortBalance = wallets.Select(x => x.Value).OrderByDescending(x => x.Balance);

            _cache.Set("Wallets", wallets);

            _cache.Set("WalletsSortBalance", walletsSortBalance);
            _cache.Set("IsActualWalletsSortBalance", true);
        }
    }
}
