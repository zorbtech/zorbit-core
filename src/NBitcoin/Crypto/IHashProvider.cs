namespace NBitcoin.Crypto
{
    public interface IHashProvider
    {
        uint256 Hash(byte[] data);
    }
}