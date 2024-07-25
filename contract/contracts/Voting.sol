// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "./HashFix.sol";
import "./NumberToString.sol";

contract Voting {
    using HashFix for *;
    using NumberToString for uint256;
    
    struct Candidate {
        uint id;
        string name;
        string description; 
        uint voteCount;
    } 

    address payable private immutable admin;

    mapping(uint => Candidate) public candidates;
    mapping(bytes32 => bool) private alreadyExists; 
    mapping(address => uint) private voteRecord;
    uint public candidatesCount;
    address[] private voters;
    uint public votingStartTime;
    uint public votingEndTime;
    bool public votingStarted = false;
    uint public totalVotes = 0; 

    event CandidateAdded(uint candidateID, string name, string description);
    event Voted(address indexed voter, uint candidateID);
    event VotingStarted(uint startTime, uint durationMinutes);
    event VotingEnded(uint endTime);
    event WinnerDeclared(uint candidateID, string name, string description, uint voteCount, string percent);

    modifier onlyAdmin() {
        require(msg.sender == admin, "Only admin can perform this action");
        _;
    }

    constructor() payable {
        admin = payable(msg.sender);
    }
 
    // Добавление нового кандидата администратором
    function addCandidate(string memory name, string memory description) public onlyAdmin {
        require(!alreadyExists[HashFix.hashStruct(name, description)], "Candidate with the same name and description already exists");
        require(!votingStarted, "Voting has already started");

        candidatesCount++;
        candidates[candidatesCount] = Candidate(candidatesCount, name, description, 0);
        alreadyExists[HashFix.hashStruct(name, description)] = true; // Отметить имя и партию как добавленные

        emit CandidateAdded(candidatesCount, name, description);
    }

    // Начало голосования администратором
    function startVoting(uint durationMinutes) public onlyAdmin {
        require(candidatesCount >= 2, "Need at least two candidates to start voting");
        require(!votingStarted, "Voting has already started");

        votingStartTime = block.timestamp;
        votingEndTime = votingStartTime + (durationMinutes * 1 minutes);
        votingStarted = true;
        totalVotes = 0;

        emit VotingStarted(votingStartTime, durationMinutes);
    }
    
    // Голосование за кандидата
    function vote(uint candidateID) public {
        require(votingStarted, "Voting not started yet");
        require(block.timestamp >= votingStartTime && block.timestamp <= votingEndTime, "Voting period has ended or not started");
        require(voteRecord[msg.sender] == 0, "Already voted");
        require(candidateID > 0 && candidateID <= candidatesCount, "Invalid candidate ID");

        voteRecord[msg.sender] = candidateID;
        voters.push(msg.sender); // Сохранение адреса голосующего
        candidates[candidateID].voteCount++;
        totalVotes++;

        emit Voted(msg.sender, candidateID);
    }
    
    // Получение статуса голосования
    function getVotingStatus() public view returns (bool isVotingActive, uint startTime, uint endTime, uint totalVotesCount) {
        return (votingStarted, votingStartTime, votingEndTime, totalVotes);
    }

    // Получение информации о кандидате по его ID
    function getCandidate(uint candidateID) public view returns (Candidate memory) {
        require(candidateID > 0 && candidateID <= candidatesCount, "Invalid candidate ID");
        return candidates[candidateID];
    }

    // Проверка, голосовал ли указанный адрес
    function verifyVoter(address voter) public view returns (bool) {
        return voteRecord[voter] != 0;
    }

    // Завершение голосования администратором
    function endVoting() public onlyAdmin {
        require(votingStarted, "Voting has not been started yet");
        votingStarted = false;
        emit VotingEnded(block.timestamp);
        if(totalVotes > 0) 
            declareWinner();
        resetVoting();
    }

    // Сброс состояния голосования
    function resetVoting() private {
        require(!votingStarted, "Cannot reset voting while it is active.");

        for (uint i = 1; i <= candidatesCount; i++) {
            Candidate storage candidate = candidates[i];
            bytes32 namedescriptionKey = HashFix.hashStruct(candidate.name, candidate.description);
            delete alreadyExists[namedescriptionKey];
            delete candidates[i];
        }
        candidatesCount = 0;

        for (uint i = 0; i < voters.length; i++) {
            delete voteRecord[voters[i]];
        }
        delete voters; 

        totalVotes = 0;
        votingStarted = false;
        votingStartTime = 0;
        votingEndTime = 0;
    }

    // Определение победителя голосования 
    function declareWinner() private {
        require(totalVotes > 0, "No votes have been cast");

        uint winningVoteCount = 0;
        uint[] memory winners = new uint[](candidatesCount); // Массив с максимально возможным размером
        uint winnersCount = 0;

        for (uint i = 1; i <= candidatesCount; i++) {
            if (candidates[i].voteCount > winningVoteCount) {
                winningVoteCount = candidates[i].voteCount;
                winners[0] = i;
                winnersCount = 1;
            } else if (candidates[i].voteCount == winningVoteCount) {
                winners[winnersCount] = i;
                winnersCount++;
            }
        }

        for(uint i = 0; i < winnersCount; i++){
            uint candidateID = winners[i];
            uint winPercent100 = (winningVoteCount * 10000) / totalVotes;
            uint whole = winPercent100 / 100;
            uint fract = winPercent100 % 100;
            string memory winnerPercent = string(abi.encodePacked(whole.toString(), ".", fract.toString()));

            emit WinnerDeclared(candidateID, candidates[candidateID].name, candidates[candidateID].description, winningVoteCount, winnerPercent);
        }
    }
}
