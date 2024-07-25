using Nethereum.ABI.FunctionEncoding.Attributes;

namespace VotingSystemLib
{
    public class CandidateAddedEventArgs : EventArgs
    {
        [Parameter("uint", "candidateID", 1)]
        public uint CandidateID { get; set; }

        [Parameter("string", "name", 2)]
        public string Name { get; set; } = string.Empty;

        [Parameter("string", "description", 3)]
        public string Description { get; set; } = string.Empty;
    }

    public class VotedEventArgs : EventArgs
    {
        [Parameter("address", "voter", 1, true)]
        public string Voter { get; set; } = string.Empty;

        [Parameter("uint", "candidateID", 2)]
        public uint CandidateID { get; set; }
    }

    public class VotingStartedEventArgs : EventArgs
    {
        [Parameter("uint", "startTime", 1)]
        public uint StartTime { get; set; }

        [Parameter("uint", "durationMinutes", 2)]
        public uint DurationMinutes { get; set; }
    }

    public class VotingEndedEventArgs : EventArgs
    {
        [Parameter("uint", "endTime", 1)]
        public uint EndTime { get; set; }
    }

    public class WinnerDeclaredEventArgs : EventArgs
    {
        [Parameter("uint", "candidateID", 1)]
        public uint CandidateID { get; set; }

        [Parameter("string", "name", 2)]
        public string Name { get; set; } = string.Empty;

        [Parameter("string", "description", 3)]
        public string Description { get; set; } = string.Empty;

        [Parameter("uint", "voteCount", 4)]
        public uint VoteCount { get; set; }

        [Parameter("string", "percent", 5)]
        public string Percent { get; set; } = string.Empty;
    }
}
