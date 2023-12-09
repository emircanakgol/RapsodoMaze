using System;
using System.Collections;
using AI.Core.States;
using Gameplay;
using Unity.VisualScripting;
using UnityEngine;

namespace Controllers
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;
        
        public static event Action<int, int> OnHealthChangedCallback;
        public static event Action<string> OnGameOverCallback;
        public static event Action OnGameWonCallback;

        private int _maxHealth = 5;
        
        private int _health = 5;
        public int Health {
            get => _health;
            set {
                if(_health == value) return;
                _health = value;
                if(_health == 0) GameOver(false);
                OnHealthChangedCallback?.Invoke(_health, _maxHealth);
            }
        }

        public MovementController movementController;
        [SerializeField] private GameObject playerMeshGO;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(Instance);
                Instance = this;
            }
            else 
                Instance = this;
        }
        
        private void OnEnable() {
            ShootState.OnShootPlayerCallback += GameOver;
            MovementController.OnHitWallCallback += DamagePlayer;
        }

        private void OnDisable() {
            ShootState.OnShootPlayerCallback -= GameOver;
            MovementController.OnHitWallCallback -= DamagePlayer;
        }

        private void Start() {
            Instance ??= this;
        }

        public void GameWon() {
            OnGameWonCallback?.Invoke();
            StartCoroutine(StopGame());
        }

        public void GameOver(bool dueToSpheres) {
            if(dueToSpheres)
                OnGameOverCallback?.Invoke("The spheres got you!");
            else
                OnGameOverCallback?.Invoke("You've hit your head to the walls too much.");
            StartCoroutine(StopGame());
        }

        private IEnumerator StopGame() {
            yield return new WaitForSeconds(movementController.movementDuration + 0.1f);
            Destroy(playerMeshGO);
        }

        private void DamagePlayer() {
            Health--;
        }
    }
}
