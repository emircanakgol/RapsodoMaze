using System;
using System.Collections.Generic;
using AI;
using Controllers;
using Core;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

namespace Gameplay
{
    public struct SideValues
    {
        public int Left;
        public int Right;
        public int Up;
        public int Down;
        public bool Condition;
        public List<Enemy> Enemies;

        public int NonZeroCount() {
            int count = 0;
            if (Left != 0) count++;
            if (Right != 0) count++;
            if (Up != 0) count++;
            if (Down != 0) count++;
            return count;
        }
    }
    
    [RequireComponent(typeof(MovementController))]
    public class ShootingController : MonoBehaviour
    {
        public static event Action<Enemy> OnShootEnemyCallback;
        
        [Header("References")]
        [SerializeField] private VisualEffect leftVFX;
        [SerializeField] private VisualEffect rightVFX;
        [SerializeField] private VisualEffect upVFX;
        [SerializeField] private VisualEffect downVFX;
        
        private MazeGenerator _mazeGenerator;

        private float _beamScale;

        private void OnEnable() {
            MazeGenerator.OnMazeGeneratedCallback += Init;
        }
        
        private void OnDisable() {
            MazeGenerator.OnMazeGeneratedCallback -= Init;
        }

        private void Init(MazeGenerator mazeGenerator) {
            _mazeGenerator = mazeGenerator;
            _beamScale = _mazeGenerator.scale;
        }

        public void Shoot(Vector2Int position) {
            if(position.x < 0 ||
               position.x >= _mazeGenerator.Size ||
               position.y < 0 ||
               position.y >= _mazeGenerator.Size)
                return;
            var shootingData = CalculateShootingData(position);
            if (shootingData.Condition) {
                for (int i = 0; i < shootingData.Enemies.Count; i++) {
                    OnShootEnemyCallback?.Invoke(shootingData.Enemies[i]);
                }
            }

            if (shootingData.Left != 0) {
                leftVFX.SetFloat("Size", shootingData.Left);
                leftVFX.Play();
            }
            if (shootingData.Right != 0) {
                rightVFX.SetFloat("Size", shootingData.Right);
                rightVFX.Play();
            }
            if (shootingData.Up != 0) {
                upVFX.SetFloat("Size", shootingData.Up);
                upVFX.Play();
            }
            if (shootingData.Down != 0) {
                downVFX.SetFloat("Size", shootingData.Down);
                downVFX.Play();
            }
        }

        private SideValues CalculateShootingData(Vector2Int cellPos) {
            SideValues sideValues = new SideValues();
            sideValues.Enemies = new();
            int left = 0, right = 0, up = 0, down = 0;
            for (int l = cellPos.x - 1; l >= 0; l--) {
                var pos = new Vector2Int(l, cellPos.y);
                var cell = _mazeGenerator.cells[pos];
                if (cell.HasAWall(Side.Right))
                    break;
                if (_mazeGenerator.CheckEnemy(cell.position, out var enemy)) {
                    sideValues.Condition = true;
                    sideValues.Enemies.Add(enemy);
                }
                left++;
            }
            for (int r = cellPos.x + 1; r < _mazeGenerator.Size; r++) {
                var pos = new Vector2Int(r, cellPos.y);
                var cell = _mazeGenerator.cells[pos];
                if (cell.HasAWall(Side.Left))
                    break;
                if (_mazeGenerator.CheckEnemy(cell.position, out var enemy)) {
                    sideValues.Condition = true;
                    sideValues.Enemies.Add(enemy);
                }
                right++; 
            }
            for (int u = cellPos.y + 1; u < _mazeGenerator.Size; u++) {
                var pos = new Vector2Int(cellPos.x, u);
                var cell = _mazeGenerator.cells[pos];
                if (cell.HasAWall(Side.Down))
                    break;
                if (_mazeGenerator.CheckEnemy(cell.position, out var enemy)) {
                    sideValues.Condition = true;
                    sideValues.Enemies.Add(enemy);
                }
                up++; 
            }
            for (int d = cellPos.y - 1; d >= 0; d--) {
                var pos = new Vector2Int(cellPos.x, d);
                var cell = _mazeGenerator.cells[pos];
                if (cell.HasAWall(Side.Up))
                    break;
                if (_mazeGenerator.CheckEnemy(cell.position, out var enemy)) {
                    sideValues.Condition = true;
                    sideValues.Enemies.Add(enemy);
                }
                down++;
            }
            sideValues.Left = left; sideValues.Right = right;
            sideValues.Up = up; sideValues.Down = down;
            return sideValues;
        }
    }
}
