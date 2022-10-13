#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

// ReSharper disable MemberCanBePrivate.Global
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
    [AddComponentMenu("Threading/Dispatcher")]
    public sealed partial class Dispatcher : MonoBehaviour
    {
        /*
         * Configuration
         */

        [Space]
        [Tooltip("Determine the runtime hide flags for the dispatcher scene object")]
        [SerializeField] private HideFlags runtimeHideFlags = HideFlags.HideInHierarchy;

        [Tooltip("Enable / Disable execution of dispatched work during FixedUpdate")]
        [SerializeField] private bool enableFixedUpdate = false;

        [Tooltip("Enable / Disable execution of dispatched work during Update")]
        [SerializeField] private bool enableUpdate = true;

        [Tooltip("Enable / Disable execution of dispatched work during LateUpdate")]
        [SerializeField] private bool enableLateUpdate = true;

        /*
         * Properties & Getter
         */

        /// <summary>
        /// Returns true if called from the main thread and false if not.
        /// </summary>
        /// <returns></returns>
        ///<footer><a href="https://johnbaracuda.com/dispatcher.html#miscellaneous">Documentation</a></footer>
        public static bool IsMainThread() => Thread.CurrentThread.ManagedThreadId == (mainThread?.ManagedThreadId
            ?? throw new Exception($"{nameof(Dispatcher)}.{nameof(mainThread)} is not initialized"));

        /// <summary>
        /// Throws an InvalidOperationException if not called from the main thread.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GuardAgainstIsNotMainThread(string methodCall)
        {
            if (!IsMainThread())
            {
                throw new InvalidOperationException($"{methodCall} is only allowed to bne called from the main thread!");
            }
        }

        /// <summary>
        /// Ensure that a <see cref="Dispatcher"/> instance exists.
        /// </summary>
        /// <returns></returns>
        ///<footer><a href="https://johnbaracuda.com/dispatcher.html#miscellaneous">Documentation</a></footer>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void Validate()
        {
#if UNITY_EDITOR
            Assert.IsNotNull(GetOrCreateDispatcherInstance());
#else
            GetOrCreateDispatcherInstance();
#endif
        }

        /// <summary>
        /// Get the <see cref="ExecutionCycle"/> definition of the currently executed update cycle.
        /// </summary>
        ///<footer><a href="https://johnbaracuda.com/dispatcher.html#cycle">Documentation</a></footer>
        public static ExecutionCycle CurrentCycle { get; private set; } = ExecutionCycle.Update;


        /// <summary>
        /// Return a <see cref="CancellationToken"/> that is valid for the duration of the applications runtime.
        /// This means until OnApplicationQuit is called in a build
        /// or until the play state is changed in the editor.
        /// </summary>
        /// <footer><a href="https://johnbaracuda.com/dispatcher.html#runtimeToken">Documentation</a></footer>
        public static CancellationToken RuntimeToken => runtimeCts.Token;

        /// <summary>
        /// Enable / Disable execution of dispatched work during FixedUpdate
        /// </summary>
        public static bool EnableFixedUpdate
        {
            get => Current.enableFixedUpdate;
            set => Current.enableFixedUpdate = value;
        }

        /// <summary>
        /// Enable / Disable execution of dispatched work during Update
        /// </summary>
        public static bool EnableUpdate
        {
            get => Current.enableUpdate;
            set => Current.enableUpdate = value;
        }

        /// <summary>
        /// Enable / Disable execution of dispatched work during LateUpdate
        /// </summary>
        public static bool EnableLateUpdate
        {
            get => Current.enableLateUpdate;
            set => Current.enableLateUpdate = value;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Enable / Disable execution of dispatched work during editor update<b/>
        /// This property is editor only!
        /// </summary>
        public static bool EnableEditorUpdate { get; set; }
#endif

        //--------------------------------------------------------------------------------------------------------------

        #region --- Private Fields ---

        private static readonly Thread mainThread = Thread.CurrentThread;

        private static CancellationTokenSource runtimeCts = new CancellationTokenSource();

        private static readonly Queue<Action> defaultExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedDefault = false;

        private static readonly Queue<Action> fixedUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedFixed = false;

        private static readonly Queue<Action> updateExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedUpdate = false;

        private static readonly Queue<Action> lateUpdateExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedLate = false;


#if UNITY_EDITOR
        private static readonly Queue<Action> editorExecutionQueue = new Queue<Action>(10);
        private static volatile bool queuedEditor = false;

        static Dispatcher()
        {
            UnityEditor.EditorApplication.playModeStateChanged += change =>
            {

                switch (change)
                {
                    case UnityEditor.PlayModeStateChange.ExitingEditMode:
                    case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                        CancelAndResetRuntimeToken();
                        break;
                }
            };

            UnityEditor.EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            CurrentCycle = ExecutionCycle.EditorUpdate;

            if (queuedEditor)
            {
                lock (editorExecutionQueue)
                {
                    while (editorExecutionQueue.Count > 0)
                    {
                        editorExecutionQueue.Dequeue().Invoke();
                    }

                    queuedEditor = false;
                }
            }

            ReleaseDefaultQueue();
        }
#endif //UNITY_EDITOR

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void DispatchAfterAssembliesLoaded() => ReleaseDefaultQueue();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void DispatchBeforeSceneLoad() => ReleaseDefaultQueue();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void DispatchAfterSceneLoad() => ReleaseDefaultQueue();


        private void OnApplicationQuit()
        {
            CancelAndResetRuntimeToken();
        }

        private static void CancelAndResetRuntimeToken()
        {
            runtimeCts.Cancel();
            runtimeCts.Dispose();
            runtimeCts = new CancellationTokenSource();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Update: Behaviour ---

        private void Update()
        {
            if (!enableUpdate)
            {
                return;
            }
            CurrentCycle = ExecutionCycle.Update;

            if (queuedUpdate)
            {
                lock (updateExecutionQueue)
                {
                    while (updateExecutionQueue.Count > 0)
                    {
                        updateExecutionQueue.Dequeue().Invoke();
                    }

                    queuedUpdate = false;
                }
            }

            ReleaseDefaultQueue();
        }

        private void LateUpdate()
        {
            if (!enableLateUpdate)
            {
                return;
            }
            CurrentCycle = ExecutionCycle.LateUpdate;

            if (queuedLate)
            {
                lock (lateUpdateExecutionQueue)
                {
                    while (lateUpdateExecutionQueue.Count > 0)
                    {
                        lateUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    queuedLate = false;
                }
            }

            ReleaseDefaultQueue();
        }

        private void FixedUpdate()
        {
            if (!enableFixedUpdate)
            {
                return;
            }
            CurrentCycle = ExecutionCycle.FixedUpdate;

            if (queuedFixed)
            {
                lock (fixedUpdateExecutionQueue)
                {
                    while (fixedUpdateExecutionQueue.Count > 0)
                    {
                        fixedUpdateExecutionQueue.Dequeue().Invoke();
                    }

                    queuedFixed = false;
                }
            }

            ReleaseDefaultQueue();
        }

        #endregion

        #region --- Event Queue Release ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReleaseDefaultQueue()
        {
            if (!queuedDefault)
            {
                return;
            }

            lock (defaultExecutionQueue)
            {
                while (defaultExecutionQueue.Count > 0)
                {
                    defaultExecutionQueue.Dequeue()?.Invoke();
                }

                queuedDefault = false;
            }
        }

        #endregion
    }
}

