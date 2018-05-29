using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;

namespace NBitcoin
{
    public partial class Network
    {
        /// <summary> The name of the root folder containing the different Zorbit blockchains (ZorbitMain, ZorbitTest, ZorbitRegTest). </summary>
        public const string ZorbitRootFolderName = "zorbit";

        /// <summary> The default name used for the Stratis configuration file. </summary>
        public const string ZorbitDefaultConfigFilename = "zorbit.conf";

        /// <summary> Zorbit maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int ZorbitMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Zorbit default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int ZorbitDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        public static Network ZorbitMain => Network.GetNetwork("ZorbitMain") ?? InitZorbitMain();

        public static Network ZorbitTest => Network.GetNetwork("ZorbitTest") ?? InitZorbitTest();

        public static Network ZorbitRegTest => Network.GetNetwork("ZorbitRegTest") ?? InitZorbitRegTest();

        internal static Network InitZorbitMain()
        {
            var messageStart = new byte[4];
            messageStart[0] = 0x11;
            messageStart[1] = 0x10;
            messageStart[2] = 0x19;
            messageStart[3] = 0x07;
            var magic = BitConverter.ToUInt32(messageStart, 0);

            var network = new Network
            {
                Name = "ZorbitMain",
                RootFolderName = ZorbitRootFolderName,
                DefaultConfigFilename = ZorbitDefaultConfigFilename,
                Magic = magic,
                DefaultPort = 17777,
                RPCPort = 17778,
                MinTxFee = 10000,
                FallbackFee = 60000,
                MinRelayTxFee = 10000,
                MaxTimeOffsetSeconds = ZorbitMaxTimeOffsetSeconds,
                MaxTipAge = ZorbitDefaultMaxTipAgeInSeconds,
                Consensus =
                {
                    MajorityEnforceBlockUpgrade = 750,
                    MajorityRejectBlockOutdated = 950,
                    MajorityWindow = 1000
                }
            };

            network.Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 0;
            network.Consensus.BIP34Hash = new uint256("0x000000252806976858281f397637f0d063743dfda42ccba1a995e5d30e359716");
            network.Consensus.PowLimit = new Target(new uint256("000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            network.Consensus.PowTargetTimespan = TimeSpan.FromMinutes(10);
            network.Consensus.PowTargetSpacing = TimeSpan.FromMinutes(2);
            network.Consensus.PowAllowMinDifficultyBlocks = false;
            network.Consensus.PowNoRetargeting = false;
            network.Consensus.RuleChangeActivationThreshold = 684; // 95% of 720
            network.Consensus.MinerConfirmationWindow = 720; // nPowTargetTimespan / nPowTargetSpacing
            network.Consensus.LastPOWBlock = 788400; // 3 Years
            network.Consensus.IsProofOfStake = true;
            network.Consensus.ConsensusFactory = new PowPosConsensusFactory() { Consensus = network.Consensus };
            network.Consensus.ProofOfStakeLimit = new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            network.Consensus.ProofOfStakeLimitV2 = new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            network.Consensus.CoinType = 177;
            network.Consensus.DefaultAssumeValid = new uint256("0x000000252806976858281f397637f0d063743dfda42ccba1a995e5d30e359716"); // 0

            network.genesis = CreateZorbitGenesisBlock(network.Consensus.ConsensusFactory, 1515944103, 33395447, network.Consensus.PowLimit, 1, Money.Zero);
            network.Consensus.HashGenesisBlock = network.genesis.GetHash();

            Assert(network.Consensus.HashGenesisBlock == uint256.Parse("0x000000252806976858281f397637f0d063743dfda42ccba1a995e5d30e359716"));
            Assert(network.genesis.Header.HashMerkleRoot == uint256.Parse("0xe0c01fb7ea26f7de5cc362056b01fd8de036c1a166d355e6f07bbf2dfab1c4ee"));

            network.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x000000252806976858281f397637f0d063743dfda42ccba1a995e5d30e359716"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }
            };

            network.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (142) };
            network.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (80) };
            network.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (63 + 128) };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            network.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            network.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            network.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            network.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            network.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2d };
            network.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            network.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            var encoder = new Bech32Encoder("zrb");
            network.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            network.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

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

            Register(network);

            return network;
        }

        internal static Network InitZorbitTest()
        {
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            var messageStart = new byte[4];
            messageStart[0] = 0x16;
            messageStart[1] = 0x24;
            messageStart[2] = 0x43;
            messageStart[3] = 0x04;
            var magic = BitConverter.ToUInt32(messageStart, 0); //0x5223570; 

            var network = new Network
            {
                Name = "ZorbitTest",
                RootFolderName = ZorbitRootFolderName,
                DefaultConfigFilename = ZorbitDefaultConfigFilename,
                Magic = magic,
                DefaultPort = 27777,
                RPCPort = 27778,
                MaxTimeOffsetSeconds = ZorbitMaxTimeOffsetSeconds,
                MaxTipAge = ZorbitDefaultMaxTipAgeInSeconds,
                MinTxFee = 10000,
                FallbackFee = 60000,
                MinRelayTxFee = 10000,
                Consensus =
                {
                    MajorityEnforceBlockUpgrade = 750,
                    MajorityRejectBlockOutdated = 950,
                    MajorityWindow = 1000
                }
            };

            network.Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 0;
            network.Consensus.BIP34Hash = new uint256("0x00000a1d7540dd6d82f381c9098587cbf553ac21d41cd34db17787a01f557158");
            network.Consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            network.Consensus.PowTargetTimespan = TimeSpan.FromMinutes(10);
            network.Consensus.PowTargetSpacing = TimeSpan.FromMinutes(2);
            network.Consensus.PowAllowMinDifficultyBlocks = false;
            network.Consensus.PowNoRetargeting = false;
            network.Consensus.RuleChangeActivationThreshold = 684; // 95% of 720
            network.Consensus.MinerConfirmationWindow = 720; // nPowTargetTimespan / nPowTargetSpacing
            network.Consensus.LastPOWBlock = 788400; // 3 Years
            network.Consensus.IsProofOfStake = true;
            network.Consensus.ConsensusFactory = new PowPosConsensusFactory() { Consensus = network.Consensus };
            network.Consensus.ProofOfStakeLimit = new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            network.Consensus.ProofOfStakeLimitV2 = new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            network.Consensus.CoinType = 177;
            network.Consensus.DefaultAssumeValid = new uint256("0x00000a1d7540dd6d82f381c9098587cbf553ac21d41cd34db17787a01f557158"); // 0

            Block genesis = CreateZorbitGenesisBlock(network.Consensus.ConsensusFactory, 1515944103, 33395447, network.Consensus.PowLimit, 1, Money.Zero);
            genesis.Header.Time = 1515944095;
            genesis.Header.Nonce = 860806;
            genesis.Header.Bits = network.Consensus.PowLimit;
            network.genesis = genesis;
            network.Consensus.HashGenesisBlock = genesis.GetHash();
            Assert(network.Consensus.HashGenesisBlock == uint256.Parse("0x00000a1d7540dd6d82f381c9098587cbf553ac21d41cd34db17787a01f557158"));

            network.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x00000a1d7540dd6d82f381c9098587cbf553ac21d41cd34db17787a01f557158"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }
            };

            network.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (127) };
            network.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (65) };
            network.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (63 + 128) };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            network.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            network.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            network.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            network.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            network.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2e };
            network.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            network.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            var encoder = new Bech32Encoder("zrt");
            network.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            network.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

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

            Register(network);

            return network;
        }

        internal static Network InitZorbitRegTest()
        {
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            var messageStart = new byte[4];
            messageStart[0] = 0xdd;
            messageStart[1] = 0xd6;
            messageStart[2] = 0xea;
            messageStart[3] = 0xf7;
            var magic = BitConverter.ToUInt32(messageStart, 0);

            var network = new Network
            {
                Name = "ZorbitRegTest",
                RootFolderName = ZorbitRootFolderName,
                DefaultConfigFilename = ZorbitDefaultConfigFilename,
                Magic = magic,
                DefaultPort = 37777,
                RPCPort = 37778,
                MaxTimeOffsetSeconds = ZorbitMaxTimeOffsetSeconds,
                MaxTipAge = ZorbitDefaultMaxTipAgeInSeconds,
                MinTxFee = 10000,
                FallbackFee = 60000,
                MinRelayTxFee = 10000,
                Consensus =
                {
                    MajorityEnforceBlockUpgrade = 750,
                    MajorityRejectBlockOutdated = 950,
                    MajorityWindow = 1000
                }
            };

            network.Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 0;
            network.Consensus.BIP34Hash = new uint256("0x301b0f400afd80b21830101ca2bf847a6a56e8b6ff99e2320798c452c34f6c3b");
            network.Consensus.PowLimit = new Target(new uint256("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            network.Consensus.PowTargetTimespan = TimeSpan.FromMinutes(10);
            network.Consensus.PowTargetSpacing = TimeSpan.FromMinutes(2);
            network.Consensus.PowAllowMinDifficultyBlocks = true;
            network.Consensus.PowNoRetargeting = true;
            network.Consensus.RuleChangeActivationThreshold = 684; // 95% of 720
            network.Consensus.MinerConfirmationWindow = 720; // nPowTargetTimespan / nPowTargetSpacing
            network.Consensus.LastPOWBlock = 788400; // 3 Years
            network.Consensus.IsProofOfStake = true;
            network.Consensus.ConsensusFactory = new PowPosConsensusFactory() { Consensus = network.Consensus };
            network.Consensus.ProofOfStakeLimit = new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            network.Consensus.ProofOfStakeLimitV2 = new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            network.Consensus.CoinType = 177;
            network.Consensus.DefaultAssumeValid = null; // turn off assumevalid for regtest.

            Block genesis = CreateZorbitGenesisBlock(network.Consensus.ConsensusFactory, 1515944103, 33395447, network.Consensus.PowLimit, 1, Money.Zero);
            genesis.Header.Time = 1515944354;
            genesis.Header.Nonce = 8;
            genesis.Header.Bits = network.Consensus.PowLimit;
            network.genesis = genesis;
            network.Consensus.HashGenesisBlock = genesis.GetHash();
            Assert(network.Consensus.HashGenesisBlock == uint256.Parse("0x301b0f400afd80b21830101ca2bf847a6a56e8b6ff99e2320798c452c34f6c3b"));

            network.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x301b0f400afd80b21830101ca2bf847a6a56e8b6ff99e2320798c452c34f6c3b"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }
            };

            network.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (127) };
            network.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (65) };
            network.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (63 + 128) };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            network.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            network.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            network.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            network.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            network.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2e };
            network.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            network.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            var encoder = new Bech32Encoder("zrr");
            network.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            network.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            Register(network);

            return network;
        }

        private static Block CreateZorbitGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
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
            Stopwatch sw = new Stopwatch();

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
