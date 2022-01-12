using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Threading.Internal;
using UnityEngine;

namespace Baracuda.Threading
{
    public sealed partial class Dispatcher
    {
        #region --- [INTERNAL COROUTINE] ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void StartCoroutineInternal(IEnumerator coroutine, TaskCompletionSource<object> completionSource, bool throwExceptions)
            => StartCoroutineInternal(coroutine, completionSource, throwExceptions, Current);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void StartCoroutineInternal(IEnumerator coroutine, TaskCompletionSource<object> completionSource, CancellationToken ct, bool throwExceptions)
            => StartCoroutineInternal(coroutine, completionSource, ct, throwExceptions, Current);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void StartCoroutineInternal(IEnumerator coroutine, TaskCompletionSource<object> tcs, bool throwExceptions, MonoBehaviour target)
        {
            if (throwExceptions)
            {
                target.StartCoroutineExceptionSensitive(coroutine, tcs.TrySetException, tcs.TrySetResult);
            }
            else
            {
                target.StartCoroutineExceptionSensitive(coroutine, tcs.TrySetResult, tcs.TrySetResult);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void StartCoroutineInternal(IEnumerator coroutine, TaskCompletionSource<object> tcs, CancellationToken ct, bool throwExceptions, MonoBehaviour target)
        {
            if (throwExceptions)
            {
                target.StartCoroutineExceptionSensitive(coroutine, tcs.TrySetException, tcs.TrySetResult, ct);
            }
            else
            {
                target.StartCoroutineExceptionSensitive(coroutine, tcs.TrySetResult, tcs.TrySetResult, ct);
            }
        }
        
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DISPATCH: COROUTINE (AWAITABLE)] ---

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that is executed as a <see cref="Coroutine"/>
        /// on the main thread and return a <see cref="Task"/>, that can be awaited and returns
        /// after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeCoroutineAsync(IEnumerator enumerator, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource<object>();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, throwExceptions);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeCoroutineAsync(IEnumerator enumerator, CancellationToken ct, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource<object>();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, ct, throwExceptions);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }
        

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeCoroutineAsync(IEnumerator enumerator, MonoBehaviour target, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource<object>();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, throwExceptions, target);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeCoroutineAsync(IEnumerator enumerator, MonoBehaviour target, CancellationToken ct, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource<object>();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, ct, throwExceptions, target);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            });

            return tcs.Task;
        }



        //--------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeCoroutineAsync(IEnumerator enumerator, ExecutionCycle cycle, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource<object>();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, throwExceptions);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeCoroutineAsync(IEnumerator enumerator, ExecutionCycle cycle, CancellationToken ct, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource<object>();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, ct, throwExceptions);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeCoroutineAsync(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource<object>();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, throwExceptions, target);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task"/>, that when awaited on the calling thread, returns
        /// the after the <see cref="Coroutine"/> has completed on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"></param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"></param>
        /// <param name="throwExceptions"></param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task InvokeCoroutineAsync(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, CancellationToken ct, bool throwExceptions = true)
        {
            var tcs = new TaskCompletionSource<object>();

            Invoke(() =>
            {
                try
                {
                    StartCoroutineInternal(enumerator, tcs, ct, throwExceptions, target);
                }
                catch (Exception exception)
                {
                    tcs.TrySetException(exception);
                }
            }, cycle);

            return tcs.Task;
        }
        
        #endregion
    }
}