#if  DISPATCHER_DISABLE_UPDATE && DISPATCHER_DISABLE_LATEUPDATE && DISPATCHER_DISABLE_POSTUPDATE && DISPATCHER_DISABLE_FIXEDUPDATE && DISPATCHER_DISABLE_TICKUPDATE
    #define TICKFALLBACK
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Threading.Internal;
using UnityEngine;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Baracuda.Threading
{
    /// <summary>
    /// Class for dispatching the execution of a<br/>
    /// <see cref="Delegate"/>, <see cref="IEnumerator"/> or <see cref="Task"/>
    /// from a background thread to the main thread.
    /// </summary>
    /// <footer><a href="https://johnbaracuda.com/dispatcher.html">Documentation</a></footer>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DispatcherPostUpdate))]
    public sealed partial class Dispatcher : MonoBehaviour
    {
        #region --- [UTILITIES] ---

        /// <summary>
        /// Returns true if called from the main thread and false if not.
        /// </summary>
        /// <returns></returns>
        ///<footer><a href="https://johnbaracuda.com/dispatcher.html#miscellaneous">Documentation</a></footer>
        public static bool IsMainThread() => Thread.CurrentThread.ManagedThreadId == (_mainThread?.ManagedThreadId 
            ?? throw new Exception($"{nameof(Dispatcher)}.{nameof(_mainThread)} is not initialized"));

        
        /// <summary>
        /// Ensure that a <see cref="Dispatcher"/> instance exists and return it.
        /// This method is just a wrapper for Dispatcher.<see cref="Dispatcher.Current"/>
        /// </summary>
        /// <returns></returns>
        ///<footer><a href="https://johnbaracuda.com/dispatcher.html#miscellaneous">Documentation</a></footer>
        public static Dispatcher ValidateDispatcher() => Current;

        
#if DISPATCHER_DEBUG
        /// <summary>
        /// Get the <see cref="ExecutionCycle"/> definition of the currently executed update cycle.
        /// This property is only available if DISPATCHER_DEBUG is defined.
        /// </summary>
        public static ExecutionCycle CurrentCycle { get; private set; } = ExecutionCycle.Default;
#endif
        
        #endregion
                
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DISPATCH: ACTION] ---

        /// <summary>
        /// Dispatch the execution of an <see cref="Action"/> to the main thread.
        /// Actions are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// Use <see cref="Invoke(System.Action, ExecutionCycle)"/> 
        /// for more control over the cycle in which the dispatched <see cref="Action"/> is executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions">Documentation</a></footer>
        public static void Invoke(Action action)
        {
            lock (_defaultExecutionQueue)
            {
                _defaultExecutionQueue.Enqueue(action);
            }
            _queuedDefault = true;
        }


        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and determine the exact cycle,
        /// during which the passed action will be executed.
        /// </summary>
        /// <param name="action"><see cref="Action"/> dispatched action.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Action"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions">Documentation</a></footer>
        public static void Invoke(Action action, ExecutionCycle cycle)
        {
            switch (cycle)
            {
#if !DISPATCHER_DISABLE_UPDATE
                case ExecutionCycle.Update:
                    lock (_updateExecutionQueue)
                    {
                        _updateExecutionQueue.Enqueue(action);
                    }
                    _queuedUpdate = true;
                    break;
#endif
                
#if !DISPATCHER_DISABLE_LATEUPDATE
                case ExecutionCycle.LateUpdate:
                    lock (_lateUpdateExecutionQueue)
                    {
                        _lateUpdateExecutionQueue.Enqueue(action);
                    } 
                    _queuedLate = true;
                    break;
#endif
                
#if !DISPATCHER_DISABLE_FIXEDUPDATE
                case ExecutionCycle.FixedUpdate:
                    lock (_fixedUpdateExecutionQueue)
                    {
                        _fixedUpdateExecutionQueue.Enqueue(action);
                    } 
                    _queuedFixed = true;
                    break;
#endif
                
#if !DISPATCHER_DISABLE_POSTUPDATE
                case ExecutionCycle.PostUpdate:
                    lock (_postUpdateExecutionQueue)
                    {
                        _postUpdateExecutionQueue.Enqueue(action);
                    } 
                    _queuedPost = true;
                    break;
#endif
                
#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
                case ExecutionCycle.TickUpdate:
                    lock (_tickExecutionQueue)
                    {
                        _tickExecutionQueue.Enqueue(action);
                    } 
                    _queuedTick = true;
                    break;
#endif
                
#if UNITY_EDITOR && !DISPATCHER_DISABLE_EDITORUPDATE
                case ExecutionCycle.EditorUpdate:
                    lock (_editorExecutionQueue)
                    {
                        _editorExecutionQueue.Enqueue(action);
                    } 
                    _queuedEditor = true;
                    break;
#endif
                
                default:
                    lock (_defaultExecutionQueue)
                    {
                        _defaultExecutionQueue.Enqueue(action);
                    } 
                    _queuedDefault = true;
                    break;
                    
            }
        }
        

        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> that will be invoked.</param>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-async">Documentation</a></footer>
        public static Task InvokeAsync(Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            
            Invoke(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            });
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> that will be invoked.</param>
        /// <param name="cycle">The execution cycle during which the <see cref="Action"/> will be invoked.</param>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-async">Documentation</a></footer>
        public static Task InvokeAsync(Action action, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<object>();
            
            Invoke(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }, cycle);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"><see cref="Action"/> that will be invoked.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"></param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-async-cancel">Documentation</a></footer>
        public static Task InvokeAsync(Action action, CancellationToken ct, bool throwOnCancellation = true)
        {
            if (ct.IsCancellationRequested)
            {
                if (throwOnCancellation)
                {
                    ct.ThrowIfCancellationRequested();
                }
                else
                {
                    return Task.CompletedTask;
                }
            }

            var tcs = new TaskCompletionSource<object>();
            
            Invoke(() =>
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
                            tcs.SetResult(null);
                            return;
                        }
                    }
                    
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            });
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="Action"/> that will be executed on the main thread and return a <see cref="Task"/>, 
        /// that when awaited on the calling thread, returns after the <see cref="Action"/>
        /// was executed on the main thread.
        /// </summary>
        /// <param name="action"> <see cref="Action"/> that will be invoked.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Action"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> optional parameter that determines if an <see cref="OperationCanceledException"/>
        /// is thrown if the Task is cancelled prematurely.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the passed action has been executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#actions-async-cancel">Documentation</a></footer>
        public static Task InvokeAsync(Action action, ExecutionCycle cycle, CancellationToken ct, bool throwOnCancellation = true)
        {
            if (ct.IsCancellationRequested)
            {
                if (throwOnCancellation)
                {
                    ct.ThrowIfCancellationRequested();
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
            var tcs = new TaskCompletionSource<object>();
            
            Invoke(() =>
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
                            tcs.SetCanceled();
                            return;
                        }
                    }
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }, cycle);
            return tcs.Task;
        }
        
        #endregion
        
        #region --- [DISPATCH: COROUTINE] ---
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Invoke(IEnumerator enumerator)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) 
                    throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                Current.StartCoroutine(enumerator);
            });
        }

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Invoke(IEnumerator enumerator, ExecutionCycle cycle)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                    return;
                }
#endif
                Current.StartCoroutine(enumerator);
            }, cycle);
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Invoke(IEnumerator enumerator, MonoBehaviour target)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                    return;
                }
#endif
                target.StartCoroutine(enumerator);
            });
        }
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/> on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines">Documentation</a></footer>
        public static void Invoke(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target)
        {
            Invoke(() =>
            {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                    return;
                }
#endif
                target.StartCoroutine(enumerator);
            },cycle);
        }
        
        //--------------------------------------------------------------------------------------------------------------      
        
        
        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator)
        {
            var tcs = new TaskCompletionSource<Coroutine>();
            
            Invoke(() =>
            {
                try
                {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                    if (!Application.isPlaying)
                    {
                        Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                        return;
                    }
#endif
                    var result = Current.StartCoroutine(enumerator);
                    tcs.TrySetResult(result);
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
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, MonoBehaviour target)
        {
            var tcs = new TaskCompletionSource<Coroutine>();
            
            lock (_defaultExecutionQueue)
            {
                _defaultExecutionQueue.Enqueue(() =>
                {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                    if (!Application.isPlaying)
                    {
                        Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                        return;
                    }
#endif
                    var result = target.StartCoroutine(enumerator);
                    tcs.TrySetResult(result);
                });
            }
            _queuedDefault = true;
            return tcs.Task;
        }
        

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<Coroutine>();
            
            lock (_defaultExecutionQueue)
            {
                _defaultExecutionQueue.Enqueue(() =>
                {
                    try
                    {
                        ct.ThrowIfCancellationRequested();
#if UNITY_EDITOR
                        if (!Application.isPlaying) 
                            throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                        var result = Current.StartCoroutine(enumerator);
                        tcs.TrySetResult(result);
                    }
                    catch (Exception exception)
                    {
                        tcs.TrySetException(exception);
                    }
                });
            }
            _queuedDefault = true;

            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, MonoBehaviour target, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<Coroutine>();

            lock (_defaultExecutionQueue)
            {
                _defaultExecutionQueue.Enqueue(() =>
                {
                    ct.ThrowIfCancellationRequested();
#if UNITY_EDITOR
                    if (!Application.isPlaying) 
                        throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                    var result = target.StartCoroutine(enumerator);
                    tcs.TrySetResult(result);
                });
            }
            
            _queuedDefault = true;
            return tcs.Task;
        }
        
        //--- Async & Execution Cycle

        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<Coroutine>();
            
           Invoke(() =>
           {
               try
               {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                   if (!Application.isPlaying)
                   {
                       Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                       return;
                   }
#endif
                   var result = Current.StartCoroutine(enumerator);
                   tcs.TrySetResult(result);
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
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target)
        {
            var tcs = new TaskCompletionSource<Coroutine>();
            
            Invoke(() =>
            {
#if UNITY_EDITOR && DISPATCHER_DEBUG
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"{nameof(Coroutine)} can only be dispatched in playmode!");
                    return;
                }
#endif
                var result = target.StartCoroutine(enumerator);
                tcs.TrySetResult(result);
                
            }, cycle);
            
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch an <see cref="IEnumerator"/> that will be started and executed as a <see cref="Coroutine"/>
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, ExecutionCycle cycle, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<Coroutine>();
            
            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
#if UNITY_EDITOR
                    if (!Application.isPlaying) 
                        throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                    var result = Current.StartCoroutine(enumerator);
                    tcs.TrySetResult(result);
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
        /// on the main thread; and return a <see cref="Task{Coroutine}"/>, that when awaited on the calling thread, returns
        /// the <see cref="Coroutine"/> after it was started on the main thread.
        /// </summary>
        /// <param name="enumerator"><see cref="IEnumerator"/> that is started as a <see cref="Coroutine"/>.</param>
        /// <param name="cycle"> the execution cycle during which the passed <see cref="Coroutine"/> is started.</param>
        /// <param name="target"> the target <see cref="MonoBehaviour"/> on which the coroutine will run.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="InvalidOperationException"> exception is thrown if an <see cref="IEnumerator"/> is dispatched during edit mode.</exception>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#coroutines-async-cancel">Documentation</a></footer>
        public static Task<Coroutine> InvokeAsync(IEnumerator enumerator, ExecutionCycle cycle, MonoBehaviour target, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<Coroutine>();
            
            Invoke(() =>
            {
                ct.ThrowIfCancellationRequested();
#if UNITY_EDITOR
                if (!Application.isPlaying) 
                    throw new InvalidOperationException($"{nameof(Coroutine)} can only be dispatched in playmode!");
#endif
                var result = target.StartCoroutine(enumerator);
                tcs.TrySetResult(result);
            }, cycle);
            
            return tcs.Task;
        }
        
        
        #endregion
        
        #region --- [DISPATCH: FUNC] ---

        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
        {
            var tcs = new TaskCompletionSource<TResult>();
            Invoke(() =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            });
            return tcs.Task;
        }
       
        
        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Func{T}"/> is executed.</param>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<TResult>();
            Invoke(() =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }, cycle);
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func-cancel">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<TResult>();
            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            });
            return tcs.Task;
        }


        /// <summary>
        /// Dispatch a <see cref="Func{T}"/> that wil executed on the main thread; and return a <see cref="Task{TResult}"/>,
        /// that when awaited on the calling thread, returns the result of the passed <see cref="Func{T}"/>
        /// after it was executed on the main thread.
        /// </summary>
        /// <param name="func"><see cref="Func{T}"/> delegate that will be executed on the main thread.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Func{T}"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <exception cref="OperationCanceledException"> exception is thrown if the task is cancelled prematurely.</exception>
        /// <returns>Task that will complete on the calling thread after the delegate was executed.</returns>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#func-cancel">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<TResult> func, ExecutionCycle cycle, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = new TaskCompletionSource<TResult>();
            Invoke(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception exception)
                {
                    if (!tcs.TrySetException(exception))
                    {
                        Debug.LogException(exception);
                    }
                }
            }, cycle);
            return tcs.Task;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DISPATCH: TASK] ---
        
        
        /// <summary>
        /// Dispatch the execution of a <see cref="Task"/> to the main thread; and return a <see cref="Task"/>,
        /// that can be awaited.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// Use <see cref="InvokeAsync(System.Threading.Tasks.Task, ExecutionCycle)"/>
        /// for more control over the cycle in which the dispatched <see cref="Task"/> is executed.
        /// </summary>
        /// <param name="task"><see cref="Task"/> dispatched task.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Task task)
        {
            var tcs = new TaskCompletionSource<object>();
            
            async void Action()
            {
                try
                {
                    await task;
                    tcs.SetResult(null);
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
        /// <param name="task"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Task task, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<object>();
            
            async void Action()
            {
                try
                {
                    await task;
                    tcs.SetResult(null);
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
        /// Use <see cref="InvokeAsync(System.Threading.Tasks.Task, ExecutionCycle)"/>
        /// for more control over the cycle in which the dispatched <see cref="Task"/> is executed.
        /// </summary>
        /// <param name="task"><see cref="Task"/> dispatched task.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Task task, CancellationToken ct, bool throwOnCancellation = true)
        {
            var tcs = new TaskCompletionSource<object>();
            
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
                            tcs.SetResult(null);
                            return;
                        }
                    }
                    await task;
                    tcs.SetResult(null);
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
        /// <param name="task"><see cref="Task"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <param name="throwOnCancellation"> </param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task">Documentation</a></footer>
        public static Task InvokeAsync(Task task, ExecutionCycle cycle, CancellationToken ct, bool throwOnCancellation = true)
        {
            var tcs = new TaskCompletionSource<object>();
            
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
                            tcs.SetResult(null);
                            return;
                        }
                    }
                    await task;
                    tcs.SetResult(null);
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
        
        #region --- [DISPATCH: TASK<TRESULT>] ---
        
        
        /// <summary>
        /// Dispatch the execution of a <see cref="Task{TResult}"/> to the main thread; and return a <see cref="Task{TResult}"/>,
        /// that can be awaited on the calling thread after and will yield the result of the passed <see cref="Task{TResult}"/>.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// Use <see cref="InvokeAsync(System.Threading.Tasks.Task, ExecutionCycle)"/>
        /// for more control over the cycle in which the dispatched <see cref="Task{TResult}"/> is executed.
        /// </summary>
        /// <param name="task"><see cref="Task{TResult}"/> dispatched task.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-TResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Task<TResult> task)
        {
            var tcs = new TaskCompletionSource<TResult>();
            
            async void Action()
            {
                try
                {
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
        /// Dispatch the execution of a <see cref="Task{TResult}"/> to the main thread; and return a <see cref="Task{TResult}"/>,
        /// that can be awaited on the calling thread after and will yield the result of the passed <see cref="Task{TResult}"/>.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="task"><see cref="Task{TResult}"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task{TResult}"/> is executed.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-TResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Task<TResult> task, ExecutionCycle cycle)
        {
            var tcs = new TaskCompletionSource<TResult>();
            
            async void Action()
            {
                try
                {
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
        /// Dispatch the execution of a <see cref="Task{TResult}"/> to the main thread; and return a <see cref="Task{TResult}"/>,
        /// that can be awaited on the calling thread after and will yield the result of the passed <see cref="Task{TResult}"/>.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// Use <see cref="InvokeAsync(System.Threading.Tasks.Task, ExecutionCycle)"/>
        /// for more control over the cycle in which the dispatched <see cref="Task{TResult}"/> is executed.
        /// </summary>
        /// <param name="task"><see cref="Task{TResult}"/> dispatched task.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-TResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Task<TResult> task, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();
            
            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
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
        /// Dispatch the execution of a <see cref="Task{TResult}"/> to the main thread; and return a <see cref="Task{TResult}"/>,
        /// that can be awaited on the calling thread after and will yield the result of the passed <see cref="Task{TResult}"/>.
        /// Tasks are by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="task"><see cref="Task{TResult}"/> dispatched task.</param>
        /// <param name="cycle">The execution cycle during which the passed <see cref="Task{TResult}"/> is executed.</param>
        /// <param name="ct"> optional cancellation token that can be passed to abort the task prematurely.</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#task-TResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Task<TResult> task, ExecutionCycle cycle, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();
            
            async void Action()
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
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
        
        #region --- [DISPATCH: FUNC<TASK<TRESULT>>] ---

        
        /// <summary>
        /// Dispatch the execution of a <see cref="Func{TResult}"/> to the main thread, which yields a <see cref="Task{TResult}"/>
        /// that will then be executed on the main thread. This call returns a <see cref="Task{TResult}"/> that when awaited
        /// will yield the result of the <see cref="Task{TResult}"/> executed on the main thread.
        /// The passed delegate is by default executed during the next available<br/>
        /// Update, FixedUpdate, LateUpdate or TickUpdate cycle.<br/>
        /// </summary>
        /// <param name="func"><see cref="Func{TResult}"/> dispatched function that yields a <see cref="Task{TResult}"/> .</param>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#funcTaskTResult">Documentation</a></footer>
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
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#funcTaskTResult">Documentation</a></footer>
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
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#funcTaskTResult">Documentation</a></footer>
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
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#funcTaskTResult">Documentation</a></footer>
        public static Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func, ExecutionCycle cycle, CancellationToken ct)
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
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [FIELDS: PRIVATE] ---
        
        private static readonly Thread _mainThread = Thread.CurrentThread;
        
        private static readonly Queue<Action> _defaultExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedDefault = false;
        
#if !DISPATCHER_DISABLE_FIXEDUPDATE
        private static readonly Queue<Action> _fixedUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedFixed = false;
#endif
        
        
#if !DISPATCHER_DISABLE_UPDATE
        private static readonly Queue<Action> _updateExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedUpdate = false;
#endif
        
        
#if !DISPATCHER_DISABLE_FIXEDUPDATE
        private static readonly Queue<Action> _lateUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedLate = false;
#endif
        
        
#if !DISPATCHER_DISABLE_POSTUPDATE
        private static readonly Queue<Action> _postUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedPost = false;
#endif
        
        
#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
        private static readonly Queue<Action> _tickExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedTick = false;
#endif
        
#if UNITY_EDITOR && !DISPATCHER_DISABLE_EDITORUPDATE
        private static readonly Queue<Action> _editorExecutionQueue = new Queue<Action>(10);
        private static volatile bool _queuedEditor = false;
#endif



        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SINGLETON] ---

        private static Dispatcher _current;
        private bool _throw = true;
        
        /// <summary>
        /// Get the current instance of <see cref="Dispatcher"/>. If no instance can be found a new object is created.
        /// </summary>
        public static Dispatcher Current
        {
            get
            {
                if (_current == null)
                {
                    _current = FindObjectOfType<Dispatcher>() ?? new GameObject($"{nameof(Dispatcher)}")
                        .AddComponent<Dispatcher>();
                }
                return _current;
            }
        }
        
        private void Awake()
        {
            if(this == null) return;
            
            if (Current != null && Current != this)
            {
                Debug.LogWarning($"Multiple Dispatcher detected! Destroying {gameObject.name} Please ensure that there " +
                                 $"is only one Dispatcher in your scene!");
                _throw = false;
                Destroy(gameObject);
                return;
            }
            
            _current = this;

            DontDestroyOnLoad(gameObject);
        }


        private void OnDestroy()
        {
            if (Current != this) return;
            _current = null;
            
            if (_throw && gameObject.scene.isLoaded)
            {
                ValidateDispatcher();
                throw new Exception(
                    $"{nameof(Dispatcher)} was destroyed during playmode." +
                    $"Please ensure that the {nameof(Dispatcher)} Scene Object / Component" +
                    $"is not destroyed during playmode!");
            }
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INITIALIZE] ---
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitSceneComponent()
        {
            if (_current == null)
            {
                _current = FindObjectOfType<Dispatcher>() ?? new GameObject($"{nameof(Dispatcher)}")
                    .AddComponent<Dispatcher>();
            }
        }
        

#if UNITY_EDITOR
        static Dispatcher()
        {
#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
            InitializeTickUpdateLoop();
#endif
            
#if !DISPATCHER_DISABLE_EDITORUPDATE
            UnityEditor.EditorApplication.update += EditorUpdate;
#endif
        }
#endif

#if UNITY_EDITOR && !DISPATCHER_DISABLE_EDITORUPDATE
        private static void EditorUpdate()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.EditorUpdate;      
#endif
            if (_queuedEditor)
            {
                lock (_editorExecutionQueue)
                {
                    while (_editorExecutionQueue.Count > 0)
                    {
                        _editorExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedEditor = false;
                }
            }
            
            ReleaseDefaultQueue();
        }
#endif
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void DispatchAfterAssembliesLoaded() => ReleaseDefaultQueue();
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DispatchBeforeSceneLoad() => ReleaseDefaultQueue();
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void DispatchAfterSceneLoad() => ReleaseDefaultQueue();
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [UPDATE: TICK] ---

#if !DISPATCHER_DISABLE_TICKUPDATE || ENABLE_TICKFALLBACK
        
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private const int TICK_DELAY = 50;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitTick() => InitializeTickUpdateLoop();
        
        private static void InitializeTickUpdateLoop()
        {
            StopTick();
            _cts = new CancellationTokenSource();
            TickUpdate(_cts.Token);
        }

        private static void StopTick()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        
        private static async void TickUpdate(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
#if DISPATCHER_DEBUG
                CurrentCycle = ExecutionCycle.TickUpdate;      
#endif
                if (_queuedTick)
                {
                    lock (_tickExecutionQueue)
                    {
                        while (_tickExecutionQueue.Count > 0)
                        {
                            _tickExecutionQueue.Dequeue().Invoke();
                        }
                    }
                }

                ReleaseDefaultQueue();
                
                await Task.Delay(TICK_DELAY, ct);
            }
        }
#endif
        
        #endregion
        
        #region --- [UPDATE: MONOBEHAVIOUR] ---
        
#if !DISPATCHER_DISABLE_UPDATE
        private void Update()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.Update;      
#endif
            if (_queuedUpdate)
            {
                lock (_updateExecutionQueue)
                {
                    while (_updateExecutionQueue.Count > 0)
                    {
                        _updateExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedUpdate = false;
                }
            }
            
            ReleaseDefaultQueue();
        }
#endif
        
#if !DISPATCHER_DISABLE_LATEUPDATE
        private void LateUpdate()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.LateUpdate;      
#endif
            if (_queuedLate)
            {
                lock (_lateUpdateExecutionQueue)
                {
                    while (_lateUpdateExecutionQueue.Count > 0)
                    {
                        _lateUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedLate = false;
                }
            }
            
            ReleaseDefaultQueue();
        }
#endif
        
#if !DISPATCHER_DISABLE_FIXEDUPDATE
        private void FixedUpdate()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.FixedUpdate;      
#endif
            if (_queuedFixed)
            {
                lock (_fixedUpdateExecutionQueue)
                {
                    while (_fixedUpdateExecutionQueue.Count > 0)
                    {
                        _fixedUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedFixed = false;
                }
            }
            
            ReleaseDefaultQueue();
        }
#endif

#if !DISPATCHER_DISABLE_POSTUPDATE
        internal static void PostUpdate()
        {
#if DISPATCHER_DEBUG
            CurrentCycle = ExecutionCycle.PostUpdate;      
#endif
            if (_queuedPost)
            {
                lock (_postUpdateExecutionQueue)
                {
                    while (_postUpdateExecutionQueue.Count > 0)
                    {
                        _postUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    _queuedPost = false;
                }
            }

            ReleaseDefaultQueue();
        }
#endif

        #endregion

        #region --- [RELEASE] ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReleaseDefaultQueue()
        {
            if (!_queuedDefault) return;
            
            lock (_defaultExecutionQueue)
            {
                while (_defaultExecutionQueue.Count > 0)
                {
                    _defaultExecutionQueue.Dequeue()?.Invoke();
                }

                _queuedDefault = false;
            }
        }

        #endregion
    }
}

