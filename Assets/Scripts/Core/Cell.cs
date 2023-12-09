using System;
using System.Collections.Generic;
using Controllers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public class Cell
    {
        public event Action<Side[]> OnWallUpdateCallback;
        
        public Vector2Int position { get; private set; }
        public bool isVisited;
    
        private MazeGenerator _mazeGenerator;
        private List<Side> _openSides;
    
        public Cell(int x, int y, MazeGenerator generator) {
            position = new Vector2Int(x, y);
            _mazeGenerator = generator;
            _openSides = new();
        }

        public Side[] AvailableNeighbors() {
            List<Side> neighbors = new List<Side>();
        
            if(GetLeftCell() != null) neighbors.Add(Side.Left);
            if(GetRightCell() != null) neighbors.Add(Side.Right);
            if(GetUpperCell() != null) neighbors.Add(Side.Up);
            if(GetLowerCell() != null) neighbors.Add(Side.Down);

            if (neighbors.Count == 0) return null;
            return neighbors.ToArray();
        }

        public Cell RandomNeighbor() {
            var neighbors = AvailableNeighbors();
            int randomIndex = Random.Range(0, neighbors.Length); 
            return GetCell(neighbors[randomIndex]);
        }

        public void RemoveWall(Side side) {
            if(_openSides.Contains(side)) return;
            _openSides.Add(side);
            Side[] openSidesArray = _openSides.ToArray();
            OnWallUpdateCallback?.Invoke(openSidesArray);
        }

        public Side CheckWhichSide(Cell refCell) {
            if (refCell == GetLeftCell()) return Side.Left;
            if (refCell == GetRightCell()) return Side.Right;
            if (refCell == GetUpperCell()) return Side.Up;
            if (refCell == GetLowerCell()) return Side.Down;
            Debug.LogError("Two compared cells are not neighbors.");
            return Side._;
        }

        public Side[] GetWalls() {
            List<Side> walls = new();
            if(!_openSides.Contains(Side.Left)) walls.Add(Side.Left);
            if(!_openSides.Contains(Side.Right)) walls.Add(Side.Right);
            if(!_openSides.Contains(Side.Up)) walls.Add(Side.Up);
            if(!_openSides.Contains(Side.Down)) walls.Add(Side.Down);

            return walls.ToArray();
        }

        public bool HasAWall(Side side) {
            return !_openSides.Contains(side);
        }

        public Cell GetRandomAvailableCell() {
            int randomIndex = Random.Range(0, _openSides.Count);
            var cell = GetCell(_openSides[randomIndex]);
            if (cell == null) return this;
            foreach (var enemy in _mazeGenerator.enemies) {
                if (enemy.position == cell.position)
                    return this;
            }
            if (cell.position == _mazeGenerator.startPosition) 
                return GetModifiedStartCell();
            if (cell.position == _mazeGenerator.endPosition)
                return GetModifiedEndCell();
            return cell;
        }

        public Cell GetCell(Side side) {
            switch (side) {
                case Side.Left:  return GetLeftCell();
                case Side.Right: return GetRightCell();
                case Side.Up:    return GetUpperCell();
                case Side.Down:  return GetLowerCell();
                default:         return null;
            }
        }
    
        private Cell GetLeftCell() {
            Vector2Int pos = new Vector2Int(position.x - 1, position.y);
            return _mazeGenerator.cells.GetValueOrDefault(pos);
        }
    
        private Cell GetRightCell() {
            Vector2Int pos = new Vector2Int(position.x + 1, position.y);
            return _mazeGenerator.cells.GetValueOrDefault(pos);
        }
    
        private Cell GetUpperCell() {
            Vector2Int pos = new Vector2Int(position.x, position.y + 1);
            return _mazeGenerator.cells.GetValueOrDefault(pos);
        }
    
        private Cell GetLowerCell() {
            Vector2Int pos = new Vector2Int(position.x, position.y - 1);
            return _mazeGenerator.cells.GetValueOrDefault(pos);
        }

        private static Cell GetModifiedStartCell() {
            var mazeGenerator = MazeGenerator.Instance;
            var startCell = mazeGenerator.cells[mazeGenerator.startPosition];
            var cell = new Cell(mazeGenerator.startPosition.x, mazeGenerator.startPosition.y, mazeGenerator);
            foreach (var openSide in startCell._openSides) 
                if(openSide!=Side.Down) cell._openSides.Add(openSide);

            return cell;
        }
        
        private static Cell GetModifiedEndCell() {
            var mazeGenerator = MazeGenerator.Instance;
            var endCell = mazeGenerator.cells[mazeGenerator.endPosition];
            var cell = new Cell(mazeGenerator.endPosition.x, mazeGenerator.endPosition.y, mazeGenerator);
            foreach (var openSide in endCell._openSides) 
                if(openSide!=Side.Up) cell._openSides.Add(openSide);
            
            return cell;
        }
    }
}
