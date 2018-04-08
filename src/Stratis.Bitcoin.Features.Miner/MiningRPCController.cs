﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.RPC.Dtos;
using Stratis.Bitcoin.Base;
using Stratis.Bitcoin.Base.Deployments;
using Stratis.Bitcoin.Features.Miner.Interfaces;
using Stratis.Bitcoin.Features.Miner.Models;
using Stratis.Bitcoin.Features.RPC;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Features.Wallet.Interfaces;
using Stratis.Bitcoin.Utilities;
using BlockTemplateResponse = NBitcoin.RPC.Dtos.BlockTemplate;

namespace Stratis.Bitcoin.Features.Miner
{
    /// <summary>
    /// RPC controller for calls related to PoW mining and PoS minting.
    /// </summary>
    [Controller]
    public class MiningRPCController : FeatureController
    {
        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        /// <summary>PoW miner.</summary>
        private readonly IPowMining powMining;

        /// <summary>PoS staker.</summary>
        private readonly IPosMinting posMinting;

        /// <summary>Full node.</summary>
        private readonly IFullNode fullNode;

        /// <summary>Wallet manager.</summary>
        private readonly IWalletManager walletManager;

        private readonly IAssemblerFactory blockAssemblerFactory;

        private readonly IChainState chainState;

        private readonly NodeDeployments nodeDeployments;

        private readonly MinerSettings minerSettings;

        /// <summary>
        /// Initializes a new instance of the object.
        /// </summary>
        /// <param name="powMining">PoW miner.</param>
        /// <param name="fullNode">Full node to offer mining RPC.</param>
        /// <param name="loggerFactory">Factory to be used to create logger for the node.</param>
        /// <param name="walletManager">The wallet manager.</param>
        /// <param name="posMinting">PoS staker or null if PoS staking is not enabled.</param>
        public MiningRPCController(IPowMining powMining, IFullNode fullNode, ILoggerFactory loggerFactory, IWalletManager walletManager,
            IAssemblerFactory blockAssemblerFactory, MinerSettings minerSettings, IPosMinting posMinting = null) : base(fullNode: fullNode)
        {
            Guard.NotNull(powMining, nameof(powMining));
            Guard.NotNull(fullNode, nameof(fullNode));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNull(walletManager, nameof(walletManager));

            this.fullNode = fullNode;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.walletManager = walletManager;
            this.powMining = powMining;
            this.posMinting = posMinting;
            this.blockAssemblerFactory = blockAssemblerFactory;

            this.minerSettings = this.fullNode.NodeService<MinerSettings>();
            this.chainState = this.fullNode.NodeService<IChainState>();
            this.nodeDeployments = this.fullNode.NodeService<NodeDeployments>();
        }

        /// <summary>
        /// Tries to mine one or more blocks.
        /// </summary>
        /// <param name="blockCount">Number of blocks to mine.</param>
        /// <returns>List of block header hashes of newly mined blocks.</returns>
        /// <remarks>It is possible that less than the required number of blocks will be mined because the generating function only
        /// tries all possible header nonces values.</remarks>
        [ActionName("generate")]
        [ActionDescription("Tries to mine a given number of blocks and returns a list of block header hashes.")]
        public List<uint256> Generate(int blockCount)
        {
            this.logger.LogTrace("({0}:{1})", nameof(blockCount), blockCount);
            if (blockCount <= 0)
            {
                throw new RPCServerException(NBitcoin.RPC.RPCErrorCode.RPC_INVALID_REQUEST, "The number of blocks to mine must be higher than zero.");
            }

            WalletAccountReference accountReference = this.GetAccount();
            HdAddress address = this.walletManager.GetUnusedAddress(accountReference);

            List<uint256> res = this.powMining.GenerateBlocks(new ReserveScript(address.Pubkey), (ulong)blockCount, int.MaxValue);

            this.logger.LogTrace("(-):*.{0}={1}", nameof(res.Count), res.Count);
            return res;
        }

        /// <summary>
        /// Starts staking a wallet.
        /// </summary>
        /// <param name="walletName">The name of the wallet.</param>
        /// <param name="walletPassword">The password of the wallet.</param>
        /// <returns></returns>
        [ActionName("startstaking")]
        [ActionDescription("Starts staking a wallet.")]
        public bool StartStaking(string walletName, string walletPassword)
        {
            Guard.NotEmpty(walletName, nameof(walletName));
            Guard.NotEmpty(walletPassword, nameof(walletPassword));

            this.logger.LogTrace("({0}:{1})", nameof(walletName), walletName);

            Wallet.Wallet wallet = this.walletManager.GetWallet(walletName);

            // Check the password
            try
            {
                Key.Parse(wallet.EncryptedSeed, walletPassword, wallet.Network);
            }
            catch (Exception ex)
            {
                throw new SecurityException(ex.Message);
            }

            this.fullNode.NodeFeature<MiningFeature>(true).StartStaking(walletName, walletPassword);

            return true;
        }

        /// <summary>
        /// Implements "getstakinginfo" RPC call.
        /// </summary>
        /// <param name="isJsonFormat">Indicates whether to provide data in JSON or binary format.</param>
        /// <returns>Staking information RPC response.</returns>
        [ActionName("getstakinginfo")]
        [ActionDescription("Gets the staking information.")]
        public GetStakingInfoModel GetStakingInfo(bool isJsonFormat = true)
        {
            this.logger.LogTrace("({0}:{1})", nameof(isJsonFormat), isJsonFormat);

            if (!isJsonFormat)
            {
                this.logger.LogError("Binary serialization is not supported for RPC '{0}'.", nameof(this.GetStakingInfo));
                throw new NotImplementedException();
            }

            GetStakingInfoModel model = this.posMinting != null ? this.posMinting.GetGetStakingInfoModel() : new GetStakingInfoModel();

            this.logger.LogTrace("(-):{0}", model);
            return model;
        }

        [ActionName("getblocktemplate")]
        [ActionDescription("Get the template for PoW mining blocks")]
        public BlockTemplateResponse GetBlockTemplate(BlockTemplateRequest request)
        {
            var template = this.blockAssemblerFactory.Create(this.chainState.ConsensusTip).CreateNewBlock(BitcoinAddress.Create(this.minerSettings.MineAddress, this.fullNode.Network).ScriptPubKey);
            var blockTemplate = new BlockTemplateResponse
            {
                Version = (uint)template.Block.Header.Version,
                PreviousBlockhash = this.chainState?.ConsensusTip?.HashBlock?.ToString(),
                CoinbaseValue = template.Block.Transactions[0].Outputs[0].Value,
                Target = template.Block.Header.Bits.ToUInt256().ToString(),
                NonceRange = "00000000ffffffff",
                CurTime = (uint)DateTimeOffset.Now.ToUnixTimeSeconds(),
                Bits = template.Block.Header.Bits.ToString(),
                Height = (uint)(this.chainState?.ConsensusTip?.Height + 1 ?? 1),
                Transactions = this.GetTransactions(template),
                CoinbaseAux = this.GetCoinbaseFlags(),
                DefaultWitnessCommitment = this.GetWitnessCommitment(template, request.Rules)
            };

            return blockTemplate;
        }

        private BitcoinBlockTransaction[] GetTransactions(BlockTemplate blockTemplate)
        {
            var transactions = new List<BitcoinBlockTransaction>();

            var i = 0;
            foreach(var tx in blockTemplate.Block.Transactions)
            {
                i++;

                if (tx.IsCoinBase)
                    continue;

                var transaction = new BitcoinBlockTransaction()
                {
                    Data = tx.ToHex(),
                    TxId = this.GetHex(tx.GetHash().ToString()),
                    Hash = this.GetHex(tx.GetWitHash().ToString()),
                    Fee = blockTemplate.VTxFees[i - 1].ToDecimal(MoneyUnit.Satoshi)
                };

                transactions.Add(transaction);
            }

            return transactions.ToArray();
        }

        private CoinbaseAux GetCoinbaseFlags()
        {
            if (this.chainState == null || this.chainState.ConsensusTip == null)
                return null;

            var flagsString = this.nodeDeployments.GetFlags(this.chainState.ConsensusTip).ScriptFlags.ToString();
            var flagsHex = this.GetHex(flagsString);

            var aux = new CoinbaseAux()
            {
                Flags = flagsHex
            };

            return aux;
        }

        private string GetWitnessCommitment(BlockTemplate template, string[] rules)
        {
            if (rules == null || rules.Length == 0)
                return null;

            if (!string.IsNullOrEmpty(template.CoinbaseCommitment) && rules.Any(x => x.Contains(BIP9Deployments.Segwit.ToString(), StringComparison.InvariantCultureIgnoreCase)))
            {
                // CoinbaseCommitment not yet implemented in Stratis codebase
                return this.GetHex(template.CoinbaseCommitment);
            }

            return null;
        }

        private string GetHex(string value)
        {
            var bytes = Encoding.Default.GetBytes(value);
            var hexString = Encoders.Hex.EncodeData(bytes);
            return hexString;
        }

        /// <summary>
        /// Finds first available wallet and its account.
        /// </summary>
        /// <returns>Reference to wallet account.</returns>
        private WalletAccountReference GetAccount()
        {
            this.logger.LogTrace("()");

            string walletName = this.walletManager.GetWalletsNames().FirstOrDefault();
            if (walletName == null)
                throw new RPCServerException(NBitcoin.RPC.RPCErrorCode.RPC_INVALID_REQUEST, "No wallet found");

            HdAccount account = this.walletManager.GetAccounts(walletName).FirstOrDefault();
            if (account == null)
                throw new RPCServerException(NBitcoin.RPC.RPCErrorCode.RPC_INVALID_REQUEST, "No account found on wallet");

            var res = new WalletAccountReference(walletName, account.Name);

            this.logger.LogTrace("(-):'{0}'", res);
            return res;
        }
    }
}
