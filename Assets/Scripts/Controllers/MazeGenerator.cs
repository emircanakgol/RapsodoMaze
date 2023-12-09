using System;
using System.Collections.Generic;
using AI;
using Core;
using UnityEngine;
using Visual;
using Random = UnityEngine.Random;

namespace Controllers
{
    public enum Side
    {
        _, Left, Right, Up, Down
    }

    public class MazeGenerator : MonoBehaviour
    {
        public static MazeGenerator Instance;

        public static event Action<MazeGenerator> OnMazeGeneratedCallback;
    
        [Header("Prefabs")] 
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject enemyPrefab;
    
        [field: Header("Settings")]
        [field: SerializeField] public int Size {get; private set;}
        public float scale;
        public Vector3 offset;
        public int enemyCount;
    
        public Vector2Int startPosition {get; private set;}
        public Vector2Int endPosition {get; private set;}
        public Dictionary<Vector2Int, Cell> cells;
        public List<Enemy> enemies;
    
        private Cell _startCell;
        private Cell _endCell;
        private List<Cell> _unvisitedCells;
    
    
        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(Instance);
                Instance = this;
            }
            else 
                Instance = this;
        
            cells = new();
            enemies = new();
            _unvisitedCells = new();
        }

        private void OnEnable() {
            Instance ??= this;
        }

        private void Start() {
            scale = 10f / Size;
            offset = new Vector3(-Size / 2f + 0.5f, 0, -Size / 2f + 0.5f) * scale;
            GenerateMaze();
            AldousBroderAlgorithm();
            RandomEnemies();
            transform.localScale = new Vector3(scale, 1, scale);
            transform.position = offset;
            OnMazeGeneratedCallback?.Invoke(this);
        }
    
        private void GenerateMaze() {
            // Using odd numbers in "columns" is fine.
            var startX = Size / 2;
            startPosition = new Vector2Int(startX, 0);
            endPosition = new Vector2Int(1, Size - 1);
        
            for (int i = 0; i < Size; i++) {
                for (int j = 0; j < Size; j++) {
                    var cellPosition = new Vector2Int(i, j);
                    var cell = new Cell(i, j, this);
                    cells.Add(cellPosition, cell);
                    _unvisitedCells.Add(cell);
                    var worldPosition = new Vector3(cellPosition.x, 0, cellPosition.y);
                    var cellGO = Instantiate(
                        cellPrefab,
                        worldPosition, 
                        Quaternion.identity, 
                        transform
                    );
                    var cellRenderer = cellGO.GetComponent<CellRenderer>();
                    cellRenderer.SetCell(cell);
                }
            }

            if (cells[startPosition] == null) {
                Debug.LogError("There was an error while generating the cells.");
                return;
            }
            _startCell = cells[startPosition];
            _endCell = cells[endPosition];
        }
    
        /// <summary>
        /// Source https://en.wikipedia.org/wiki/Maze_generation_algorithm#Aldous-Broder_algorithm
        /// </summary>
        private void AldousBroderAlgorithm() {
            _startCell.RemoveWall(Side.Down);
            _endCell.RemoveWall(Side.Up);
            Cell currentCell = _startCell;
            currentCell.isVisited = true;
        
            if (_unvisitedCells.Contains(currentCell)) 
                _unvisitedCells.Remove(currentCell);
        
            while (_unvisitedCells.Count > 0) {
                Cell randomNeighbor = currentCell.RandomNeighbor();
                if (randomNeighbor == null) {
                    Debug.LogError("There was an error while getting the neighbors of a cell.");
                    break;
                }
                if (!randomNeighbor.isVisited) 
                    WallRemover(currentCell, randomNeighbor);
            
                randomNeighbor.isVisited = true;
                if (_unvisitedCells.Contains(randomNeighbor)) 
                    _unvisitedCells.Remove(randomNeighbor);
                currentCell = randomNeighbor;
            }
        }

        private void RandomEnemies() {
            List<Vector2Int> spawnedPositions = new();
            for (int i = 0; i < enemyCount; i++) {
                var x = Random.Range(0, Size);
                var y = Random.Range(0, Size);
                Vector2Int randomPos = new Vector2Int(x, y);
                if(spawnedPositions.Contains(randomPos)) {
                    enemyCount++;
                    continue;
                }
                spawnedPositions.Add(randomPos);
                var pos = new Vector3(randomPos.x, 0, randomPos.y) * scale;
                var enemyGO = Instantiate(enemyPrefab, pos + offset, Quaternion.identity);
                var enemy = enemyGO.GetComponent<Enemy>();
                enemy.position = randomPos;
                enemies.Add(enemy);
            }
        }

        private void WallRemover(Cell first, Cell second) {
            Side firstCellWallSide = first.CheckWhichSide(second);
            Side secondCellWallSide = second.CheckWhichSide(first);
            first.RemoveWall(firstCellWallSide);
            second.RemoveWall(secondCellWallSide);
        }

        public bool CheckEnemy(Vector2Int position, out Enemy foundEnemy) {
            foreach (var enemy in enemies) {
                if (position == enemy.position) {
                    foundEnemy = enemy;
                    return true;
                }
            }
            foundEnemy = null;
            return false;
        }
    }
}