using System;
using System.Collections.Generic;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;

namespace NBitcoin
{
    public sealed class ZorbitRegTest : AbstractZorbitNetwork
    {
        public ZorbitRegTest()
        {
            var messageStart = new byte[4];
            messageStart[0] = 0xdd;
            messageStart[1] = 0xd6;
            messageStart[2] = 0xea;
            messageStart[3] = 0xf7;

            this.Name = "ZorbitRegTest";
            this.RootFolderName = ZorbitRootFolderName;
            this.DefaultConfigFilename = ZorbitDefaultConfigFilename;
            this.Magic = BitConverter.ToUInt32(messageStart, 0);
            this.DefaultPort = 37777;
            this.RPCPort = 37778;
            this.MaxTimeOffsetSeconds = ZorbitMaxTimeOffsetSeconds;
            this.MaxTipAge = ZorbitDefaultMaxTipAgeInSeconds;
            this.MinTxFee = 10000;
            this.FallbackFee = 60000;
            this.MinRelayTxFee = 10000;
            this.Consensus = new Consensus
            {
                MajorityEnforceBlockUpgrade = 750,
                MajorityRejectBlockOutdated = 950,
                MajorityWindow = 1000
            };

            this.Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 0;
            this.Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 0;
            this.Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 0;
            this.Consensus.BIP34Hash = new uint256("0x301b0f400afd80b21830101ca2bf847a6a56e8b6ff99e2320798c452c34f6c3b");
            this.Consensus.PowLimit = new Target(new uint256("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            this.Consensus.PowTargetTimespan = TimeSpan.FromMinutes(10);
            this.Consensus.PowTargetSpacing = TimeSpan.FromMinutes(2);
            this.Consensus.PowAllowMinDifficultyBlocks = true;
            this.Consensus.PowNoRetargeting = true;
            this.Consensus.RuleChangeActivationThreshold = 684; // 95% of 720
            this.Consensus.MinerConfirmationWindow = 720; // nPowTargetTimespan / nPowTargetSpacing
            this.Consensus.LastPOWBlock = 788400; // 3 Years
            this.Consensus.IsProofOfStake = true;
            this.Consensus.ConsensusFactory = new PowPosConsensusFactory() { Consensus = this.Consensus };
            this.Consensus.ProofOfStakeLimit = new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            this.Consensus.ProofOfStakeLimitV2 = new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false));
            this.Consensus.CoinType = 177;
            this.Consensus.DefaultAssumeValid = null; // turn off assumevalid for regtest.

            Block genesis = CreateZorbitGenesisBlock(this.Consensus.ConsensusFactory, 1515944103, 33395447, this.Consensus.PowLimit, 1, Money.Zero);
            genesis.Header.Time = 1515944354;
            genesis.Header.Nonce = 8;
            genesis.Header.Bits = this.Consensus.PowLimit;
            this.Genesis = genesis;

            this.Consensus.HashGenesisBlock = genesis.GetHash();
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x301b0f400afd80b21830101ca2bf847a6a56e8b6ff99e2320798c452c34f6c3b"));

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x301b0f400afd80b21830101ca2bf847a6a56e8b6ff99e2320798c452c34f6c3b"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }
            };

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

            var encoder = new Bech32Encoder("zrr");
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;
        }
    }
}