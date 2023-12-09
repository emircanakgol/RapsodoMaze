using System;
using AI.Core.States;
using Controllers;
using Gameplay;
using UnityEngine;

namespace AI.Core
{
    public class AIStateMachine
    {
        public event Action<IAIState> OnStateChangedCallback; 
        
        public IAIState CurrentState { get; private set; }

        public WanderState wanderState;
        public ShootState shootState;
        public DetectedState detectedState;

        private Enemy _enemy;

        public AIStateMachine(Enemy enemy, MovementController movementController, MazeGenerator mazeGenerator) 
        {
            _enemy = enemy;
            wanderState = new WanderState(enemy, mazeGenerator);
            shootState = new ShootState(enemy, movementController, mazeGenerator);
            detectedState = new DetectedState(enemy, mazeGenerator);
        }

        public void Initialize(IAIState state) {
            CurrentState = state;
            state.Enter();
            OnStateChangedCallback?.Invoke(state);
        }

        public void TransitionTo(IAIState nextState) {
            CurrentState.Exit();
            nextState.Enter();
            CurrentState = nextState;

            OnStateChangedCallback?.Invoke(nextState);
        }

        public void Update() {
            CurrentState?.Update();
        }

        public Type CurrentStateType() {
            return CurrentState.GetType();
        }
    }
}