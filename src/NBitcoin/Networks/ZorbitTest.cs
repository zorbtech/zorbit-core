using System;
using System.Collections.Generic;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;

namespace NBitcoin
{
    public sealed class ZorbitTest : ZorbitNetwork
    {
        public ZorbitTest()
        {
            var messageStart = new byte[4];
            messageStart[0] = 0x16;
            messageStart[1] = 0x24;
            messageStart[2] = 0x43;
            messageStart[3] = 0x04;

            this.Name = "ZorbitTest";
            this.RootFolderName = ZorbitRootFolderName;
            this.DefaultConfigFilename = ZorbitDefaultConfigFilename;
            this.Magic = BitConverter.ToUInt32(messageStart, 0); //0x5223570; 
            this.DefaultPort = 27777;
            this.RPCPort = 27778;
            this.MaxTimeOffsetSeconds = ZorbitMaxTimeOffsetSeconds;
            this.MaxTipAge = ZorbitDefaultMaxTipAgeInSeconds;
            this.MinTxFee = 10000;
            this.FallbackFee = 60000;
            this.MinRelayTxFee = 10000;

            this.Consensus.MajorityEnforceBlockUpgrade = 750;
            this.Consensus.MajorityRejectBlockOutdated = 950;
            this.Consensus.MajorityWindow = 1000;
            this.Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 0;
            this.Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 0;
            this.Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 0;
            this.Consensus.BIP34Hash = new uint256("0x00000a1d7540dd6d82f381c9098587cbf553ac21d41cd34db17787a01f557158");
            this.Consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            this.Consensus.PowTargetTimespan = TimeSpan.FromMinutes(10);
            this.Consensus.PowTargetSpacing = TimeSpan.FromMinutes(2);
            this.Consensus.PowAllowMinDifficultyBlocks = false;
            this.Consensus.PowNoRetargeting = false;
            this.Consensus.RuleChangeActivationThreshold = 684; // 95% of 720
            this.Consensus.MinerConfirmationWindow = 720; // nPowTargetTimespan / nPowTargetSpacing
            this.Consensus.LastPOWBlock = 788400; // 3 Years
            this.Consensus.IsProofOfStake = true;
            this.Consensus.ConsensusFactory = new PowPosConsensusFactory() { Consensus = Consensus };
            this.Consensus.ProofOfStakeLimit = new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            this.Consensus.ProofOfStakeLimitV2 = new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            this.Consensus.CoinType = 177;
            this.Consensus.DefaultAssumeValid = new uint256("0x00000a1d7540dd6d82f381c9098587cbf553ac21d41cd34db17787a01f557158"); // 0

            Block genesis = CreateZorbitGenesisBlock(this.Consensus.ConsensusFactory, 1515944103, 33395447, this.Consensus.PowLimit, 1, Money.Zero);
            genesis.Header.Time = 1515944095;
            genesis.Header.Nonce = 860806;
            genesis.Header.Bits = this.Consensus.PowLimit;
            this.Genesis = genesis;

            this.Consensus.HashGenesisBlock = genesis.GetHash();
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x00000a1d7540dd6d82f381c9098587cbf553ac21d41cd34db17787a01f557158"));

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x00000a1d7540dd6d82f381c9098587cbf553ac21d41cd34db17787a01f557158"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }
            };

            this.DNSSeeds = new List<DNSSeedData>();
            this.SeedNodes = new List<NetworkAddress>();

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (127) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (65) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (63 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2e };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            var encoder = new Bech32Encoder("zrt");
            this.Bech32Encoders = new Bech32Encoder[2];
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            //DNSSeeds.AddRange(new[]
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
            //        Endpoint = Utils.ParseIpEndpoint(seed, DefaultPort)
            //    };

            //    SeedNodes.Add(addr);
            //}
        }
    }
}