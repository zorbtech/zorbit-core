﻿using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.Consensus;
using Stratis.Bitcoin.Features.Consensus.Interfaces;
using Stratis.Bitcoin.Features.MemoryPool;
using Stratis.Bitcoin.Features.MemoryPool.Interfaces;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.Miner
{
    public class PosBlockAssembler : PowBlockAssembler
    {
        /// <summary>Instance logger.</summary>
        protected readonly ILogger logger;

        /// <summary>Database of stake related data for the current blockchain.</summary>
        protected readonly StakeChain stakeChain;

        /// <summary>Provides functionality for checking validity of PoS blocks.</summary>
        protected readonly IStakeValidator stakeValidator;

        public PosBlockAssembler(
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
            : base(consensusLoop, network, mempoolLock, mempool, dateTimeProvider, chainTip, loggerFactory, options)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.stakeChain = stakeChain;
            this.stakeValidator = stakeValidator;
        }

        public override BlockTemplate CreateNewBlock(Script scriptPubKeyIn, bool fMineWitnessTx = true)
        {
            this.logger.LogTrace("({0}.{1}:{2},{3}:{4})", nameof(scriptPubKeyIn), nameof(scriptPubKeyIn.Length), scriptPubKeyIn.Length, nameof(fMineWitnessTx), fMineWitnessTx);

            base.CreateNewBlock(scriptPubKeyIn, fMineWitnessTx);

            this.ClearCoinbase();

            IPosConsensusValidator posValidator = this.consensusLoop.Validator as IPosConsensusValidator;
            Guard.NotNull(posValidator, nameof(posValidator));

            this.logger.LogTrace("(-)");
            return this.pblocktemplate;
        }

        protected virtual void ClearCoinbase()
        {
            this.coinbase.Outputs[0].ScriptPubKey = new Script();
            this.coinbase.Outputs[0].Value = Money.Zero;
        }

        protected override void UpdateHeaders()
        {
            this.logger.LogTrace("()");

            base.UpdateHeaders();

            var stake = new BlockStake(this.pblock);
            this.pblock.Header.Bits = this.stakeValidator.GetNextTargetRequired(this.stakeChain, this.ChainTip, this.network.Consensus, this.options.IsProofOfStake);

            this.logger.LogTrace("(-)");
        }

        protected override void TestBlockValidity()
        {
            this.logger.LogTrace("()");

            //base.TestBlockValidity();

            this.logger.LogTrace("(-)");
        }
    }
}
