using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Nethereum.Hex.HexTypes;

namespace EthereumIntegration
{
    public class SmartContract
    {
        private readonly string accountAddress;

        protected readonly Account ownerAccount;
        protected readonly Web3 web3;
        protected readonly string abi;
        protected readonly Contract contract;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartContract"/> class.
        /// </summary>
        /// <param name="url">The URL of the Ethereum node.</param>
        /// <param name="abiPath">The path to the ABI JSON file.</param>
        /// <param name="contractAddress">Smart contract address.</param>
        /// <param name="accountAddress">Owner account address.</param>
        /// <param name="privateKey">Private key of the owner's account.</param>
        public SmartContract(in string url, in string abiPath, in string contractAddress, in string accountAddress, in string privateKey)
        {
            this.accountAddress = accountAddress;
            ownerAccount = new Account(privateKey);
            web3 = new Web3(ownerAccount, url);

            if (File.Exists(abiPath))
            {
                var json = JObject.Parse(File.ReadAllText(abiPath));
                abi = json["abi"]?.ToString() ?? throw new Exception("Invalid ABI file: 'abi' property not found");
            }
            else
            {
                try
                {
                    var remoteAbi = RemoteDataFetcher.FetchAbiFromRemoteUrlAsync(abiPath).GetAwaiter().GetResult();
                    abi = remoteAbi;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to retrieve ABI from remote URL.", ex);
                }
            }

            contract = web3.Eth.GetContract(abi, contractAddress);
        }


        /// <summary>
        /// Changes the state of the contract (sending a transaction).
        /// </summary>
        /// <remarks>Submit a transaction on behalf of the owner </remarks>
        /// <returns>The transaction hash.</returns>
        public async Task<string> SendTransactionAsOwnerAsync(string functionName, params object[] functionInput)
        {
            var function = contract.GetFunction(functionName);
            const int defaultGasLimit = 3000000;

            try
            {
                var transactionHash = await function.SendTransactionAndWaitForReceiptAsync(
                    accountAddress,
                    new HexBigInteger(defaultGasLimit),
                    null,
                    null,
                    functionInput
                );

                return transactionHash.TransactionHash;
            }
            catch (Exception ex)
            {
                // исключение пробрасывается дальше 
                Console.WriteLine($"Error in SendTransactionAsOwnerAsync: {ex.Message}");
                throw new Exception("Failed to send transaction as owner.", ex);
            }
        }

        /// <summary>
        /// Changes the state of the contract (sending a transaction).
        /// </summary>
        /// <remarks> Submit a transaction on behalf of a user </remarks>
        /// <returns>The transaction hash.</returns>
        public async Task<string> SendTransactionAsUserAsync(string functionName, string signerPrivateKey, params object[] functionInput)
        {
            var account = new Account(signerPrivateKey);
            var function = contract.GetFunction(functionName);
            const int defaultGasLimit = 3000000;

            try
            {
                var transactionHash = await function.SendTransactionAndWaitForReceiptAsync(
                    account.Address,
                    new HexBigInteger(defaultGasLimit),
                    null,
                    null,
                    functionInput);

                return transactionHash.TransactionHash;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Error in SendTransactionAsUserAsync: {ex.Message}");
                throw new Exception("Failed to send transaction as user.", ex);
            }
        }

        /// <summary>
        /// Does not change the state of the contract (function call)
        /// </summary>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <returns>Result of the function call.</returns>
        public async Task<T> CallFunctionAsync<T>(string functionName, params object[] functionInput)
        {
            var function = contract.GetFunction(functionName);

            try
            {
                return await function.CallAsync<T>(functionInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CallFunctionAsync: {ex.Message}");
                throw new Exception("Failed to call function.", ex);
            }
        }


        /// <summary>
        /// Starts listening for events emitted by the smart contract.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments, which should derive from <see cref="EventArgs"/>.</typeparam>
        /// <param name="eventName">The name of the event to listen for.</param>
        /// <param name="eventHandler">The event handler to be invoked when the event is received. Can be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StartListeningEventAsync<TEventArgs>(string eventName, EventHandler<TEventArgs>? eventHandler)
            where TEventArgs : EventArgs, new()
        {
            var contractEvent = contract.GetEvent(eventName);
            var filterInput = contractEvent.CreateFilterInput();

            // Subscribing to new logs
            var filterId = await contractEvent.CreateFilterAsync();
            _ = Task.Run(async () =>
            {
                int eventCount = 0;
                var stopwatch = Stopwatch.StartNew();

                while (true)
                {
                    try
                    {
                        var newLogs = await web3.Eth.Filters.GetFilterChangesForEthNewFilter.SendRequestAsync(filterId);

                        foreach (var log in newLogs)
                        {
                            eventCount++;
                            var decodedEvents = contractEvent.DecodeAllEventsForEvent<TEventArgs>(new[] { log });
                            foreach (var eventInstance in decodedEvents)
                            {
                                var eventArgs = new TEventArgs();
                                foreach (var prop in typeof(TEventArgs).GetProperties())
                                {
                                    var value = eventInstance.Event.GetType().GetProperty(prop.Name)?.GetValue(eventInstance.Event);
                                    prop.SetValue(eventArgs, value);
                                }
                                Console.WriteLine($"Decoded event: {eventInstance.Event}");
                                eventHandler?.Invoke(this, eventArgs);
                                Console.WriteLine($"Event triggered: {eventName}, {eventArgs}");
                            }
                        }

                        await Task.Delay(AdaptiveDelay(eventCount));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in event listener: {ex.Message}");
                        throw new Exception("Error in event listener:", ex);
                    }
                }

                int AdaptiveDelay(int eventCount)
                {
                    int delay = 1000;
                    const int minDelay = 100;
                    const int maxDelay = 2000;
                    const int adjustInterval = 60000;

                    if (stopwatch.ElapsedMilliseconds > adjustInterval)
                    {
                        if (eventCount > 50)
                        {
                            delay = Math.Max(minDelay, delay / 2);
                        }
                        else if (eventCount < 10)
                        {
                            delay = Math.Min(maxDelay, delay * 2);
                        }

                        eventCount = 0;
                        stopwatch.Restart();
                    }
                    return delay;
                }
            });
        }

        /// <summary>
        /// Starts listening for events emitted by the smart contract with support for cancellation.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments, which should derive from <see cref="EventArgs"/>.</typeparam>
        /// <param name="eventName">The name of the event to listen for.</param>
        /// <param name="eventHandler">The event handler to be invoked when the event is received. Can be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method creates a filter to listen for events emitted by the smart contract. It handles the decoding of event logs and invokes the provided event handler.
        /// The operation can be cancelled by passing a <see cref="CancellationToken"/>. The method will uninstall the filter once the cancellation is requested or the task is completed.
        /// </remarks>
        public async Task StartListeningEventAsync<TEventArgs>(string eventName, EventHandler<TEventArgs>? eventHandler, CancellationToken cancellationToken = default)
            where TEventArgs : EventArgs, new()
        {
            var contractEvent = contract.GetEvent(eventName);
            var filterId = await contractEvent.CreateFilterAsync();

            _ = Task.Run(async () =>
            {
                int eventCount = 0;
                var stopwatch = Stopwatch.StartNew();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var newLogs = await web3.Eth.Filters.GetFilterChangesForEthNewFilter.SendRequestAsync(filterId);

                        foreach (var log in newLogs)
                        {
                            eventCount++;
                            var decodedEvents = contractEvent.DecodeAllEventsForEvent<TEventArgs>(new[] { log });

                            foreach (var eventInstance in decodedEvents)
                            {
                                var eventArgs = new TEventArgs();
                                foreach (var prop in typeof(TEventArgs).GetProperties())
                                {
                                    var value = eventInstance.Event.GetType().GetProperty(prop.Name)?.GetValue(eventInstance.Event);
                                    prop.SetValue(eventArgs, value);
                                }

                                Console.WriteLine($"Decoded event: {eventInstance.Event}");
                                eventHandler?.Invoke(this, eventArgs);
                                Console.WriteLine($"Event triggered: {eventName}, {eventArgs}");
                            }
                        }

                        await Task.Delay(AdaptiveDelay(eventCount), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Event listening was canceled.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in event listener: {ex.Message}");
                        throw new Exception("Error in event listener:", ex);
                    }
                }
                // Uninstall the filter when done
                await web3.Eth.Filters.UninstallFilter.SendRequestAsync(filterId);

                int AdaptiveDelay(int eventCount)
                {
                    int delay = 1000;
                    const int minDelay = 100;
                    const int maxDelay = 2000;
                    const int adjustInterval = 60000;

                    if (stopwatch.ElapsedMilliseconds > adjustInterval)
                    {
                        if (eventCount > 50)
                        {
                            delay = Math.Max(minDelay, delay / 2);
                        }
                        else if (eventCount < 10)
                        {
                            delay = Math.Min(maxDelay, delay * 2);
                        }

                        eventCount = 0;
                        stopwatch.Restart();
                    }
                    return delay;
                }
            }, cancellationToken);
        }
    }
}
