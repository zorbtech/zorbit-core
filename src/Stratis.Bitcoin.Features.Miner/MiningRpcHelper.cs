namespace Stratis.Bitcoin.Features.Miner
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using NBitcoin;
    using NBitcoin.DataEncoders;

    public class MiningRpcHelper
    {
        public double CalculateNetworkHashps(ChainedHeader tip, ChainBase chain, long difficultyAdjustmentInterval, long lookup, int height)
        {
            if (height >= 0 && height < tip?.Height)
                tip = chain.GetBlock(height);

            if (tip == null || tip.Height == 0)
                return 0;

            if (lookup <= 0)
                lookup = tip.Height % difficultyAdjustmentInterval + 1;

            if (lookup > tip.Height)
                lookup = tip.Height;

            var minTime = tip.Header.BlockTime.ToUnixTimeSeconds();
            var maxTime = minTime;

            var lookupBlock = CloneBlock(tip);

            // is there a more efficient way to skip back a specified number of blocks??
            for (var i = 0; i < lookup; i++)
            {
                lookupBlock = lookupBlock.Previous;
                var time = lookupBlock.Header.BlockTime.ToUnixTimeSeconds();
                minTime = Math.Min(time, minTime);
                maxTime = Math.Max(time, maxTime);
            }

            if (minTime.Equals(maxTime))
                return 0;

            var workDiff = tip.ChainWork.GetLow64() - lookupBlock.ChainWork.GetLow64();
            var timeDiff = maxTime - minTime;

            var hashPs = double.Parse(workDiff.ToString()) / timeDiff;

            return hashPs;
        }

        public string GetHex(string value)
        {
            var bytes = Encoding.Default.GetBytes(value);
            var hexString = Encoders.Hex.EncodeData(bytes);
            return hexString;
        }

        public byte[] GetBytesFromHex(string hex)
        {
            return Encoders.Hex.DecodeData(hex);
        }

        public ChainedHeader CloneBlock(ChainedHeader pb)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, pb);
                stream.Seek(0, SeekOrigin.Begin);
                return (ChainedHeader)formatter.Deserialize(stream);
            }
        }

    }
}
