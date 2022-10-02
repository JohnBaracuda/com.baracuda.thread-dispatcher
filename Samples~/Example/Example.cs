using Baracuda.Threading.Coroutines;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Baracuda.Threading.Demo
{
    /// <summary>
    /// Example class, showcasing basic capabilities of the Thread Dispatcher Asset.
    /// Please read the documentation online for detailed information.<br/>
    /// <a href="https://johnbaracuda.com/dispatcher.html">View Documentation</a>
    /// </summary>
    public class Example : MonoBehaviour
    {
        [SerializeField] private Text actionText;
        [SerializeField] private Text funcText;
        [SerializeField] private Text coroutineText;
        [SerializeField] private bool throwExceptionInCoroutine = false;
        [SerializeField] private Text coroutineExceptionText;
        [SerializeField] private Text taskText;

        private void Start()
        {
            StartActionExample();
            StartFuncExample();
            StartCoroutineExample();
            StartCoroutineExampleWithException();
            StartTaskExample();
        }

        //--------------------------------------------------------------------------------------------------------------

        #region --- Example: Action ---

        private void StartActionExample()
        {
            Task.Run(ActionExampleWorker);
        }

        private async Task ActionExampleWorker()
        {
            // caching the current thread id
            var threadID = Thread.CurrentThread.ManagedThreadId;

            // simulating async work
            await Task.Delay(1000);


            await Dispatcher.InvokeAsync(() =>
            {
                actionText.text = $"Dispatched from Thread: {threadID:00}";
            });
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Example: Func<TResult> ---

        private void StartFuncExample()
        {
            Task.Run(FuncExampleWorker);
        }

        private async Task FuncExampleWorker()
        {
            // caching the current thread id
            var threadID = Thread.CurrentThread.ManagedThreadId;

            // simulating async work
            await Task.Delay(1000);

            var dispatcherName = await Dispatcher.InvokeAsync(() => FindObjectOfType<Example>().gameObject.name);

            // simulating async work
            await Task.Delay(1000);

            await Dispatcher.InvokeAsync(() =>
            {
                funcText.text = $"Example GameObject is '{dispatcherName}' " +
                                $"Dispatched from thread: {threadID:00}";
            });
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Example Coroutine ---

        private void StartCoroutineExample()
        {
            Task.Run(() => CoroutineExampleWorker(Dispatcher.RuntimeToken), Dispatcher.RuntimeToken);
        }

        private async Task CoroutineExampleWorker(CancellationToken ct)
        {
            // caching the current thread id
            var threadID = Thread.CurrentThread.ManagedThreadId;

            // simulating async work
            await Task.Delay(1000, ct);

            await Dispatcher.InvokeAsyncAwaitStart(ExampleCoroutine(threadID), ct);
        }

        private IEnumerator ExampleCoroutine(int threadId)
        {
            var value = 0;
            while (true)
            {
                yield return new WaitForSeconds(.5f);
                coroutineText.text = $"Working: {++value:000}% Completed | Dispatched from thread: {threadId:00}";

                if (value >= 100)
                {
                    break;
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Example: Coroutine ---

        private void StartCoroutineExampleWithException()
        {
            Task.Run(() => CoroutineExampleWorkerWithException(Dispatcher.RuntimeToken), Dispatcher.RuntimeToken);
        }

        private async Task CoroutineExampleWorkerWithException(CancellationToken ct)
        {
            // caching the current thread id
            var threadID = Thread.CurrentThread.ManagedThreadId;

            // simulating async work
            await Task.Delay(1000, ct);

            try
            {
                await Dispatcher.InvokeAsyncAwaitCompletion(ExampleCoroutineWithException(threadID), ct);
            }
            catch (BehaviourDisabledException behaviourDisabledException)
            {
                // This exception is thrown when the coroutines target behaviour is disabled which will also happen
                // when exiting playmode while the coroutine is still running.
                Debug.Log(behaviourDisabledException.Message);
                return;
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                await Dispatcher.InvokeAsync(() => coroutineExceptionText.text = $"{exception.GetType().Name} Occured!", ct);
                return;
            }

            await Dispatcher.InvokeAsync(() => coroutineExceptionText.text = "Work Completed!", ct);
        }

        private IEnumerator ExampleCoroutineWithException(int threadId)
        {
            var value = 0;
            while (true)
            {
                yield return new WaitForSeconds(.1f);
                coroutineExceptionText.text = $"Working: {++value:000}% Completed | Dispatched from thread: {threadId:00}";

                if (throwExceptionInCoroutine && value >= 5)
                {
                    throw new InvalidOperationException("This Exception is thrown inside a Coroutine!");
                }

                if (value >= 100)
                {
                    break;
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Example: Task ---

        private void StartTaskExample()
        {
            Task.Run(() => TaskExampleWorker(Dispatcher.RuntimeToken));
        }

        private async Task TaskExampleWorker(CancellationToken ct)
        {
            try
            {
                // caching the current thread id
                var threadID = Thread.CurrentThread.ManagedThreadId;

                // simulating async work
                await Task.Delay(2000, ct);
                Debug.Log("Start");
                var result = await Dispatcher.InvokeAsync(TaskExampleMainThread, ct)
                    .TimeoutAsync(1500, ct)
                    .IgnoreTimeoutExceptionAsync()
                    .IgnoreOperationCanceledExceptionAsync();

                await Dispatcher.InvokeAsync(() =>
                {
                    taskText.text = $"Waited {result:00} ms on the main thread! | Dispatched from thread: {threadID:00}";
                });
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
            }
        }

        private async Task<int> TaskExampleMainThread(CancellationToken ct)
        {
            var random = Random.Range(1000, 2000);

            await Task.Delay(random, ct);

            return random;
        }

        #endregion
    }
}
