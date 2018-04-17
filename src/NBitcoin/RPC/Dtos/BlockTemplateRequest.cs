namespace NBitcoin.RPC.Dtos
{
    public class BlockTemplateRequest
    {
        public BlockTemplateRequestMode Mode { get; set; }
        public string[] Capabilities { get; set; }
        public string[] Rules { get; set; }
        public string Data { get; set; }
    }

    public enum BlockTemplateRequestMode
    {
        Template,
        Proposal,
        Submit
    }
}
