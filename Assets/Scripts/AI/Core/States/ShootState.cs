using System;
using Controllers;
using DG.Tweening;
using Gameplay;
using UnityEngine;

namespace AI.Core.States
{
    public class ShootState : IAIState
    {
        public static event Action<bool> OnShootPlayerCallback;
        
        private Enemy _enemy;
        private MovementController _movementController;
        private MazeGenerator _mazeGenerator;

        private float _movementScale;
        private AIStateMachine _stateMachine;
        private IAIState _wanderState;
        private Tween _shootTween;
        
        public ShootState(Enemy enemy, MovementController movementController, MazeGenerator mazeGenerator) 
        {
            _enemy = enemy;
            _movementController = movementController;
            _mazeGenerator = mazeGenerator;
            _movementScale = _mazeGenerator.scale;
        }
        
        public void Enter() {
            _stateMachine = _enemy.stateMachine;
            _wanderState = _stateMachine.wanderState;
            _shootTween = _enemy.model.DOScale(0.7f * _movementScale, 0.1f)
                .OnComplete(()=> Shoot());
            _enemy.shootAudio.Play();
        }

        public void Update() {
            
        }

        public void Exit() {
            if(_shootTween.IsActive())
                _shootTween.Kill();
        }

        private void Shoot() {
            var shootingRange = CalculateShootingData(_enemy.position);
            if(shootingRange.Condition)
                OnShootPlayerCallback?.Invoke(true);
            ShootVFX(shootingRange);
            _stateMachine.TransitionTo(_wanderState);
        }

        private void ShootVFX(SideValues shootingRange) {
            if (shootingRange.Left != 0) {
                _enemy.leftVFX.SetFloat("Size", shootingRange.Left);
                _enemy.leftVFX.Play();
            }
            if (shootingRange.Right != 0) {
                _enemy.rightVFX.SetFloat("Size", shootingRange.Right);
                _enemy.rightVFX.Play();
            }
            if (shootingRange.Up != 0) {
                _enemy.upVFX.SetFloat("Size", shootingRange.Up);
                _enemy.upVFX.Play();
            }
            if (shootingRange.Down != 0) {
                _enemy.downVFX.SetFloat("Size", shootingRange.Down);
                _enemy.downVFX.Play();
            }
        }
        
        public SideValues CalculateShootingData(Vector2Int cellPos) {
            SideValues sideValues = new SideValues();
            int left = 0, right = 0, up = 0, down = 0;
            for (int l = cellPos.x - 1; l >= 0; l--) {
                var pos = new Vector2Int(l, cellPos.y);
                var cell = _mazeGenerator.cells[pos];
                if (cell.HasAWall(Side.Right))
                    break;
                if (_movementController.mazePosition == cell.position)
                    sideValues.Condition = true;
                left++;
            }
            for (int r = cellPos.x + 1; r < _mazeGenerator.Size; r++) {
                var pos = new Vector2Int(r, cellPos.y);
                var cell = _mazeGenerator.cells[pos];
                if (cell.HasAWall(Side.Left))
                    break;
                if (_movementController.mazePosition == cell.position)
                    sideValues.Condition = true;
                right++; 
            }
            for (int u = cellPos.y + 1; u < _mazeGenerator.Size; u++) {
                var pos = new Vector2Int(cellPos.x, u);
                var cell = _mazeGenerator.cells[pos];
                if (cell.HasAWall(Side.Down))
                    break;
                if (_movementController.mazePosition == cell.position)
                    sideValues.Condition = true;
                up++; 
            }
            for (int d = cellPos.y - 1; d >= 0; d--) {
                var pos = new Vector2Int(cellPos.x, d);
                var cell = _mazeGenerator.cells[pos];
                if (cell.HasAWall(Side.Up))
                    break;
                if (_movementController.mazePosition == cell.position)
                    sideValues.Condition = true;
                down++;
            }
            sideValues.Left = left; sideValues.Right = right;
            sideValues.Up = up; sideValues.Down = down;
            return sideValues;
        }
    }
}