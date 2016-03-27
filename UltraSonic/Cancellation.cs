using System.Threading;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private CancellationToken GetCancellationToken(string tokenType)
        {
            CancelTasks(tokenType);
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            QueueTask(tokenType, tokenSource);
            return token;
        }

        private void CancelTasks(string tokenType)
        {
            CancellationTokenSource token;

            if (_cancellableTasks.TryRemove(tokenType, out token))
                token.Cancel();
        }

        private void QueueTask(string tokenType, CancellationTokenSource token)
        {
            _cancellableTasks.TryAdd(tokenType, token);
        }
    }
}
