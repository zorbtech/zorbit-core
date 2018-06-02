using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;

namespace NBitcoin
{
    public sealed class ZorbitMain : ZorbitNetwork
    {
        public ZorbitMain()
        {
            var messageStart = new byte[4];
            messageStart[0] = 0x11;
            messageStart[1] = 0x10;
            messageStart[2] = 0x19;
            messageStart[3] = 0x07;

            this.Name = "ZorbitMain";
            this.RootFolderName = ZorbitRootFolderName;
            this.DefaultConfigFilename = ZorbitDefaultConfigFilename;
            this.Magic = BitConverter.ToUInt32(messageStart, 0);
            this.DefaultPort = 17777;
            this.RPCPort = 17778;
            this.MinTxFee = 10000;
            this.FallbackFee = 60000;
            this.MinRelayTxFee = 10000;
            this.MaxTimeOffsetSeconds = ZorbitMaxTimeOffsetSeconds;
            this.MaxTipAge = ZorbitDefaultMaxTipAgeInSeconds;
            this.Consensus = new Consensus
            {
                MajorityEnforceBlockUpgrade = 750,
                MajorityRejectBlockOutdated = 950,
                MajorityWindow = 1000
            };

            this.Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 0;
            this.Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 0;
            this.Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 0;
            this.Consensus.BIP34Hash = new uint256("0x000000252806976858281f397637f0d063743dfda42ccba1a995e5d30e359716");
            this.Consensus.PowLimit = new Target(new uint256("000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            this.Consensus.PowTargetTimespan = TimeSpan.FromMinutes(10);
            this.Consensus.PowTargetSpacing = TimeSpan.FromMinutes(2);
            this.Consensus.PowAllowMinDifficultyBlocks = false;
            this.Consensus.PowNoRetargeting = false;
            this.Consensus.RuleChangeActivationThreshold = 684; 
            this.Consensus.MinerConfirmationWindow = 720; 
            this.Consensus.LastPOWBlock = 788400; 
            this.Consensus.IsProofOfStake = true;
            this.Consensus.ConsensusFactory = new PowPosConsensusFactory() { Consensus = this.Consensus };
            this.Consensus.ProofOfStakeLimit = new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            this.Consensus.ProofOfStakeLimitV2 = new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            this.Consensus.CoinType = 177;
            this.Consensus.DefaultAssumeValid = new uint256("0x000000252806976858281f397637f0d063743dfda42ccba1a995e5d30e359716"); // 0

            this.Genesis = CreateZorbitGenesisBlock(this.Consensus.ConsensusFactory, 1515944103, 33395447, this.Consensus.PowLimit, 1, Money.Zero);
            this.Consensus.HashGenesisBlock = this.Genesis.GetHash();

            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x000000252806976858281f397637f0d063743dfda42ccba1a995e5d30e359716"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0xe0c01fb7ea26f7de5cc362056b01fd8de036c1a166d355e6f07bbf2dfab1c4ee"));

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x000000252806976858281f397637f0d063743dfda42ccba1a995e5d30e359716"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }
            };

            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (142) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (80) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (63 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2d };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            var encoder = new Bech32Encoder("zrb");
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            //network.DNSSeeds.AddRange(new[]
            //{
            //    new DNSSeedData("seed1..", "seed1..")
            //});

            //var seeds = new[] { "0.0.0.0" };
            //// Convert the seeds array into usable address objects.
            //var rand = new Random();
            //TimeSpan oneWeek = TimeSpan.FromDays(7);
            //foreach (string seed in seeds)
            //{
            //    // It'll only connect to one or two seed nodes because once it connects,
            //    // it'll get a pile of addresses with newer timestamps.
            //    // Seed nodes are given a random 'last seen time' of between one and two weeks ago.
            //    var addr = new NetworkAddress
            //    {
            //        Time = DateTime.UtcNow - (TimeSpan.FromSeconds(rand.NextDouble() * oneWeek.TotalSeconds)) - oneWeek,
            //        Endpoint = Utils.ParseIpEndpoint(seed, network.DefaultPort)
            //    };

            //    network.SeedNodes.Add(addr);
            //}
        }
    }

    public partial class Network
    {
        public static Network ZorbitMain => GetNetwork("ZorbitMain") ?? Register(new ZorbitMain());

        public static Network ZorbitTest => GetNetwork("ZorbitTest") ?? Register(new ZorbitTest());

        public static Network ZorbitRegTest => GetNetwork("ZorbitRegTest") ?? Register(new ZorbitRegTest());
    }

    public abstract class ZorbitNetwork : Network
    {        
        /// <summary> The name of the root folder containing the different Zorbit blockchains (ZorbitMain, ZorbitTest, ZorbitRegTest). </summary>
        protected const string ZorbitRootFolderName = "zorbit";

        /// <summary> The default name used for the Stratis configuration file. </summary>
        protected const string ZorbitDefaultConfigFilename = "zorbit.conf";

        /// <summary> Zorbit maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        protected const int ZorbitMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Zorbit default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        protected const int ZorbitDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        protected static Block CreateZorbitGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            string pszTimestamp = "Hawaii in shock after false missile alert";
            return CreateZorbitGenesisBlock(consensusFactory, pszTimestamp, nTime, nNonce, nBits, nVersion, genesisReward);
        }

        private static Block CreateZorbitGenesisBlock(ConsensusFactory consensusFactory, string pszTimestamp, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = 1;
            txNew.Time = nTime;
            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(0), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)42 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(pszTimestamp)))
            });
            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
            });
            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
            genesis.Header.Bits = nBits;
            genesis.Header.Nonce = nNonce;
            genesis.Header.Version = nVersion;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();
            return genesis;
        }

        public static uint CalculateProofOfWork(BlockHeader header, Consensus consensus)
        {
            uint nonce = 0;
            int count = 0;
            var sw = new Stopwatch();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 4
            };

            sw.Start();

            Parallel.ForEach(Enumerable.Range(0, int.MaxValue), options, (i, state) =>
            {
                BlockHeader tmp = header.Clone();
                tmp.Nonce = (uint)i;
                count++;
                if (state.IsStopped || state.ShouldExitCurrentIteration || !tmp.CheckProofOfWork(consensus))
                {
                    if (sw.ElapsedMilliseconds < 1000)
                    {
                        return;
                    }

                    Console.WriteLine("{0} H/s", count);
                    count = 0;
                    sw.Restart();
                    return;
                }
                sw.Stop();
                nonce = tmp.Nonce;
                state.Break();
            });

            return nonce;
        }
    }
}
