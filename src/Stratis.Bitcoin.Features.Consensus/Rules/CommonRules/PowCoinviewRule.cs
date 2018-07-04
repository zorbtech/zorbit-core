using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Base.Deployments;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.Consensus.Rules.CommonRules
{
    /// <inheritdoc />
    [ExecutionRule]
    public sealed class PowCoinviewRule : CoinViewRule
    {
        /// <summary>Consensus parameters.</summary>
        private NBitcoin.Consensus consensusParams;

        /// <inheritdoc />
        public override void Initialize()
        {
            this.Logger.LogTrace("()");

            base.Initialize();

            this.consensusParams = this.Parent.Network.Consensus;

            this.Logger.LogTrace("(-)");
        }

        /// <inheritdoc/>
        public override void CheckBlockReward(RuleContext context, Money fees, int height, Block block)
        {
            this.Logger.LogTrace("()");

            Money blockReward = fees + this.GetProofOfWorkReward(height);
            if (block.Transactions[0].TotalOut > blockReward)
            {
                this.Logger.LogTrace("(-)[BAD_COINBASE_AMOUNT]");
                ConsensusErrors.BadCoinbaseAmount.Throw();
            }

            this.Logger.LogTrace("(-)");
        }

        /// <inheritdoc/>
        public override Money GetProofOfWorkReward(int height)
        {
            int halvings = height / this.consensusParams.SubsidyHalvingInterval;

            // Force block reward to zero when right shift is undefined.
            if (halvings >= 64)
                return 0;

            Money subsidy = this.PowConsensusOptions.ProofOfWorkReward;
            // Subsidy is cut in half every 210,000 blocks which will occur approximately every 4 years.
            subsidy >>= halvings;

            return subsidy;
        }

        /// <inheritdoc/>
        public override void CheckMaturity(UnspentOutputs coins, int spendHeight)
        {
            base.CheckCoinbaseMaturity(coins, spendHeight);
        }

        /// <inheritdoc/>
        public override void UpdateCoinView(RuleContext context, Transaction transaction)
        {
            base.UpdateUTXOSet(context, transaction);
        }

        /// <inheritdoc />
        /// 
        public override Task RunAsync(RuleContext context)
        {
            return base.RunAsync(context);
        }

        public void UpdateUncommittedBlockStructures(Block block, ChainedHeader previousBlock)
        {
            var commitPos = this.GetWitnessCommitmentIndex(block);
            var nonce = RandomUtils.GetUInt64();
            var nonceBytes = Encoding.Default.GetBytes(nonce.ToString());
            if (commitPos != -1 && this.IsWitnessEnabled(previousBlock) && !block.Transactions[0].HasWitness)
            {
                var transaction = block.Transactions[0];
                transaction.Inputs[0].WitScript = new WitScript(new[] { nonceBytes }, true);
            }
        }

        private bool IsWitnessEnabled(ChainedHeader block)
        {
            var thresholdConditionCache = new ThresholdConditionCache(this.consensusParams);
            var state = thresholdConditionCache.GetState(block, BIP9Deployments.Segwit);
            return state == ThresholdState.Active;
        }

        /// <summary>
        /// Gets index of the last coinbase transaction output with SegWit flag.
        /// </summary>
        /// <param name="block">Block which coinbase transaction's outputs will be checked for SegWit flags.</param>
        /// <returns>
        /// <c>-1</c> if no SegWit flags were found.
        /// If SegWit flag is found index of the last transaction's output that has SegWit flag is returned.
        /// </returns>
        private int GetWitnessCommitmentIndex(Block block)
        {
            int commitpos = -1;
            for (int i = 0; i < block.Transactions[0].Outputs.Count; i++)
            {
                var scriptPubKey = block.Transactions[0].Outputs[i].ScriptPubKey;

                if (scriptPubKey.Length >= 38)
                {
                    byte[] scriptBytes = scriptPubKey.ToBytes(true);

                    if ((scriptBytes[0] == (byte)OpcodeType.OP_RETURN) &&
                        (scriptBytes[1] == 0x24) &&
                        (scriptBytes[2] == 0xaa) &&
                        (scriptBytes[3] == 0x21) &&
                        (scriptBytes[4] == 0xa9) &&
                        (scriptBytes[5] == 0xed))
                    {
                        commitpos = i;
                    }
                }
            }

            return commitpos;
        }
    }
}