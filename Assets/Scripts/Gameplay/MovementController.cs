using System;
using System.Linq;
using Cinemachine;
using Controllers;
using Core;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    [RequireComponent(typeof(ShootingController))]
    public class MovementController : MonoBehaviour
    {
        public static event Action OnPlayerFirstMovedCallback;
        public static event Action OnPlayerMovedCallback;
        public static event Action OnHitWallCallback;
        
        [Header("References")]
        [SerializeField] private Transform model;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private AudioSource shootAudio;
        [SerializeField] private AudioSource moveAudio;
        [SerializeField] private AudioSource hitWallAudio;
        
        [Header("Settings")]
        public float movementDuration;

        public Cell cell;
        public Vector2Int mazePosition;
        
        private MazeGenerator _mazeGenerator;
        private ShootingController _shootingController;
        private CinemachineBasicMultiChannelPerlin _noise;

        private bool _firstMoved;
        private bool _isMoving;
        private float _movementScale;
        private Tween _moveTween;
        private Tween _chargeTween;
        private Vector2Int _finish;
        
        private void Awake() {
            _noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _shootingController = GetComponent<ShootingController>();
        }
        
        private void Update() {
            CheckForInput();
        }

        private void OnEnable() {
            _mazeGenerator = MazeGenerator.Instance;
            MazeGenerator.OnMazeGeneratedCallback += Init;
            OnHitWallCallback += ShakeCamera;
        }

        private void OnDisable() {
            MazeGenerator.OnMazeGeneratedCallback -= Init;
            OnHitWallCallback -= ShakeCamera;
        }

        private void Init(MazeGenerator mazeGenerator) {
            _mazeGenerator = mazeGenerator;
            _movementScale = _mazeGenerator.scale;
            _finish = _mazeGenerator.endPosition + Vector2Int.up;
            var middlePoint = - _mazeGenerator.Size / 2;
            mazePosition = new Vector2Int(- middlePoint, -1);
            transform.position = new Vector3(0, 0,  _movementScale * (middlePoint - 1));
            model.localScale = new Vector3(0.5f, 0.5f, 0.5f) * _movementScale;
        }

        private void CheckForInput() {
            if (Input.GetKeyDown(KeyCode.W)) Move(_movementScale * Vector3.forward, Side.Up);
            if(!_firstMoved) return;
            if (Input.GetKeyDown(KeyCode.A)) Move(_movementScale * Vector3.left, Side.Left);
            if (Input.GetKeyDown(KeyCode.S)) Move(_movementScale * Vector3.back, Side.Down);
            if (Input.GetKeyDown(KeyCode.D)) Move(_movementScale * Vector3.right, Side.Right);
            if (Input.GetKeyDown(KeyCode.Space)) Charge();
            if (Input.GetKeyUp(KeyCode.Space)) Shoot();
        }

        private bool CheckWall(Side direction) {
            var walls = cell.GetWalls();
            if (walls.Contains(direction)) return true;
            return false;
        }
        
        private void Move(Vector3 movement, Side direction) {
            _isMoving = true;
            if (!_firstMoved) {
                _firstMoved = true;
                OnPlayerFirstMovedCallback?.Invoke();
            }
            if (_mazeGenerator.cells.TryGetValue(mazePosition, out var c)) 
                cell = c;
            
            if (cell != null && CheckWall(direction)) {
                OnHitWallCallback?.Invoke();
                HitWallMoveTween(direction);
                HitWallScaleTween(direction);
                hitWallAudio.Play();
                return;
            }
            cell = null;
            
            Vector2Int deltaDirection = Vector2Int.zero;
            switch (direction) {
                case Side.Left: deltaDirection = new Vector2Int(-1, 0); break;
                case Side.Right: deltaDirection = new Vector2Int(1, 0); break;
                case Side.Up: deltaDirection = new Vector2Int(0, 1); break;
                case Side.Down: deltaDirection = new Vector2Int(0, -1); break;
            }
            mazePosition += deltaDirection;
            transform.position += movement;
            
            if(mazePosition == _finish)
                GameController.Instance.GameWon();
            
            if (_mazeGenerator.CheckEnemy(mazePosition, out var enemy)) 
                GameController.Instance.GameOver(true);
            
            MoveTween();
            ScaleTween(direction);
            
            OnPlayerMovedCallback?.Invoke();
            moveAudio.Play();
        }

        private void Charge() {
            if(_isMoving) return;
            _chargeTween = model.DOScale(0.3f * _movementScale, movementDuration * 2 / 3)
                .SetEase(Ease.InOutBack);
        }
        
        private void Shoot() {
            if(_isMoving) return;
            if(model==null) return;
            shootAudio.Play();
            if(_chargeTween.IsActive()) _chargeTween.Kill();
            _chargeTween = model.DOScale(0.5f * _movementScale, movementDuration / 3);
            
            if (_mazeGenerator.cells.TryGetValue(mazePosition, out var c)) {
                _shootingController.Shoot(c.position);
            }
        }

        private void MoveTween() {
            if(_moveTween.IsActive()) {
                _moveTween.Complete();
                _isMoving = true;
            }
            _moveTween = model.DOLocalMove(transform.position, movementDuration)
                            .SetEase(Ease.OutCubic).OnComplete(()=>_isMoving = false);
        }
        
        private void ScaleTween(Side direction) {
            Sequence sequence = DOTween.Sequence();
            if (direction == Side.Up || direction == Side.Down) {
                sequence.Append(
                    model.DOScaleX(0.25f * _movementScale, movementDuration / 2));
                sequence.Join(
                    model.DOScaleZ(1f * _movementScale, movementDuration / 2));
                sequence.Append(
                    model.DOScaleX(0.5f * _movementScale, movementDuration / 2));
                sequence.Join(
                    model.DOScaleZ(0.5f * _movementScale, movementDuration / 2));
            }
            else {
                sequence.Append(
                    model.DOScaleZ(0.25f * _movementScale, movementDuration / 2));
                sequence.Join(
                    model.DOScaleX(1f * _movementScale, movementDuration / 2));
                sequence.Append(
                    model.DOScaleZ(0.5f * _movementScale, movementDuration / 2));
                sequence.Join(
                    model.DOScaleX(0.5f * _movementScale, movementDuration / 2));
            }
        }
        
        private void HitWallMoveTween(Side direction) {
            float movement;
            Vector3 pos = model.position;
            if (direction == Side.Left || direction == Side.Down)
                movement = -0.4f * _movementScale;
            else
                movement = 0.4f * _movementScale;

            Sequence sequence = DOTween.Sequence().OnComplete(()=>_isMoving = false);
            if (direction == Side.Left || direction == Side.Right) {
                sequence.Append(
                    model.DOMoveX(pos.x + movement, movementDuration / 2));
                sequence.Append(
                    model.DOMoveX(pos.x, movementDuration / 2));
            }
            else {
                sequence.Append(
                    model.DOMoveZ(pos.z + movement, movementDuration / 2));
                sequence.Append(
                    model.DOMoveZ(pos.z, movementDuration / 2));
            }
        }

        private void HitWallScaleTween(Side direction) {
            Sequence sequence = DOTween.Sequence();
            if (direction == Side.Left || direction == Side.Right) {
                sequence.Append(
                    model.DOScaleZ(1 * _movementScale, movementDuration / 2));
                sequence.Join(
                    model.DOScaleX(0.25f * _movementScale, movementDuration / 2));
                sequence.Append(
                    model.DOScaleZ(0.5f * _movementScale, movementDuration / 2));
                sequence.Join(
                    model.DOScaleX(0.5f * _movementScale, movementDuration / 2));
            }
            else {
                sequence.Append(
                    model.DOScaleX(1 * _movementScale, movementDuration / 2));
                sequence.Join(
                    model.DOScaleZ(0.25f * _movementScale, movementDuration / 2));
                sequence.Append(
                    model.DOScaleX(0.5f * _movementScale, movementDuration / 2));
                sequence.Join(
                    model.DOScaleZ(0.5f * _movementScale, movementDuration / 2));
            }
        }

        private void ShakeCamera() {
            DOVirtual.Float(_noise.m_AmplitudeGain, 
                2, 
                movementDuration * 2 / 3, 
                (x)=>_noise.m_AmplitudeGain=x)
                    .OnComplete(()=>
                        DOVirtual.Float(2, 
                        0, 
                        movementDuration / 3, 
                        (x)=>_noise.m_AmplitudeGain=x)
                    );
        }
    }
}
