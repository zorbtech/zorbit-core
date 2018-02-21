using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Base.Deployments;
using Stratis.Bitcoin.Features.Consensus.Interfaces;
using Stratis.Bitcoin.Features.MemoryPool;
using Stratis.Bitcoin.Features.MemoryPool.Interfaces;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.Miner
{
    public class PowPosBlockAssembler : PosBlockAssembler
    {
        public PowPosBlockAssembler(
            IConsensusLoop consensusLoop,
            Network network,
            MempoolSchedulerLock mempoolLock,
            ITxMempool mempool,
            IDateTimeProvider dateTimeProvider,
            StakeChain stakeChain,
            IStakeValidator stakeValidator,
            ChainedBlock chainTip,
            ILoggerFactory loggerFactory,
            AssemblerOptions options = null)
            : base(consensusLoop, network, mempoolLock, mempool, dateTimeProvider, stakeChain,
                stakeValidator, chainTip, loggerFactory, options)
        {
        }


        protected override int ComputeBlockVersion(ChainedBlock prevChainedBlock, NBitcoin.Consensus consensus)
        {
            if (this.options == null || this.options.IsProofOfStake)
                return base.ComputeBlockVersion(prevChainedBlock, consensus);

            uint nVersion = (uint)BlockHeader.CurrentVersion;
            var thresholdConditionCache = new ThresholdConditionCache(consensus);

            IEnumerable<BIP9Deployments> deployments = Enum.GetValues(typeof(BIP9Deployments))
                .OfType<BIP9Deployments>();

            foreach (BIP9Deployments deployment in deployments)
            {
                ThresholdState state = thresholdConditionCache.GetState(prevChainedBlock, deployment);
                if (state == ThresholdState.LockedIn || state == ThresholdState.Started)
                    nVersion |= thresholdConditionCache.Mask(deployment);
            }

            return (int)nVersion;
        }
    }
}