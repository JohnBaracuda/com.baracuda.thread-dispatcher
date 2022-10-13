using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Threading
{
    public sealed partial class Dispatcher
    {
        /// <summary>
        /// Get the current instance of <see cref="Dispatcher"/>. If no instance can be found a new object is created.<br/>
        /// This property is only allowed to be accessed from the main thread!
        /// </summary>
        public static Dispatcher Current => GetOrCreateDispatcherInstance();

        /// <summary>
        /// Promises to return a valid dispatcher instance.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dispatcher GetOrCreateDispatcherInstance()
        {
            if (current != null)
            {
                return current;
            }

            GuardAgainstIsNotMainThread(nameof(Current));
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                current = FindObjectOfType<Dispatcher>() ?? new GameObject(nameof(Dispatcher)).AddComponent<Dispatcher>();
                return current;
            }
#endif
            current = new GameObject(nameof(Dispatcher)).AddComponent<Dispatcher>();
            return current;
        }

        private static Dispatcher current;

        /// <summary>
        /// flag to determine if an invalid operation exception should be thrown when destroying the GameObject.
        /// </summary>
        private bool _throw = true;

        private void Awake()
        {
            if (this == null)
            {
                return;
            }

            if (current != null && current != this)
            {
                Debug.LogWarning($"Multiple Dispatcher detected! Destroying {gameObject.name} Please ensure that there is only one Dispatcher in your scene!");
                _throw = false;
                Destroy(gameObject);
                return;
            }

            current = this;

            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = runtimeHideFlags;
        }


        private void OnDestroy()
        {
            if (Current != this)
            {
                return;
            }

            current = null;

            if (_throw && gameObject.scene.isLoaded)
            {
                Validate();
                throw new InvalidOperationException(
                    $"{nameof(Dispatcher)} was destroyed during playmode. Please ensure that the {nameof(Dispatcher)} is not destroyed during playmode!");
            }
        }
    }
}

