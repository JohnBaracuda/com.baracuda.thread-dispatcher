using System;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Threading.Utils;
using UnityEngine;

namespace Baracuda.Threading
{
    public partial class Dispatcher
    {
        #region --- [DISPATCH: TASK] ---

        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="task"><see cref="Task"/> dispatched task.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Invoke(Func<Task> task)
        {
            async void Action()
            {
                await task();
            }
            Invoke(Action);
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Invoke(Func<Task> func, ExecutionCycle cycle)
        {
            async void Action()
            {
                await func();
            }

            Invoke(Action, cycle);
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Task"/> dispatched task.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Invoke(Func<Task> func, CancellationToken ct, bool throwOnCancellation = true)
        {
            async void Action()
            {
                if (ct.IsCancellationRequested)
                {
                    if (throwOnCancellation)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        return;
                    }
                }

                await func();
            }
            Invoke(Action);
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static void Invoke(Func<Task> func, ExecutionCycle cycle, CancellationToken ct,
            bool throwOnCancellation = true)
        {
            async void Action()
            {
                if (ct.IsCancellationRequested)
                {
                    if (throwOnCancellation)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        return;
                    }
                }

                await func();
            }
            Invoke(Action, cycle);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DISPATCH: TASK ASYNC] ---

        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="task"><see cref="Task"/> dispatched task.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Func<Task> task)
        {
            var tcs = new TaskCompletionSource();

            async void Action()
            {
                try
                {
                    await task();
                    tcs.SetCompleted();
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Func<Task> func, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource();

            async void Action()
            {
                try
                {
                    await func();
                    tcs.SetCompleted();
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action, cycle);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Task"/> dispatched task.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Func<Task> func, CancellationToken ct, bool throwOnCancellation = true)
        {
            var tcs = new TaskCompletionSource();

            async void Action()
            {
                try
                {
                    if (ct.IsCancellationRequested)
                    {
                        if (throwOnCancellation)
                        {
                            ct.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            tcs.SetCompleted();
                            return;
                        }
                    }

                    await func();
                    tcs.SetCompleted();
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Func<Task> func, ExecutionCycle cycle, CancellationToken ct,
            bool throwOnCancellation = true)
        {
            var tcs = new TaskCompletionSource();

            async void Action()
            {
                try
                {
                    if (ct.IsCancellationRequested)
                    {
                        if (throwOnCancellation)
                        {
                            ct.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            tcs.SetCompleted();
                            return;
                        }
                    }

                    await func();
                    tcs.SetCompleted();
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action, cycle);
            return tcs.Task;
        }

        #endregion

        #region --- [DISPATCH: TASK<TRESULT> ASYNC] ---

        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#taskTResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func)
        {
            var tcs = new TaskCompletionSource<TResult>();

            async void Action()
            {
                try
                {
                    var task = func();
                    tcs.SetResult(await task);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task{TResult}"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#taskTResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<TResult>();

            async void Action()
            {
                try
                {
                    var task = func();
                    tcs.SetResult(await task);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action, cycle);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <param name="ct"></param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#taskTResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();

            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var task = func();
                    tcs.SetResult(await task);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task{TResult}"/> is executed.</param>
        /// <param name="ct"></param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#taskTResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func, ExecutionCycle cycle,
            CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();

            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var task = func();
                    tcs.SetResult(await task);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            Invoke(Action, cycle);
            return tcs.Task;
        }

        #endregion
    }
}