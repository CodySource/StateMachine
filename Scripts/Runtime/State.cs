using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodySource
{
    namespace StateMachine
    {
        /// <summary>
        /// An available state for a state machine to utilize.
        /// </summary>
        [System.Serializable]
        public class State
        {
            [System.Flags] public enum CallbackCondition { None = 0, Enter = 1, Update = 2, FixedUpdate = 4, Exit = 8, Everything = 15 };

            #region PROPERTIES

            /// <summary>
            /// The name of the state
            /// </summary>
            public string name;

            /// <summary>
            /// State callbacks
            /// </summary>
            public List<Callback> callbacks = new List<Callback>();

            #endregion

            #region PUBLIC METHODS

            /// <summary>
            /// Enter the state by calling the enter state callbacks.
            /// </summary>
            public void _Enter() => callbacks.FindAll(c => c.conditions.HasFlag(CallbackCondition.Enter) && c.active).ForEach(c => c.onInvoke?.Invoke());

            /// <summary>
            /// Invoke the state's update callbacks.
            /// </summary>
            public void _Update() => callbacks.FindAll(c => c.conditions.HasFlag(CallbackCondition.Update) && c.active).ForEach(c => c.onInvoke?.Invoke());

            /// <summary>
            /// Invoke the state's fixed update callbacks.
            /// </summary>
            public void _FixedUpdate() => callbacks.FindAll(c => c.conditions.HasFlag(CallbackCondition.FixedUpdate) && c.active).ForEach(c => c.onInvoke?.Invoke());

            /// <summary>
            /// Exit the state by calling the exit state callbacks.
            /// </summary>
            public void _Exit() => callbacks.FindAll(c => c.conditions.HasFlag(CallbackCondition.Exit) && c.active).ForEach(c => c.onInvoke?.Invoke());

            /// <summary>
            /// Overriding the standard comparison for States to simply check names match.
            /// </summary>
            public static bool operator ==(State _a, State _b)
            {
                if (_a is null)
                {
                    if (_b is null) return true;
                    return false;
                }
                return _a.Equals(_b);
            }
            public static bool operator !=(State _a, State _b) => !(_a == _b);
            public override bool Equals(object other) => this.Equals(other as State);
            public override int GetHashCode() => (name).GetHashCode();
            public bool Equals(State _b)
            {
                if (_b is null) return false;
                if (ReferenceEquals(this, _b)) return true;
                if (GetType() != _b.GetType()) return false;
                return name == _b.name;
            }

            #endregion

            #region PUBLIC CLASSES

            [System.Serializable]
            public class Callback
            {
                public string name = "";
                [IDDrawer] [Tooltip("Click the button to copy the callback Id")]
                public string callbackId = "";
                public CallbackCondition conditions;
                public bool active = true;
                public void SetActive(bool tf) => active = tf;
                public UnityEvent onInvoke = new UnityEvent();
                public class IDDrawer : PropertyAttribute { }
#if UNITY_EDITOR
                [CustomPropertyDrawer(typeof(IDDrawer))]
                public class ReadOnlyAttrDrawer : PropertyDrawer
                {
                    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                    {
                        property.stringValue = (property.stringValue == "") ? System.Guid.NewGuid().ToString() : property.stringValue;
                        EditorGUI.LabelField(position, label);
                        GUI.backgroundColor = Color.yellow;
                        GUI.contentColor = Color.white;
                        Rect buttonPos = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);
                        if (GUI.Button(buttonPos, $"{property.stringValue}")) GUIUtility.systemCopyBuffer = property.stringValue;
                        GUI.backgroundColor = Color.white;
                        GUI.contentColor = Color.white;
                    }
                }
#endif
            }

            #endregion

        }
    }
}