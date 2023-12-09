using System;
using System.Collections;
using AI.Core;
using AI.Core.States;
using Controllers;
using DG.Tweening;
using Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace AI
{
    public class Enemy : MonoBehaviour
    {
        [Header("References")]
        public Transform worldPosition;
        public Transform model;
        public AIStateMachine stateMachine;
        public VisualEffect leftVFX;
        public VisualEffect rightVFX;
        public VisualEffect upVFX;
        public VisualEffect downVFX;
        public AudioSource chargeAudio;
        public AudioSource shootAudio;
        public AudioSource deathAudio;
        
        public Vector2Int position;
        
        private MazeGenerator _mazeGenerator;
        private GameController _gameController;
        private MovementController _movementController;

        private float _movementScale;
        
        private void OnEnable() {
            MazeGenerator.OnMazeGeneratedCallback += Init;
            MovementController.OnPlayerMovedCallback += UpdateStates;
            ShootingController.OnShootEnemyCallback += CheckShotEnemy;
            if(stateMachine!=null)
                stateMachine.wanderState.OnEnemyMovedCallback += UpdateStates;
        }

        private void OnDisable() {
            MazeGenerator.OnMazeGeneratedCallback -= Init;
            MovementController.OnPlayerMovedCallback -= UpdateStates;
            ShootingController.OnShootEnemyCallback -= CheckShotEnemy;
            stateMachine.wanderState.OnEnemyMovedCallback -= UpdateStates;
        }

        private void Init(MazeGenerator mazeGenerator) {
            _gameController = GameController.Instance;
            _movementController = _gameController.movementController;
            _mazeGenerator = mazeGenerator;
            _movementScale = _mazeGenerator.scale;
            stateMachine = new AIStateMachine(this, _gameController.movementController, _mazeGenerator);
            stateMachine.wanderState.OnEnemyMovedCallback += UpdateStates;
            stateMachine.Initialize(stateMachine.wanderState);
        }

        private void UpdateStates() {
            if(_movementController == null) {
                if(stateMachine.CurrentState == stateMachine.detectedState)
                    stateMachine.TransitionTo(stateMachine.wanderState);
                return;
            }
            if (CheckForPlayer(_movementController.mazePosition) && stateMachine.CurrentState != stateMachine.detectedState) 
                stateMachine.TransitionTo(stateMachine.detectedState);
            else if(stateMachine.CurrentState == stateMachine.detectedState)
                stateMachine.TransitionTo(stateMachine.wanderState);
        }

        private void Update() {
            stateMachine.Update();
        }

        public bool CheckForPlayer(Vector2Int playerPos) {
            for (int l = position.x - 1; l >= 0; l--) {
                var pos = new Vector2Int(l, position.y);
                if (_mazeGenerator.cells[pos].HasAWall(Side.Right))
                    break;
                if (pos == playerPos)
                    return true;
            }
            for (int r = position.x + 1; r < _mazeGenerator.Size; r++) {
                var pos = new Vector2Int(r, position.y);
                if (_mazeGenerator.cells[pos].HasAWall(Side.Left))
                    break;
                if (pos == playerPos)
                    return true;
            }
            for (int u = position.y + 1; u < _mazeGenerator.Size; u++) {
                var pos = new Vector2Int(position.x, u);
                if (_mazeGenerator.cells[pos].HasAWall(Side.Down))
                    break;
                if (pos == playerPos)
                    return true;
            }
            for (int d = position.y - 1; d >= 0; d--) {
                var pos = new Vector2Int(position.x, d);
                if (_mazeGenerator.cells[pos].HasAWall(Side.Up))
                    break;
                if (pos == playerPos)
                    return true;
            }
            return false;
        }

        private void CheckShotEnemy(Enemy shotEnemy) {
            if (shotEnemy == this) {
                _mazeGenerator.enemies.Remove(shotEnemy);
                stateMachine.TransitionTo(stateMachine.wanderState);
                model.DOScale(1.2f * _movementScale, 0.11f)
                    .OnComplete(()=>Destroy(gameObject));
                deathAudio.PlayDelayed(shootAudio.time);
            }
        }
    }
}
