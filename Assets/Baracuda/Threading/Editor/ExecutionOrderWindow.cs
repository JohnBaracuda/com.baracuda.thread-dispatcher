using UnityEditor;
using UnityEngine;

namespace Baracuda.Threading.Editor
{
    internal class ExecutionOrderWindow : EditorWindow
    {
        /*
         *  Fields   
         */

        private DispatcherExecutionOrder _target = null;
        
        private readonly GUIContent _executionOrderContent = new GUIContent("Main Execution Order", 
            "Set the script execution order for the Dispatcher.");
        
        private readonly GUIContent _postExecutionOrderContent = new GUIContent("Post Update Order", 
            "Set the script execution order for the post update call of the Dispatcher.");

        /*
         *  Setup   
         */

        internal static void Open()
        {
            var window = GetWindow<ExecutionOrderWindow>("Execution Order");
            window._target = DispatcherExecutionOrder.GetDispatcherExecutionOrderAsset();
            window.Show(true);
        }

        private void OnDisable()
        {
            DispatcherExecutionOrder.ValidateExecutionOrder();
        }

        /*
         *  GUI   
         */

        public void OnGUI()
        {
            var style = GUI.skin.GetStyle("HelpBox");
            style.fontSize = 12;
            style.richText = true;
            EditorGUILayout.TextArea("The <b>'Dispatcher Execution Order'</b> affects the script execution order of the Dispatchers Update, LateUpdate and FixedUpdate." +
                                     "The <b>'Post Update Execution Order'</b> affects the script execution order of the Dispatchers PostUpdate." +
                                     "Post Update is technically another LateUpdate call, that is invoked on another component and then forwarded to the Dispatcher.", style);
            
            GUILayout.Space(5);
            
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Execution Order");
            _target.executionOrder = EditorGUILayout.IntField(_executionOrderContent, _target.executionOrder);
            _target.postExecutionOrder = EditorGUILayout.IntField(_postExecutionOrderContent, _target.postExecutionOrder);
            GUILayout.EndVertical();

            if (_target.executionOrder >= 0)
            {
                EditorGUILayout.HelpBox("Dispatcher Execution Order should be less then 0 to ensure that the Dispatcher is called before the default execution time.", MessageType.Warning);
            }

            if (_target.postExecutionOrder <= 0)
            {
                EditorGUILayout.HelpBox("Post Update Execution Order should be greater then 0 to ensure that the Post Update is called after the default execution time.", MessageType.Warning);
            }

            if (_target.executionOrder > _target.postExecutionOrder)
            {
                EditorGUILayout.HelpBox("Dispatcher Execution Order should be less then Post Update Execution Order to ensure that the Dispatchers PostUpdate is called after the the Dispatchers LateUpdate!", MessageType.Warning);
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Documentation", GUILayout.MinWidth(120)))
            {
                Application.OpenURL("https://johnbaracuda.com/dispatcher.html#execution-order");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset", GUILayout.MinWidth(120)))
            {
                _target.executionOrder = DispatcherExecutionOrder.DEFAULT_EXECUTION_ORDER;
                _target.postExecutionOrder = DispatcherExecutionOrder.DEFAULT_POST_EXECUTION_ORDER;
            }
            if (GUILayout.Button("Save", GUILayout.MinWidth(120)))
            {
                DispatcherExecutionOrder.ValidateExecutionOrder();
            }
            GUILayout.EndHorizontal();

            EditorUtility.SetDirty(_target);
        }
    }
}