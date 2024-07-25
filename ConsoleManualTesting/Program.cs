using VotingSystemLib;

const string url = "HTTP://127.0.0.1:7545";
const string contractAddress = "0x70cF794453688C119e4abC44a12511399b877b40";
const string accountAddress = "0xBa1A320eAEA1933c9807B19aFeCeA1d26E504081";
const string privateKey = "0x93143c540ed7344834fa84fb9354b2e60fc0d98fb61e3d5fd6318025f31c982d";

var votingSystem = VotingSystem.GetInstance(url, VotingSystem.RemoteUrl, contractAddress, accountAddress, privateKey);

// Подписка на события
votingSystem.CandidateAdded += (sender, e) => Console.WriteLine("Кандидат добавлен: " + e.Name);
votingSystem.Voted += (subscriber, e) => Console.WriteLine($"Голос отдан за кандидата с ID: {e.CandidateID}");
votingSystem.VotingStarted += (subscriber, e) => Console.WriteLine($"Голосование начато в {e.StartTime}, длительность: {e.DurationMinutes} минут");
votingSystem.VotingEnded += (subscriber, e) => Console.WriteLine($"Голосование завершено в {e.EndTime}");
votingSystem.WinnerDeclared += (subscriber, e) => Console.WriteLine($"Победитель: {e.Name} с {e.VoteCount} голосами ({e.Percent}%)");

using var cts = new CancellationTokenSource();

await votingSystem.StartListeningAllEventsAsync(cts.Token);

Console.WriteLine("Добавление кандидата...");
try 
{
    await votingSystem.AddCandidate("Какое-то имя1", "И описание");
}
catch
{
    Console.WriteLine("Кандидат не добавлен");
}

try
{
    Console.WriteLine("Начало голосования...");
    await votingSystem.StartVoting(5); // Голосование длится 5 минут
}
catch
{
    Console.WriteLine("голосование не началось");
}

await Task.Delay(2000);

Console.WriteLine("Голосование за кандидата...");
await votingSystem.Vote(1, privateKey);

await Task.Delay(6000);

//Console.WriteLine("Завершение голосования...");
//await votingSystem.EndVoting();

await Task.Delay(2000);

cts.Cancel();

Console.WriteLine("Для выхода нажмите любую клавишу...");
Console.ReadKey();
