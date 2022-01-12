
using System;
using System.Threading.Tasks;

namespace Baracuda.Threading.Utils
{
    public class TaskCompletionSource : TaskCompletionSource<Exception>
    {
        public void SetCompleted() => SetResult(null);
        public bool TrySetCompleted() => TrySetResult(null);
    }
}
