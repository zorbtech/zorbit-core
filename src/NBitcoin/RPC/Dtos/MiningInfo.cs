namespace NBitcoin.RPC.Dtos
{
    public class MiningInfo
    {
        public int Blocks { get; set; }
        public long CurrentBlockSize { get; set; }
        public long CurrentBlockWeight { get; set; }
        public double Difficulty { get; set; }
        public double NetworkHashps { get; set; }
        public string Chain { get; set; }
    }
}
