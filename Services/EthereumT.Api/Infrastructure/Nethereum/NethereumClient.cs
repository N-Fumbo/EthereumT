using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Web3;
using System.Numerics;

namespace EthereumT.Api.Infrastructure.Nethereum
{
    public class NethereumClient : IAsyncDisposable
    {
        private readonly StreamingWebSocketClient _client;

        private readonly List<EthLogsObservableSubscription> _subscriptions = new();

        public NethereumClient(string infuraApiKey)
        {
            if(infuraApiKey is null) throw new ArgumentNullException(nameof(infuraApiKey));

            _client = new StreamingWebSocketClient($"wss://mainnet.infura.io/ws/v3/{infuraApiKey}");
        }

        public async Task SubscribeToReceiveTokenTransferLogsAsync(NewFilterInput filterInput, Action<FilterLog> action)
        {
            if (filterInput is null) throw new ArgumentNullException(nameof(filterInput));

            var subscription = new EthLogsObservableSubscription(_client);
            subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(action);

            if (!_client.IsStarted) await _client.StartAsync().ConfigureAwait(false);

            await subscription.SubscribeAsync(filterInput).ConfigureAwait(false);

            _subscriptions.Add(subscription);
        }

        public async Task UnsubscribeToReceiveTokenTransferLogsAsync()
        {
            foreach (var subscription in _subscriptions)
                await subscription.UnsubscribeAsync();
        }

        public async Task ClientStopAsync()
        {
            if (_client.IsStarted) await _client.StopAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await UnsubscribeToReceiveTokenTransferLogsAsync();
            await ClientStopAsync();
            _client.Dispose();
        }

        public static async Task<BigInteger> GetBalance(string address, string url)
        {
            if (address is null) throw new ArgumentNullException(nameof(address));

            if (url is null) throw new ArgumentNullException(nameof(url));

            Web3 web3 = new(url);
            return await web3.Eth.GetBalance.SendRequestAsync(address).ConfigureAwait(false);
        }

        public static async Task<BigInteger> GetBalance(string address, Web3 web3)
        {
            if (address is null) throw new ArgumentNullException(nameof(address));
            if (web3 is null) throw new ArgumentNullException(nameof(web3));

            return await web3.Eth.GetBalance.SendRequestAsync(address).ConfigureAwait(false);
        }
    }
}
