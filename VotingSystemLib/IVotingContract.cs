namespace VotingSystemLib
{
    public interface IVotingContract
    {
        /// <summary>
        /// Adding a new candidate by the administrator.
        /// </summary>
        Task AddCandidate(string name, string description);

        /// <summary>
        /// Start of voting by the administrator.
        /// </summary>
        /// <param name="durationMinutes">Duration of voting in minutes.</param>
        Task StartVoting(uint durationMinutes);

        /// <summary>
        /// Voting for a candidate.
        /// </summary>
        Task Vote(uint candidateID, string signerPrivateKey);

        Task<(bool isVotingActive, uint startTime, uint endTime, uint totalVotesCount)> GetVotingStatus();

        /// <summary>
        /// Checking whether the specified address voted.
        /// </summary>
        /// <param name="address">Voter Address.</param>
        /// <returns>Returns true if already voted.</returns>
        Task<bool> VerifyVoter(string address);

        /// <summary>
        /// Completion of the vote by the administrator.
        /// </summary>
        Task EndVoting();

        /// <summary>
        /// Obtaining information about a candidate by their ID.
        /// </summary>
        /// <returns>Returns information about the candidate.</returns>
        Task<Candidate> GetCandidate(uint candidateID);

        class Candidate;
    }
}
