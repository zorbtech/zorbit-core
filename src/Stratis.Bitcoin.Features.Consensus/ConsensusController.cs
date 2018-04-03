using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.RPC;
using Stratis.Bitcoin.Base;
using Stratis.Bitcoin.Features.BlockStore;
using Stratis.Bitcoin.Features.Consensus.Interfaces;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.Consensus
{
    public class ConsensusController : FeatureController
    {
        private readonly ILogger logger;

        public IConsensusLoop ConsensusLoop { get; private set; }

        public ConsensusController(ILoggerFactory loggerFactory, IChainState chainState = null,
            IConsensusLoop consensusLoop = null, ConcurrentChain chain = null)
            : base(chainState: chainState, chain: chain)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.ConsensusLoop = consensusLoop;
        }

        [ActionName("getblockchaininfo")]
        [ActionDescription("Get ")]
        public BlockChainInfo GetBlockChainInfo()
        {
            var chainInfo = new BlockChainInfo();
            chainInfo.Chain = this.Chain.Network.ToString();
            chainInfo.Blocks = this.ChainState.ConsensusTip.Height;
            // chainInfo.Headers = this.ChainState.ConsensusTip.ToString();
            chainInfo.BestBlockHash = this.ChainState.ConsensusTip.HashBlock.ToString();
            chainInfo.Difficulty = this.GetNetworkDifficulty();
            chainInfo.MedianTime = this.Chain.Tip.GetMedianTimePast().ToUnixTimeSeconds();
            chainInfo.VerificationProgress = this.GetVerificationProgress();
            chainInfo.Pruned = this.GetPruneStatus();

            return chainInfo;
        }

        [ActionName("getdifficulty")]
        [ActionDescription("Get the current proof of work difficulty")]
        public double GetDifficulty()
        {
            return this.GetNetworkDifficulty();
        }

        [ActionName("getbestblockhash")]
        [ActionDescription("Get the hash of the block at the consensus tip.")]
        public uint256 GetBestBlockHash()
        {
            Guard.NotNull(this.ChainState, nameof(this.ChainState));
            return this.ChainState?.ConsensusTip?.HashBlock;
        }

        [ActionName("getblockhash")]
        [ActionDescription("Gets the hash of the block at the given height.")]
        public uint256 GetBlockHash(int height)
        {
            Guard.NotNull(this.ConsensusLoop, nameof(this.ConsensusLoop));
            Guard.NotNull(this.Chain, nameof(this.Chain));

            this.logger.LogDebug("RPC GetBlockHash {0}", height);

            uint256 bestBlockHash = this.ConsensusLoop.Tip?.HashBlock;
            ChainedBlock bestBlock = bestBlockHash == null ? null : this.Chain.GetBlock(bestBlockHash);
            if (bestBlock == null)
                return null;
            ChainedBlock block = this.Chain.GetBlock(height);
            return block == null || block.Height > bestBlock.Height ? null : block.HashBlock;
        }

        private double GetVerificationProgress()
        {
            if (this.Chain.Tip == null)
                return 0.0;

            // requires total transaction count,
            // timestamp of last known number of transactions,
            // estimated number of transactions per second since timestamp

            return 1.0;
        }

        private bool GetPruneStatus()
        {
            StoreSettings blockSettings = (StoreSettings)this.FullNode.Services.ServiceProvider.GetService(typeof(StoreSettings));
            return blockSettings.Prune;
        }

        private double GetNetworkDifficulty()
        {
            return this.ConsensusLoop.Tip.GetWorkRequired(this.Chain.Network)?.Difficulty ?? 0.0;
        }
    }
}
