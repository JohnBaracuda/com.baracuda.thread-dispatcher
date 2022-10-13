using System;

namespace Baracuda.Threading
{
    /// <summary>
    /// Enum representing the main update cycles in which a <see cref="Delegate"/>
    /// can be invoked.
    /// </summary>
    /// <footer><a href="https://johnbaracuda.com/dispatcher.html#cycle">Documentation</a></footer>
    public enum ExecutionCycle
    {
        /// <summary>
        /// <see cref="Delegate"/> is executed at the beginning of the next Update call.
        /// </summary>
        Update = 1,

        /// <summary>
        /// <see cref="Delegate"/> is executed at the beginning of the next LateUpdate call.
        /// </summary>
        LateUpdate = 2,

        /// <summary>
        /// <see cref="Delegate"/> is executed at the beginning of the next FixedUpdate call.
        /// </summary>
        FixedUpdate = 3,

#if UNITY_EDITOR
        /// <summary>
        /// <see cref="Delegate"/> is executed at the beginning of the next editor update call.
        /// </summary>
        /// <footer><a href="https://docs.unity3d.com/ScriptReference/EditorApplication-update.html">Documentation</a></footer>
        EditorUpdate = 5,
#endif
    }
}