using System;
using Controllers;
using DG.Tweening;
using Gameplay;
using UnityEngine;

namespace AI.Core.States
{
    public class WanderState : IAIState
    {
        public event Action OnEnemyMovedCallback;
        
        private Enemy _enemy;
        private MovementController _movementController;
        private MazeGenerator _mazeGenerator;

        private float _passedTime;
        private float _movementScale;
        
        public WanderState(Enemy enemy, MazeGenerator mazeGenerator) {
            _enemy = enemy;
            _mazeGenerator = mazeGenerator;
            _movementScale = mazeGenerator.scale;
        }
        
        public void Enter() {
            _enemy.model.transform.localScale = 0.7f * _movementScale * Vector3.one;
        }

        public void Update() {
            if (_passedTime >= 1) {
                if(_mazeGenerator.cells.ContainsKey(_enemy.position)) {
                    var cell = _mazeGenerator.cells[_enemy.position];
                    var randomCell = cell.GetRandomAvailableCell();
                    Move(randomCell.position);
                }
                _passedTime = 0;
            }
            _passedTime += Time.deltaTime;
        }

        public void Exit() {
            
        }

        private void Move(Vector2Int destination) {
            _enemy.position = destination;
            Vector3 dest = new Vector3(destination.x, 0, destination.y) * _movementScale;
            _enemy.worldPosition.position = dest + _mazeGenerator.offset;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(
                _enemy.model.DOScale(0.5f * _movementScale, 0.3f));
            sequence.Append(
                _enemy.model.DOMove(_enemy.worldPosition.position, 0.4f));
            sequence.Join(
                _enemy.model.DOScale(0.7f * _movementScale, 0.4f));
            OnEnemyMovedCallback?.Invoke();
        }
    }
}