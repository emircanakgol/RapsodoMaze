using Controllers;
using DG.Tweening;
using Gameplay;
using UnityEngine;

namespace AI.Core.States
{
    public class DetectedState : IAIState
    {
        private Enemy _enemy;
        private MovementController _movementController;
        private MazeGenerator _mazeGenerator;
        private float _movementScale;
        private AIStateMachine _stateMachine;
        private Tween _shootTween;        
        
        public DetectedState(Enemy enemy, MazeGenerator mazeGenerator) {
            _enemy = enemy;
            _mazeGenerator = mazeGenerator;
            _movementScale = mazeGenerator.scale;
        }
        
        public void Enter() {
            _stateMachine = _enemy.stateMachine;
            _shootTween = _enemy.model.DOScale(1 * _movementScale, 2f)
                .OnComplete(() => _stateMachine.TransitionTo(_stateMachine.shootState));
            _enemy.chargeAudio.Play();
        }

        public void Update() {
            
        }

        public void Exit() {
            _shootTween.Kill();
            if (_enemy.chargeAudio.isPlaying) _enemy.chargeAudio.Stop();
        }
    }
}