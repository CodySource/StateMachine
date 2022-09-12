using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CodySource
{
    namespace StateMachine
    { 
        /// <summary>
        /// This is a per-object state machine that can be used to drive state-based scene behaviour.
        /// </summary>
        public class StateMachine : MonoBehaviour
        {

            #region PROPERTIES

            /// <summary>
            /// The registry for all active substatemachines
            /// </summary>
            public static List<StateMachine> allMachines = new List<StateMachine>();

            /// <summary>
            /// External reference for the next state.
            /// </summary>
            public State nextState => _nextState;

            /// <summary>
            /// External reference for the current state.
            /// </summary>
            public State currentState => _currentState;

            /// <summary>
            /// External reference for the cached state.
            /// </summary>
            public State cachedState => _cachedState;

            /// <summary>
            /// This event is fired whenever the state of the machine is changed
            /// </summary>
            public UnityEvent<StateChangeEventInformation> onStateChange { get; private set; } = new UnityEvent<StateChangeEventInformation>();

            /// <summary>
            /// The conditions triggered by the statemachine
            /// </summary>
            public State.CallbackCondition conditions = State.CallbackCondition.Everything;

            /// <summary>
            /// A flag to dictate whether or not the exit callbacks are fired when the statemachine is destroyed
            /// </summary>
            public bool isExitInvokedOnDestroy = false;

            /// <summary>
            /// The available states that the state machine can switch between.
            /// </summary>
            [SerializeField] private List<State> _availableStates = new List<State>();

            /// <summary>
            /// A public accessor to the available states.
            /// </summary>
            public List<State> availableStates => _availableStates;

            /// <summary>
            /// The current state of the state machine
            /// </summary>
            [HideInInspector] [SerializeField] private State _currentState = null;

            /// <summary>
            /// The next state of the state machine
            /// </summary>
            [HideInInspector] [SerializeField] private State _nextState = null;

            /// <summary>
            /// The cached state
            /// </summary>
            private State _cachedState = null;

            /// <summary>
            /// The current state change information
            /// </summary>
            private StateChangeEventInformation _info = new StateChangeEventInformation();

            #endregion

            #region PUBLIC METHODS

            /// <summary>
            /// Caches the current state so that it may be retreived later.
            /// </summary>
            public void CacheState() => _cachedState = currentState;

            /// <summary>
            /// Resumes the cached state.
            /// </summary>
            public void ResumeCachedState(bool pInvokeExit)
            {
                if (pInvokeExit) _ExitState();
                _info = new StateChangeEventInformation()
                {
                    inboundState = _cachedState?.name ?? "-Empty-",
                    outboundState = _currentState?.name ?? "-Empty-",
                    reason = StateChangeEventInformation.StateChangeReason.ResumeCachedState
                };
                _currentState = _cachedState;
                _cachedState = null;
                onStateChange.Invoke(_info);
            }

            /// <summary>
            /// Reinitializes the cached state.
            /// </summary>
            public void ReinitializeCachedState()
            {
                EnterState(_cachedState.name);
                _cachedState = null;
            }

            /// <summary>
            /// Attempts to enter into the first state of the state machine.
            /// </summary>
            public void EnterState()
            {
                _info = new StateChangeEventInformation()
                {
                    inboundState = _availableStates[0]?.name ?? "-Empty-",
                    outboundState = _currentState?.name ?? "-Empty-" ,
                    reason = StateChangeEventInformation.StateChangeReason.EnterState
                };
                _SetState(_availableStates[0]);
            }
            /// <summary>
            /// Attempts to enter into the provided state of the state machine.
            /// </summary>
            public void EnterState(string _pState)
            {
                State _target = _availableStates.Find(s => s.name == _pState);
                _info = new StateChangeEventInformation()
                {
                    inboundState = _target?.name ?? "-Empty-",
                    outboundState = _currentState?.name ?? "-Empty-",
                    reason = StateChangeEventInformation.StateChangeReason.EnterState
                };
                _SetState(_target);
            }

            /// <summary>
            /// Moves to the next available state.
            /// </summary>
            public void NextState()
            {
                int _index = _availableStates.FindIndex(s => s == _currentState);
                if (_index == -1) return;
                int _nextIndex = (_index + 1 < _availableStates.Count) ? _index + 1 : 0;
                _info = new StateChangeEventInformation()
                {
                    inboundState = _availableStates[_nextIndex]?.name ?? "-Empty-",
                    outboundState = _availableStates[_index]?.name ?? "-Empty-",
                    reason = StateChangeEventInformation.StateChangeReason.NextState
                };
                _SetState(_availableStates[_nextIndex]);
            }

            /// <summary>
            /// Moves to the previous available state.
            /// </summary>
            public void PreviousState()
            {
                int _index = _availableStates.FindIndex(s => s == _currentState);
                if (_index == -1) return;
                int _previousIndex = (_index - 1 >= 0) ? _index - 1 : _availableStates.Count - 1;
                _info = new StateChangeEventInformation()
                {
                    inboundState = _availableStates[_previousIndex]?.name ?? "-Empty-",
                    outboundState = _availableStates[_index]?.name ?? "-Empty-",
                    reason = StateChangeEventInformation.StateChangeReason.PreviousState
                };
                _SetState(_availableStates[_previousIndex]);
            }

            /// <summary>
            /// Targets a specific callback in a specific state and toggles its activity
            /// State Name>Callback Name>True or T
            /// </summary>
            public void SetStateCallbackIsActive(string pInfo)
            {
                if (pInfo.Split('>').Length < 2) return;
                State _target = _availableStates.Find(s => s.name == pInfo.Split('>')[0]);
                if (_target == null) return;
                State.Callback _callback = _target.callbacks.Find(c => c.name == pInfo.Split('>')[1]);
                if (_callback == null) return;
                _callback.active = (pInfo.Split('>').Length > 2 && 
                    (pInfo.Split('>')[2].ToUpper() == "T" || pInfo.Split('>')[2].ToUpper() == "TRUE"));
            }

            #endregion

            #region PRIVATE METHODS

            /// <summary>
            /// Adds this substate machine to the registry
            /// </summary>
            private void Awake()
            {
                name = name.Replace("(Clone)", "");
                allMachines.Add(this);
            }

            /// <summary>
            /// Entering into the first state listed in the available state list
            /// </summary>
            private void Start() => EnterState();

            /// <summary>
            /// Removes the substatemachine from the registry
            /// </summary>
            private void OnDestroy()
            {
                allMachines.Remove(this);
                if (!isExitInvokedOnDestroy) return;
                _currentState._Exit();
            }

            /// <summary>
            /// Invokes the current state's update callbacks.
            /// </summary>
            private void Update()
            {
                if (!conditions.HasFlag(State.CallbackCondition.Update)) return;
                _currentState?._Update();
            }

            /// <summary>
            /// Invokes the current state's fixed update callbacks.
            /// </summary>
            private void FixedUpdate()
            {
                if (!conditions.HasFlag(State.CallbackCondition.FixedUpdate)) return;
                _currentState?._FixedUpdate();
            }

            /// <summary>
            /// Sets the state if possible.
            /// </summary>
            private void _SetState(State _pNextState)
            {
                _nextState = _pNextState;
                if (_nextState == null) return;
                _ExitState();
                _currentState = _nextState;
                if (conditions.HasFlag(State.CallbackCondition.Enter)) _currentState._Enter();
                onStateChange.Invoke(_info);
            }

            /// <summary>
            /// Exits the current state.
            /// </summary>
            private void _ExitState(bool _pIsDestroyed = false)
            {
                if (_currentState == null) return;
                if (conditions.HasFlag(State.CallbackCondition.Exit)) _currentState._Exit();
            }

            #endregion

            #region PUBLIC STRUCTS

            public struct StateChangeEventInformation
            {
                public enum StateChangeReason { EnterState, NextState, PreviousState, ResumeCachedState };
                public StateChangeReason reason;
                public string outboundState;
                public string inboundState;
            }

            #endregion

        }
    }
}