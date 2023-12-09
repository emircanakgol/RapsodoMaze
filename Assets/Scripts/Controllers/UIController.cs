using System;
using System.Collections;
using DG.Tweening;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Controllers
{
    public class UIController : MonoBehaviour
    {
        [Header("References")]
        [Header("Health Bar")]
        [SerializeField] private RectTransform healthValue;
        [SerializeField] private TextMeshProUGUI healthTMP;
        [Header("Game Over")] 
        [SerializeField] private GameObject gameOverGO;
        [SerializeField] private CanvasGroup gameOverCG;
        [SerializeField] private TextMeshProUGUI reasonTMP;
        [Header("Game Won")] 
        [SerializeField] private GameObject gameWonGO;
        [SerializeField] private CanvasGroup gameWonCG;
        [Header("Other")] 
        [SerializeField] private GameObject infoGO;
        [SerializeField] private GameObject infoKeyGO;

        private Coroutine _warning;

        private void OnEnable() {
            GameController.OnHealthChangedCallback += UpdateHealthUI;
            GameController.OnHealthChangedCallback += ShowWarning;
            GameController.OnGameOverCallback += GameOverUI;
            GameController.OnGameWonCallback += GameWonUI;
            MovementController.OnPlayerFirstMovedCallback += HideWarning;
        }

        private void OnDisable() {
            GameController.OnHealthChangedCallback -= UpdateHealthUI;
            GameController.OnHealthChangedCallback -= ShowWarning;
            GameController.OnGameOverCallback -= GameOverUI;
            GameController.OnGameWonCallback -= GameWonUI;
            MovementController.OnPlayerFirstMovedCallback -= HideWarning;
        }

        private void UpdateHealthUI(int health, int maxHealth) {
            float greenRedRatio = (float)health / maxHealth;
            healthTMP.text = health.ToString();
            var endSize = new Vector2(120f * health / maxHealth, 56);
            healthValue.DOSizeDelta(endSize, 0.4f);
            var img = healthValue.GetComponent<Image>();
            var color = new Color(1 - greenRedRatio, greenRedRatio, 0, 1);
            img.DOColor(color, 0.4f);
        }

        private void Start() {
            GameStartUI();
        }

        private void GameOverUI(string reason) {
            gameOverGO.SetActive(true);
            gameOverCG.DOFade(1, 1);
            reasonTMP.text = "Reason: " + reason;
        }
        
        private void GameWonUI() {
            gameWonGO.SetActive(true);
            gameWonCG.DOFade(1, 1);
        }
        
        private void GameStartUI() {
            gameOverGO.SetActive(false);
            gameOverCG.alpha = 0;
            gameWonGO.SetActive(false);
            gameWonCG.alpha = 0;
        }

        public void RestartGame() {
            GameStartUI();
            SceneManager.LoadScene("empty", LoadSceneMode.Single);
        }

        private void ShowWarning(int _, int __) {
            if(_warning != null) StopCoroutine(_warning);
            _warning = StartCoroutine(ShowWarningForSeconds(3));
        }

        private IEnumerator ShowWarningForSeconds(float seconds) {
            infoGO.SetActive(true);
            yield return new WaitForSeconds(seconds);
            HideWarning();
        }

        private void HideWarning() {
            infoGO.SetActive(false);
            infoKeyGO.SetActive(false);
        }
    }
}
