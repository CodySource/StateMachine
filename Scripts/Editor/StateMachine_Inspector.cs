using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CodySource
{
    namespace StateMachine
    {
#if UNITY_EDITOR
        [CustomEditor(typeof(StateMachine))]
        public class StateMachine_Inspector : Editor
        {

            /// <summary>
            /// The current state
            /// </summary>
            SerializedProperty _currentState;

            public override void OnInspectorGUI()
            {
                //  Update properties
                serializedObject.UpdateIfRequiredOrScript();

                //  Show the current state during playback
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUI.color = (EditorApplication.isPlaying) ? Color.green : Color.grey;
                string _name = _currentState?.FindPropertyRelative("name")?.stringValue ?? "";
                GUILayout.Box($"Current State: { ((_name != "") ? _name : "-Empty-" ) }", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                //  Reset gui color
                GUI.color = Color.white;

                //  Draw default
                EditorGUILayout.Space();
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();
            }

            private void OnEnable() => _currentState = serializedObject.FindProperty("_currentState");
        }
#endif
    }
}