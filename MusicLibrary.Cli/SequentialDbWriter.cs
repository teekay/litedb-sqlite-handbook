using System.Collections.Concurrent;
using System.Diagnostics;

namespace MusicLibrary.Cli
{
    public sealed class SequentialDbWriter: IDbWriter, IDisposable
    {
        public SequentialDbWriter()
        {
            _commits = Task.Run(PerformCommitsOneByOne, _stopFlag.Token);
        }

        private readonly BlockingCollection<Action> _commitQueue = new BlockingCollection<Action>();
        private readonly CancellationTokenSource _stopFlag = new CancellationTokenSource();
        private readonly Task _commits;

        public void Commit(Action commitAction)
        {
            if (_commitQueue.IsAddingCompleted) return;
            _commitQueue.Add(commitAction);
        }

        /// <summary>
        /// Monitor the commit queue, and perform commits in an orderly fashion
        /// </summary>
        private void PerformCommitsOneByOne()
        {
            while (!_commitQueue.IsCompleted && !_stopFlag.IsCancellationRequested)
            {
                try
                {
                    var item = _commitQueue.Take();
                    item.Invoke();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (InvalidOperationException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Database write operation failed. Exception: {e.GetType()}. Message: {e.Message}");
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _commitQueue.CompleteAdding();
            }
            catch (ObjectDisposedException)
            {
            }
            try
            {
                _stopFlag.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException)
            {
            }

            var delays = 0;
            while (!_commits.IsCompleted && delays < 1000)
            {
                Thread.Sleep(20);
                delays++;
            }
            Debug.WriteLine($"Waited {delays * 20} ms. to complete the commit queue");
        }
    }
}
