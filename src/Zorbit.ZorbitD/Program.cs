using System;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Features.Api;
using Stratis.Bitcoin.Features.BlockStore;
using Stratis.Bitcoin.Features.Consensus;
using Stratis.Bitcoin.Features.MemoryPool;
using Stratis.Bitcoin.Features.Miner;
using Stratis.Bitcoin.Features.RPC;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Utilities;

namespace Zorbit.ZorbitD
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        public static async Task MainAsync(string[] args)
        {
            try
            {
                Network network = args.Contains("-testnet") ? Network.ZorbitTest : Network.ZorbitMain;
                NodeSettings nodeSettings = new NodeSettings(network, ProtocolVersion.ALT_PROTOCOL_VERSION, args:args, loadConfiguration:false);

                // NOTES: running BTC and STRAT side by side is not possible yet as the flags for serialization are static
                var node = new FullNodeBuilder()
                    .UseNodeSettings(nodeSettings)
                    .UsePosConsensus()
                    .UseBlockStore()
                    .UseMempool()
                    .UseWallet()
                    .AddPowPosMining()
                    .UseApi()
                    .AddRPC()
                    .Build();

                await node.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem initializing the node. Details: '{0}'", ex.Message);
            }
        }
    }
}
