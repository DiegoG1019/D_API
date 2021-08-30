using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace D_API.Lib.Types
{
#warning There's currently no way to know how many requests we still have available
    public class D_APIRequestQueue : IRequestQueue
    {
        private readonly Task Running;

        private readonly Queue<DateTime> ImmediateRequests = new Queue<DateTime>(); //These are the seconds based ones

        private readonly IReadOnlyList<(Queue<DateTime> Dates, int Max)> Requests = new List<(Queue<DateTime>, int)>
        {
            (new Queue<DateTime>(), 80), // 1 : Minutes
            (new Queue<DateTime>(), 25_000), // 2 : 12h
            (new Queue<DateTime>(), 100_000)  // 3 : 7 days
        };

        private readonly Queue<Func<Task>> QueuedTasks = new Queue<Func<Task>>();

        private readonly AsyncLock Lock = new AsyncLock();
        protected async Task EnqueueTask(Func<Task> func)
        {
            using (await Lock.LockAsync())
                QueuedTasks.Enqueue(func);
        }

        protected readonly CancellationTokenSource TokenSource = new CancellationTokenSource();

        public D_APIRequestQueue()
        {
            Running = new Task(async () => await Run(TokenSource.Token), TaskCreationOptions.LongRunning);
            Running.Start();
        }

        protected virtual async Task Run(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
            WhileContinue:;
                await Task.Delay(500);

                //   Queue Clearing
                // ------------------

                while (ImmediateRequests.Count > 0)
                    if (ImmediateRequests.Peek() <= DateTime.Now)
                        ImmediateRequests.Dequeue();
                    else
                        break;
                foreach (var (Dates, _) in Requests)
                    while (Dates.Count > 0)
                        if (Dates.Peek() <= DateTime.Now)
                            Dates.Dequeue();
                        else
                            break;

                //   Available requests
                // ----------------------

                using (await Lock.LockAsync())
                    if (QueuedTasks.Count is 0)
                        continue;

                int x = 5 - ImmediateRequests.Count;
                if (x <= 0)
                    goto WhileContinue;
                foreach (var (Dates, Max) in Requests)
                {
                    var v = Max - Dates.Count;
                    if (v <= 0)
                        goto WhileContinue;
                    if (v < x)
                        x = v;
                }

                //   Execute Requests
                // --------------------

                var arr = new Task[x];
                using(await Lock.LockAsync())
                    for (int i = 0; i < x && QueuedTasks.Count > 0; i++)
                    {
                        arr[i] = Task.Run(async () =>
                        {
                            try
                            {
                                await QueuedTasks.Dequeue()();
                            }
                            catch(Exception exc)
                            {
                                return;
                            }
                        });
                        var date = DateTime.Now;
                        ImmediateRequests.Enqueue(date + TimeSpan.FromSeconds(1));
                        Requests[0].Dates.Enqueue(date + TimeSpan.FromMinutes(1));
                        Requests[1].Dates.Enqueue(date + TimeSpan.FromHours(12));
                        Requests[2].Dates.Enqueue(date + TimeSpan.FromDays(7));
                    }
                await Task.WhenAll(arr);
            }
        }

        public Task<T> NewRequest<T>(Func<Task<T>> func)
            => Task.Run(async () =>
            {
                T result = default;
                bool done = false;
                await EnqueueTask(async () =>
                {
                    await func();
                    done = true;
                });
                while (true)
                    if (!done)
                        await Task.Delay(50);
                    else
                        break;
                return result!;
            });

        public Task<T> NewRequest<T>(Func<T> func)
            => Task.Run(async () =>
            {
                T result = default;
                bool done = false;
                await EnqueueTask(() =>
                {
                    func();
                    done = true;
                    return Task.CompletedTask;
                });
                while (true)
                    if (!done)
                        await Task.Delay(50);
                    else
                        break;
                return result!;
            });

        public Task NewRequest(Func<Task> func)
            => Task.Run(async () =>
            {
                bool done = false;
                await EnqueueTask(async () =>
                {
                    await func();
                    done = true;
                });
                while (true)
                    if (!done)
                        await Task.Delay(50);
                    else
                        break;
            });

        public Task NewRequest(Action func)
            => Task.Run(async () =>
            {
                bool done = false;
                await EnqueueTask(() =>
                {
                    func();
                    done = true;
                    return Task.CompletedTask;
                });
                while (true)
                    if (!done)
                        await Task.Delay(50);
                    else
                        break;
            });
    }
}
