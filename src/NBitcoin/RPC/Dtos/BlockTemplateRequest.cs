namespace NBitcoin.RPC.Dtos
{
    public class BlockTemplateRequest
    {
        public string[] Capabilities { get; set; }
        public string[] Rules { get; set; }
    }
}
