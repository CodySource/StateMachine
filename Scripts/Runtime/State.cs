using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
            public void _Enter() => callbacks.FindAll(c => c.conditions.HasFlag(CallbackCondition.Enter)).ForEach(c => c.onInvoke?.Invoke());

            /// <summary>
            /// Invoke the state's update callbacks.
            /// </summary>
            public void _Update() => callbacks.FindAll(c => c.conditions.HasFlag(CallbackCondition.Update)).ForEach(c => c.onInvoke?.Invoke());

            /// <summary>
            /// Invoke the state's fixed update callbacks.
            /// </summary>
            public void _FixedUpdate() => callbacks.FindAll(c => c.conditions.HasFlag(CallbackCondition.FixedUpdate)).ForEach(c => c.onInvoke?.Invoke());

            /// <summary>
            /// Exit the state by calling the exit state callbacks.
            /// </summary>
            public void _Exit() => callbacks.FindAll(c => c.conditions.HasFlag(CallbackCondition.Exit)).ForEach(c => c.onInvoke?.Invoke());

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
                public CallbackCondition conditions;
                public UnityEvent onInvoke = new UnityEvent();
            }

            #endregion

        }
    }
}