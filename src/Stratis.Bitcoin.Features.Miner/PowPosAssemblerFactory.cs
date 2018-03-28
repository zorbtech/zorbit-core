using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.Consensus;
using Stratis.Bitcoin.Features.Consensus.Interfaces;
using Stratis.Bitcoin.Features.MemoryPool;
using Stratis.Bitcoin.Features.MemoryPool.Interfaces;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.Miner
{
    public sealed class PowPosAssemblerFactory : PosAssemblerFactory
    {
        public PowPosAssemblerFactory(
            IConsensusLoop consensusLoop,
            Network network,
            MempoolSchedulerLock mempoolScheduler,
            ITxMempool mempool,
            IStakeValidator stakeValidator,
            IDateTimeProvider dateTimeProvider,
            ILoggerFactory loggerFactory,
            IStakeChain stakeChain = null) : base(consensusLoop, network, mempoolScheduler, mempool, stakeValidator, dateTimeProvider, loggerFactory, stakeChain)
        {
        }

        public override BlockAssembler Create(ChainedBlock chainTip, AssemblerOptions options = null)
        {
            return new PowPosBlockAssembler(this.consensusLoop, this.network, this.mempoolScheduler, this.mempool,
                this.dateTimeProvider, this.stakeChain, this.stakeValidator, chainTip, this.loggerFactory, options);
        }
    }
}