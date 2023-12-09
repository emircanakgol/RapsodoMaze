using System;
using Controllers;
using Core;
using UnityEngine;

namespace Visual
{
    public class CellRenderer : MonoBehaviour
    {
        private Cell _cell;

        [Header("Properties")] 
        public bool leftWall = true;
        public bool rightWall = true;
        public bool upperWall = true;
        public bool lowerWall = true;

        [Header("References")] 
        [SerializeField] private GameObject leftWallGO;
        [SerializeField] private GameObject rightWallGO;
        [SerializeField] private GameObject upperWallGO;
        [SerializeField] private GameObject lowerWallGO;

        public void SetCell(Cell cell) {
            _cell = cell;
            _cell.OnWallUpdateCallback += UpdateWalls;
        }

        private void OnEnable() {
            if(_cell != null)
                _cell.OnWallUpdateCallback += UpdateWalls;
        }

        private void OnDisable() {
            _cell.OnWallUpdateCallback -= UpdateWalls;
        }

        private void UpdateWalls(Side[] openSides) {
            foreach (var openSide in openSides) {
                switch (openSide) {
                    case Side.Left:  leftWall = false; break;
                    case Side.Right: rightWall = false; break;
                    case Side.Up:    upperWall = false; break;
                    case Side.Down:  lowerWall = false; break;
                }
            }
            UpdateGameObjects();
        }

        private void UpdateGameObjects() {
            leftWallGO.SetActive(leftWall);
            rightWallGO.SetActive(rightWall);
            upperWallGO.SetActive(upperWall);
            lowerWallGO.SetActive(lowerWall);
        }
    }
}
