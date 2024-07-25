global using EthereumIntegration;

namespace VotingSystemLib
{
    public partial class VotingSystem : SmartContract, IVotingContract
    {
        private static VotingSystem? instance;
        private static object syncRoot = new();

        private VotingSystem(string url, string abiPath, string contractAddress, string accountAddress, string privateKey)
            : base(url, abiPath, contractAddress, accountAddress, privateKey) { }

        public static VotingSystem GetInstance(in string url, in string abiPath, in string contractAddress, in string accountAddress, in string privateKey)
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    instance ??= new VotingSystem(url, abiPath, contractAddress, accountAddress, privateKey);
                }
            }
            return instance;
        }
    }
}
