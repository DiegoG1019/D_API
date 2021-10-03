using D_API.Lib.Exceptions;
using Nito.AsyncEx;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace D_API.Lib.Internal
{
    public class D_APIRequestQueue : IRequestQueue
    {
        private class EndpointTimeout
        {
            public readonly SemaphoreSlim? Semaphore;
            public readonly TimeSpan Timeout;

            public EndpointTimeout(SemaphoreSlim? semaphore, TimeSpan? timeout = null)
            {
                Semaphore = semaphore;
                Timeout = timeout ?? TimeSpan.Zero;
            }
        }

        private class Request
        {
            public readonly Endpoint Endpoint;
            public readonly Func<Task> Task;
            public Request(Endpoint endpoint, Func<Task> task)
            {
                Endpoint = endpoint;
                Task = task;
            }
        }

        private readonly SemaphoreSlim TooManyRequestsSemaphore = new SemaphoreSlim(1, 1);
        private readonly Dictionary<Endpoint, EndpointTimeout> EndpointTimeouts;
        private readonly Queue<Request> QueuedTasks = new Queue<Request>();

        protected readonly CancellationTokenSource TokenSource = new CancellationTokenSource();

        public D_APIRequestQueue()
        {
            EndpointTimeouts = new Dictionary<Endpoint, EndpointTimeout>()
            {
                { Endpoint.General, new EndpointTimeout(new SemaphoreSlim(5, 5), TimeSpan.FromSeconds(1.1)) },
                { Endpoint.Auth, new EndpointTimeout(new SemaphoreSlim(1, 1), TimeSpan.FromSeconds(1.1)) },
                { Endpoint.Probe, new EndpointTimeout(new SemaphoreSlim(2, 2), TimeSpan.FromSeconds(1.1)) },
                { Endpoint.Whitelist, new EndpointTimeout(null) }
            };

            Run(TokenSource.Token);
        }

        protected virtual async void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while(QueuedTasks.Count > 0)
                {
                    Request request;
                    using (await Lock.LockAsync())
                        if (!QueuedTasks.TryDequeue(out request))
                            continue;

                    await TooManyRequestsSemaphore.WaitAsync();
                    TooManyRequestsSemaphore.Release();

                    var et = EndpointTimeouts[request.Endpoint];
                    bool sem = et.Semaphore != null;
                    if (sem)
                        await et.Semaphore!.WaitAsync();

                    for (; ; )
                        try
                        {
                            await request.Task();

                            break;
                        }
                        catch (D_APITooManyRequestsException)
                        {
                            await TooManyRequestsSemaphore.WaitAsync();
                            await Task.Delay(2_000);
                            TooManyRequestsSemaphore.Release();

                            continue;
                        }

                    if (sem && et.Timeout > TimeSpan.Zero)
                        FireRequest(et.Semaphore!, et.Timeout);
                }
                await Task.Delay(500);
            }
        }

        protected static async void FireRequest(SemaphoreSlim semaphore, TimeSpan ts)
        {
            await Task.Delay(ts);
            semaphore.Release();
        }

        private readonly AsyncLock Lock = new AsyncLock();
        protected async Task<object?> EnqueueTask(Func<Task<object?>> request, Endpoint endpoint)
        {
            var semaphore = new SemaphoreSlim(1, 1);
            object? result = null;
            semaphore.Wait();
            using (await Lock.LockAsync())
                QueuedTasks.Enqueue(new Request(endpoint, async () =>
                {
                    try
                    {
                        result = await request();
                        semaphore.Release();
                    }
                    catch (D_APITooManyRequestsException)
                    {
                        throw;
                    }
                    catch (D_APIRequestJWTExpiredException)
                    {
                        throw;
                    }
                    catch(Exception e)
                    {
                        result = e;
                        semaphore.Release();
                    }
                }));
            await semaphore.WaitAsync();
            return result is Exception exc ? throw exc : result;
        }

        public async Task<T> NewRequest<T>(Func<Task<T>> func, Endpoint endpoint)
            => (T)await EnqueueTask(async () => await func(), endpoint);

        public async Task<T> NewRequest<T>(Func<T> func, Endpoint endpoint)
            => (T)await EnqueueTask(async () => func(), endpoint);

        public async Task NewRequest(Func<Task> func, Endpoint endpoint)
            => await EnqueueTask(async () => { await func(); return null; }, endpoint);

        public async Task NewRequest(Action func, Endpoint endpoint)
            => await EnqueueTask(() => { func(); return Task.FromResult<object?>(null); }, endpoint);

        ~D_APIRequestQueue() => TokenSource.Cancel();
    }
}
