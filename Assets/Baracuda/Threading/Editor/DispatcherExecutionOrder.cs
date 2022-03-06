using System;
using System.IO;
using Baracuda.Threading.Internal;
using UnityEngine;
using UnityEditor;

namespace Baracuda.Threading.Editor
{
    internal class DispatcherExecutionOrder : ScriptableObject
    {
        /*
         *  Fields   
         */

        [SerializeField][HideInInspector] 
        internal int executionOrder = DEFAULT_EXECUTION_ORDER;
        
        [SerializeField][HideInInspector] 
        internal int postExecutionOrder = DEFAULT_POST_EXECUTION_ORDER;
       
        private static DispatcherExecutionOrder current;
        private const string DEFAULT_PATH = "Assets/Baracuda/Threading/Editor";
        internal const int DEFAULT_EXECUTION_ORDER = -500;
        internal const int DEFAULT_POST_EXECUTION_ORDER = 500;

        private void OnEnable()
        {
            current = this;
        }

        /*
         *  Singleton   
         */

        internal static DispatcherExecutionOrder GetDispatcherExecutionOrderAsset()
        {
            if (current)
            {
                return current;
            }
            else
            {
                return current = LoadDispatcherExecutionOrderAsset();
            }
        }

        private static DispatcherExecutionOrder LoadDispatcherExecutionOrderAsset()
        {
            var paths = new string[]
            {
                "Assets/Baracuda/Threading/Editor/DispatcherExecutionOrder.asset",
                "Assets/Plugins/Baracuda/Threading/Editor/DispatcherExecutionOrder.asset",
                "Assets/Plugins/Threading/Editor/DispatcherExecutionOrder.asset",
                "Assets/Modules/Threading/Editor/DispatcherExecutionOrder.asset",
                "Assets/Modules/Baracuda/Threading/Editor/DispatcherExecutionOrder.asset",
                "Assets/Threading/Editor/DispatcherExecutionOrder.asset",
            };

            DispatcherExecutionOrder tempCurrent;
            
            for (var i = 0; i < paths.Length; i++)
            {
                tempCurrent = AssetDatabase.LoadAssetAtPath<DispatcherExecutionOrder>(paths[i]);
                if (tempCurrent != null)
                {
                    return tempCurrent;
                }
            }
            
            var guids = AssetDatabase.FindAssets($"t:{typeof(DispatcherExecutionOrder)}");
            for(var i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                tempCurrent = AssetDatabase.LoadAssetAtPath<DispatcherExecutionOrder>(assetPath);
                if(tempCurrent != null)
                {
                    return tempCurrent;
                }
            }

            tempCurrent = CreateDispatcherCache();
            if(tempCurrent != null)
            {
                return tempCurrent;
            }

            throw new Exception(
                $"{nameof(DispatcherExecutionOrder)} was not found when calling: {nameof(GetDispatcherExecutionOrderAsset)} and cannot be created!");
        }
        
        private static DispatcherExecutionOrder CreateDispatcherCache()
        {
            try
            {
                Directory.CreateDirectory(DEFAULT_PATH);
                var filePath = $"{DEFAULT_PATH}/{nameof(DispatcherExecutionOrder)}.asset";

                var asset = CreateInstance<DispatcherExecutionOrder>();
                AssetDatabase.CreateAsset(asset, filePath);
                AssetDatabase.SaveAssets();
                
#if DISPATCHER_DEBUG
                Debug.Log($"Created a new {nameof(DispatcherExecutionOrder)} at {DEFAULT_PATH}! ");
#endif
                return asset;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
        }

        /*
         *  Execution Order Logic   
         */

        /// <summary>
        /// Validate and update the current script execution order of the <see cref="Dispatcher"/>.
        /// </summary>
        [InitializeOnLoadMethod]
        public static void ValidateExecutionOrder()
        {
            var target = GetDispatcherExecutionOrderAsset();
            var monoScripts = MonoImporter.GetAllRuntimeMonoScripts();
            for (var i = 0; i < monoScripts.Length; i++)
            {
                UpdateExecutionOrderForType<Dispatcher>(monoScripts[i], target.executionOrder);
                UpdateExecutionOrderForType<DispatcherPostUpdate>(monoScripts[i], target.postExecutionOrder);
            }
        }

        private static void UpdateExecutionOrderForType<T>(MonoScript target, int newOrder) where T : MonoBehaviour
        {
            if (target.GetClass() != typeof(T))
            {
                return;
            }
            
            var currentOrder = MonoImporter.GetExecutionOrder(target);

            if (currentOrder == newOrder)
            {
                return;
            }
            
            Debug.Log($"Setting the 'Script Execution Order' of {target.name} from {currentOrder} to {newOrder}");
            MonoImporter.SetExecutionOrder(target, newOrder);
        }
    }
}