using System;

namespace Baracuda.Threading.Coroutines
{
    /// <summary>
    /// Exception is thrown inside a <see cref="UnityEngine.Coroutine"/> that is handled by the
    /// <see cref="ExceptionSensitiveCoroutineHandler"/> class if the coroutines target behaviour is disabled
    /// while it is still running.
    /// </summary>
    public sealed class BehaviourDisabledException : SystemException
    {
        internal BehaviourDisabledException(string message) : base(message)
        {
        }
    }
}