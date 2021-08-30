using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace D_API.Lib.Types
{
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

                //   Available requests
                // ----------------------

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
                for (int i = 0; i < x && QueuedTasks.Count > 0; i++)
                    arr[i] = QueuedTasks.Dequeue()();
                await Task.WhenAll(arr);
            }
        }

        public Task<T> NewRequest<T>(Func<Task<T>> func)
        {

        }

        public Task<T> NewRequest<T>(Func<T> func)
        {
            throw new NotImplementedException();
        }

        public Task NewRequest(Func<Task> func)
        {
            throw new NotImplementedException();
        }

        public Task NewRequest(Action func)
        {
            throw new NotImplementedException();
        }
    }
}
