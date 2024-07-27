using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace VotingSystemLib
{
    public partial class VotingSystem : SmartContract, IVotingContract
    {
        public const string RemoteUrl = "https://raw.githubusercontent.com/3Matvey/Voting-System-Web3-DApp-/main/contract/build/contracts/Voting.json";

        public int CandidatesCount { get => _candidatesCount; }
        private int _candidatesCount; 

        private async Task<int> CandidatesCountCall()
        {
            return await CallFunctionAsync<int>("candidatesCount");
        }  

        public async Task AddCandidate(string name, string description)
        {
            await SendTransactionAsOwnerAsync("addCandidate", name, description);
            _candidatesCount = await CandidatesCountCall();
        }

        public async Task StartVoting(uint minutes)
        {
            await SendTransactionAsOwnerAsync("startVoting", minutes);
        }

        public async Task EndVoting()
        {
            await SendTransactionAsOwnerAsync("endVoting");
        }

        public async Task Vote(uint candidateID, string signerPrivateKey)
        {
            await SendTransactionAsUserAsync("vote", signerPrivateKey, candidateID);
        }

        public async Task<(bool, uint, uint, uint)> GetVotingStatus()
        {
            return await CallFunctionAsync<(bool, uint, uint, uint)>("getVotingStatus");
        }

        public async Task<bool> VerifyVoter(string address)
        {
            return await CallFunctionAsync<bool>("verifyVoter", address);
        }


        public async Task<IVotingContract.Candidate> GetCandidate(uint candidateId)
        {
            return await CallFunctionAsync<Candidate>("getCandidate", candidateId);
        }
        

        [FunctionOutput]
        public class Candidate : IVotingContract.Candidate
        {
            [Parameter("uint", "id", 1)]
            public uint Id { get; set; }

            [Parameter("string", "name", 2)]
            public string Name { get; set; }

            [Parameter("string", "description", 3)]
            public string Description { get; set; }

            [Parameter("uint", "voteCount", 4)]
            public uint VoteCount { get; set; }
        }
    }
}
