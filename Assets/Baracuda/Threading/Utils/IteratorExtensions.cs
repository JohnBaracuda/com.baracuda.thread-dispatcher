using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Baracuda.Threading.Internal
{
    public static class IteratorExtensions
    {
        public static void StartCoroutineExceptionSensitive(
            this MonoBehaviour target,
            IEnumerator enumerator,
            Func<Exception, bool> error,
            Func<object, bool> completed
            ) 
            => StartCoroutineExceptionSensitive(target, enumerator, error, completed, CancellationToken.None);
        
        public static void StartCoroutineExceptionSensitive(
            this MonoBehaviour target,
            IEnumerator enumerator,
            Func<Exception, bool> error,
            Func<object, bool> completed,
            CancellationToken ct
        )
        {
            // Because coroutines do not return if their target behaviour was disabled, we have to manually add a 
            // component that will give us a callback if the target behaviour is disabled.
            if (!target.TryGetComponent<IDisableCallback>(out var callbackComponent))
            {
                callbackComponent = target.gameObject.AddComponent<DisableCallback>();
            }
            target.StartCoroutine(StartCoroutineExceptionSensitive(enumerator, error, completed, callbackComponent, ct));
        }
        

        /// <summary>
        /// Run an iterator function that might throw an exception. Call the callback with the exception
        /// if it does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="enumerator">Iterator function to run</param>
        /// <param name="error">Callback invoked when the iterator has thrown an exception.</param>
        /// <param name="completed">Callback invoked when the iterator has finished.</param>
        /// <param name="callback"></param>
        /// <param name="ct"></param>
        /// <returns>An enumerator that runs the given enumerator</returns>
        private static IEnumerator StartCoroutineExceptionSensitive(
            IEnumerator enumerator,
            Func<Exception, bool> error,
            Func<object, bool> completed,
            IDisableCallback callback,
            CancellationToken ct
        )
        {
            // allocating local method so we can unsubscribe it later to prevent memory allocations.
            void OnDisable() => error(new InvalidOperationException("Target Behaviour for iterator was disabled!"));

            callback.onDisable += OnDisable;
            while (true)
            {
                object current;
                try
                {
                    if (enumerator.MoveNext() == false)
                    {
                        completed(null);
                        callback.onDisable -= OnDisable;
                        break;
                    }
                    current = enumerator.Current;
                    ct.ThrowIfCancellationRequested();
                }
                catch (Exception exception)
                {
                    error(exception);
                    callback.onDisable -= OnDisable;
                    yield break;
                }
                yield return current;
            }
        }
    }
}