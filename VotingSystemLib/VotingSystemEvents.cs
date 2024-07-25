namespace VotingSystemLib
{
    public partial class VotingSystem : SmartContract, IVotingContract
    {
        EventHandler<CandidateAddedEventArgs>? _candidateAdded;
        EventHandler<VotedEventArgs>? _voted;
        EventHandler<VotingStartedEventArgs>? _votingStarted;
        EventHandler<VotingEndedEventArgs>? _votingEnded;
        EventHandler<WinnerDeclaredEventArgs>? _winnerDeclared;

        public event EventHandler<CandidateAddedEventArgs> CandidateAdded
        {
            add => _candidateAdded += value;
            remove => _candidateAdded -= value;
        }
        public event EventHandler<VotedEventArgs> Voted
        {
            add => _voted += value;
            remove => _voted -= value;
        }
        public event EventHandler<VotingStartedEventArgs> VotingStarted
        {
            add => _votingStarted += value;
            remove => _votingStarted -= value;
        }
        public event EventHandler<VotingEndedEventArgs> VotingEnded
        {
            add => _votingEnded += value;
            remove => _votingEnded -= value;
        }
        public event EventHandler<WinnerDeclaredEventArgs> WinnerDeclared
        {
            add => _winnerDeclared += value;
            remove => _winnerDeclared -= value;
        }

        public async Task StartListeningCandidateAddedEventAsync()
        {
            if(_candidateAdded != null)
                await StartListeningEventAsync("CandidateAdded", _candidateAdded);
        }

        public async Task StartListeningCandidateAddedEventAsync(CancellationToken cancellationToken)
        {
            if(_candidateAdded != null) 
                await StartListeningEventAsync("CandidateAdded", _candidateAdded, cancellationToken);
        }

        
        public async Task StartListeningVotedEventAsync()
        {
            if(_voted != null)
                await StartListeningEventAsync("Voted", _voted);
        }

        public async Task StartListeningVotedEventAsync(CancellationToken cancellationToken)
        {
            if(_voted != null)
                await StartListeningEventAsync("Voted", _voted, cancellationToken);
        }

       
        public async Task StartListeningVotingStartedEventAsync()
        {
            if(_votingStarted != null)
                await StartListeningEventAsync("VotingStarted", _votingStarted);
        }

        public async Task StartListeningVotingStartedEventAsync(CancellationToken cancellationToken)
        {
            if(_votingStarted != null)
                await StartListeningEventAsync("VotingStarted", _votingStarted, cancellationToken);
        }

        
        public async Task StartListeningVotingEndedEventAsync()
        {
            if(_votingEnded != null)
                await StartListeningEventAsync("VotingEnded", _votingEnded);
        }

        public async Task StartListeningVotingEndedEventAsync(CancellationToken cancellationToken)
        {
            if( _votingEnded != null)
                await StartListeningEventAsync("VotingEnded", _votingEnded, cancellationToken);
        }

        
        public async Task StartListeningWinnerDeclaredEventAsync()
        {
            if( _winnerDeclared != null)
                await StartListeningEventAsync("WinnerDeclared", _winnerDeclared);
        }

        public async Task StartListeningWinnerDeclaredEventAsync(CancellationToken cancellationToken)
        {
            if( _winnerDeclared != null)
                await StartListeningEventAsync("WinnerDeclared", _winnerDeclared, cancellationToken);
        }

        public async Task StartListeningAllEventsAsync()
        {
            await StartListeningCandidateAddedEventAsync();
            await StartListeningVotedEventAsync();
            await StartListeningVotingStartedEventAsync();
            await StartListeningVotingEndedEventAsync();
            await StartListeningWinnerDeclaredEventAsync();
        }

        public async Task StartListeningAllEventsAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                StartListeningCandidateAddedEventAsync(cancellationToken),
                StartListeningVotedEventAsync(cancellationToken),
                StartListeningVotingStartedEventAsync(cancellationToken),
                StartListeningVotingEndedEventAsync(cancellationToken),
                StartListeningWinnerDeclaredEventAsync(cancellationToken)
            );
        }

        public async Task StartListeningAllEventsAsync(
            CancellationToken candidateAddedToken,
            CancellationToken votedToken,
            CancellationToken votingStartedToken,
            CancellationToken votingEndedToken,
            CancellationToken winnerDeclaredToken)
        {
            await Task.WhenAll(
                StartListeningCandidateAddedEventAsync(candidateAddedToken),
                StartListeningVotedEventAsync(votedToken),
                StartListeningVotingStartedEventAsync(votingStartedToken),
                StartListeningVotingEndedEventAsync(votingEndedToken),
                StartListeningWinnerDeclaredEventAsync(winnerDeclaredToken)
            );
        }
    }
}
