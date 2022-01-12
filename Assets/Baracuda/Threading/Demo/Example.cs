using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.Threading.Demo
{
    /// <summary>
    /// Example class, showcasing basic capabilities of the Thread Dispatcher Asset.
    /// Please read the documentation online for detailed information.<br/>
    /// <a href="https://johnbaracuda.com/dispatcher.html">View Documentation</a>
    /// </summary>
    public class Example : MonoBehaviour
    {
        [SerializeField] private Text dispatchActionText;
        [SerializeField] private Text dispatchFuncText;
        [SerializeField] private Text dispatchCoroutineText;
        [SerializeField] private bool throwException;
        [SerializeField] private Text dispatchCoroutineExceptionText;
        
        private void Awake()
        {
            // Initialize tasks that will run on a separate thread.
            
            // Action example
            Task.Run(() => DispatchExampleTask(WorkCompleted));
            
            // Func<TResult> example
            Task.Run(DispatchFuncExampleTask);
            
            // Coroutine example
            Task.Run(DispatchCoroutineExampleTask);
            
            // Coroutine with exception
            Task.Run(() => DispatchCoroutineExampleTaskWithException(throwException));
            
        }
        
        

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DISPATCH ACTION EXAMPLE] ---

        /// <summary>
        /// <see cref="Action"/> example: <br/>
        /// <a href="https://johnbaracuda.com/dispatcher.html#actions">View Documentation</a>
        /// </summary>
        private Task DispatchExampleTask(Action<string> callback)
        {
            // cache the current thread id.
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            
            
            // simulate asynchronous work.
            for (var i = 0; i < 100; i++)
            {
                var iteration = i.ToString("000");
                
                // Dispatch the anonymous delegate, showcasing the current progress to the main thread.
                Dispatcher.Invoke(() =>
                {
                    if(!Application.isPlaying) return;
                    dispatchActionText.text = $"Doing Work On Thread: {currentThreadId.ToString()} | {iteration}%";
                });
                Thread.Sleep(100);
            }
            
            // Dispatch the anonymous delegate to the main thread.
            Dispatcher.Invoke(() =>
            {
                if(!Application.isPlaying) return;
                dispatchActionText.text = $"Completed Work On Thread: {currentThreadId.ToString()}";
            });

            // Dispatch the passed callback action to the main thread.
            callback.Dispatch(currentThreadId.ToString());
            
            return Task.CompletedTask;
        }

        private void WorkCompleted(string currentThreadId)
        {
            if(!Application.isPlaying) return;
            dispatchActionText.text = $"Completed Work On Thread: {currentThreadId}";
        }
        
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DISPATCH FUNC EXAMPLE] ---

        /// <summary>
        /// <see cref="Func{TResult}"/> example: <br/>
        /// <a href="https://johnbaracuda.com/dispatcher.html#func">View Documentation</a>
        /// </summary>
        private async Task DispatchFuncExampleTask()
        {
            // simulate asynchronous work.
            await Task.Delay(1000);
            
            // cache the current thread id.
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;

            // get the position of this GameObject.
            var position = await Dispatcher.InvokeAsync(() => gameObject.transform.position);
            
            await Dispatcher.InvokeAsync(() =>
            {
                if(!Application.isPlaying) return;
                dispatchFuncText.text = 
                    $"The position of this GameObject is {position.ToString()}! " +
                    $"Dispatched from Thread: {currentThreadId.ToString()}";
            });
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [COROUTINE] ---

        /// <summary>
        /// <see cref="Coroutine"/> example: <br/>
        /// <a href="https://johnbaracuda.com/dispatcher.html#coroutines">View Documentation</a>
        /// </summary>
        private async Task DispatchCoroutineExampleTask()
        {
            await Task.Delay(1000);
                
            // cache the current thread id.
            var currentThreadId = Thread.CurrentThread.ManagedThreadId.ToString();

            // dispatch the execution of the ExampleCoroutine to the main thread.
            Dispatcher.Invoke(ExampleCoroutine(currentThreadId),ExecutionCycle.Update, this);
        }

        private IEnumerator ExampleCoroutine(string threadId)
        {
            var iteration = 0;
            while (Application.isPlaying)
            {
                yield return new WaitForSeconds(1f);
                dispatchCoroutineText.text = $"Dispatched from Thread: {threadId} " +
                                             $"| Iteration: {iteration++.ToString()}";
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [DISPATCH COROUTINE WITH EXCEPTION] ---

        /// <summary>
        /// <see cref="Coroutine"/> example: <br/>
        /// <a href="https://johnbaracuda.com/dispatcher.html#coroutines">View Documentation</a>
        /// </summary>
        private async Task DispatchCoroutineExampleTaskWithException(bool throwExceptions)
        {
            await Task.Delay(1000);
                
            // cache the current thread id.
            var currentThreadId = Thread.CurrentThread.ManagedThreadId.ToString();

            try
            {
                // dispatch the execution of the ExampleCoroutine to the main thread.
                await Dispatcher.InvokeCoroutineAsync(ExampleCoroutineWithException(currentThreadId), throwExceptions);
                
                Debug.Log("Coroutine Completed");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private IEnumerator ExampleCoroutineWithException(string threadId)
        {
            var iteration = 0;
            while (Application.isPlaying)
            {
                yield return new WaitForSeconds(1f);
                dispatchCoroutineExceptionText.text = $"Dispatched from Thread: {threadId} " +
                                             $"| Iteration: {iteration++.ToString()}";

                if (iteration >= 3)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        
        #endregion
        
    }
}
