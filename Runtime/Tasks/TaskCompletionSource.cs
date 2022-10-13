using System;
using System.Threading.Tasks;

namespace Baracuda.Threading.Tasks
{
    /// <summary>
    /// Non generic wrapper for a <see cref="TaskCompletionSource{T}"/> that represents a Task that returns no value.
    /// </summary>
    internal sealed class TaskCompletionSource : TaskCompletionSource<Exception>
    {
        public void SetCompleted() => SetResult(null);
        public bool TrySetCompleted() => TrySetResult(null);
    }
}
