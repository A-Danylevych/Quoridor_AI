using System.Collections.Generic;
using System.Linq;

namespace Quoridor_AI.Model
{
    internal class Board
    {
        private List<Cell> Cells {get; set;}
        private const int CellsCount = 81;
        private const int SideWidth = 9;
        public Board()
        {
            NewBoard();
        }
        public void NewBoard()
        {
            InitializeCells();
        }
        public void MovePlayer(Player player, CellCoords coords)
        {
            var cell = FindCellByCoords(coords);
            MovePlayer(player, cell);
        }

        private Cell FindCellByCoords(CellCoords coords)
        {
            return Cells.FirstOrDefault(cell => cell.Coords.Top == coords.Top && cell.Coords.Left == coords.Left);
        }
        private void InitializeCells()
        {
            Cells = new List<Cell>();
            const int leftStart = 25;
            const int topStart = 25;
            var top = topStart;
            var left = leftStart;
            const int offset = 75; 
            for(var i = 0; i < SideWidth; i++)
            {
                for(var j = 0; j<SideWidth; j++)
                {
                    var coords = new CellCoords(top, left);
                    Cells.Add(new Cell(coords));
                    left += offset;
                }
                top += offset;
                left = leftStart;
            }

            for(var i = 0; i < SideWidth; i++)
            {
                for(var j = 0; j < SideWidth; j++)
                {
                    if(i != SideWidth-1)
                    {
                       Cells[i*SideWidth + j].BottomConnect(Cells[(i+1)*SideWidth + j]);
                    }
                    if(j != SideWidth-1)
                    {
                        Cells[i*SideWidth + j].RightConnect(Cells[i*SideWidth + j+1]);
                    }
                    if(j != 0)
                    {
                        Cells[i*SideWidth + j].LeftConnect(Cells[i*SideWidth + j-1]);
                    }
                    if(i != 0)
                    {
                        Cells[i*SideWidth + j].UpperConnect(Cells[(i-1)*SideWidth + j]);
                    }
                }
            }
        }
        public Cell TopStartPosition()
        {
            const int upperIndex = SideWidth/2;
            return Cells[upperIndex];
        }
        public Cell BottomStartPosition()
        {
            const int downIndex = CellsCount - SideWidth/2 - 1;
            return Cells[downIndex];
        }
        public List<Cell> TopWinningCells()
        {
            var topWinningCells = new List<Cell>();
            for(var i = CellsCount - 1; i > CellsCount - 1 - SideWidth; i--)
            {
                topWinningCells.Add(Cells[i]);
            }
            return topWinningCells;
        }
        public List<Cell> BottomWinningCells()
        {
            var bottomWinningCells = new List<Cell>();
            for(var i = 0; i< SideWidth; i++)
            {
                bottomWinningCells.Add(Cells[i]);
            }
            return bottomWinningCells;
        }

        internal void MovePlayer(Player player, Cell cell)
        {
            var cellToMove = FindCellByCoords(cell.Coords);
            player.ChangeCell(cellToMove);
        }

        internal void PutWall(Wall wall)
        {
            if (wall.IsVertical)
            {
                PutVerticalWall(wall);
            }
            else
            {
                PutHorizontalWall(wall);
            }
        }

        public bool CanBePlaced(Wall wall)
        {
            if (wall.IsVertical)
            {
                return CanBePlacedVertical(wall);
            }
            else
            {
                return CanBePlacedHorizontal(wall);
            }
        }

        private bool CanBePlacedVertical(Wall wall)
        {
            var cellList = FindVerticalWallNeighbours(wall);
            foreach (var cell in cellList)
            {
                if (cell == null)
                {
                    return false;
                }
            }
            var leftUpperCell = cellList[0];
            var leftBottomCell = cellList[1];
            var rightUpperCell = cellList[2];
            var rightBottomCell = cellList[3];

            if (leftUpperCell.DownCell is Wall && rightUpperCell.DownCell is Wall)
            {
                if (leftUpperCell.DownCell == rightBottomCell.DownCell)
                {
                    return false;
                }
            }

            if (leftBottomCell.UpCell is Wall && rightBottomCell.UpCell is Wall)
            {
                if (leftBottomCell.UpCell == rightBottomCell.UpCell)
                {
                    return false;
                }
            }

            if (leftBottomCell.RightCell is Wall || leftUpperCell.RightCell is Wall)
            {
                return false;
            }

            if (rightBottomCell.LeftCell is Wall || rightUpperCell.LeftCell is Wall)
            {
                return false;
            }

            return true;
        }

        private bool CanBePlacedHorizontal(Wall wall)
        {
            var cellList = FindHorizontalWallNeighbours(wall);
            if (cellList.Any(cell => cell == null))
            {
                return false;
            }
            var upperRightCell = cellList[0];
            var upperLeftCell = cellList[1];
            var bottomRightCell = cellList[2];
            var bottomLeftCell = cellList[3];

            if (upperLeftCell.DownCell is Wall || upperRightCell.DownCell is Wall)
            {
                return false;
            }

            if (bottomLeftCell.UpCell is Wall || bottomRightCell.UpCell is Wall)
            {
                return false;
            }

            if(bottomLeftCell.RightCell is Wall && upperLeftCell.RightCell is Wall)
            {
                if (bottomLeftCell.RightCell == upperLeftCell.RightCell)
                {
                    return false;
                }
            }

            if (bottomRightCell.LeftCell is Wall && upperRightCell.LeftCell is Wall)
            {
                if (bottomRightCell.LeftCell == upperRightCell.LeftCell)
                {
                    return false;
                }
            }

            return true;
        }
        private void PutVerticalWall(Wall wall)
        {
            var cellList = FindVerticalWallNeighbours(wall);
            var leftUpperCell = cellList[0];
            var leftBottomCell = cellList[1];
            var rightUpperCell = cellList[2];
            var rightBottomCell = cellList[3];
            leftUpperCell.RightConnect(wall);
            leftBottomCell.RightConnect(wall);
            rightUpperCell.LeftConnect(wall);
            rightBottomCell.LeftConnect(wall);
            var leftList = new List<IPlaceable>()
            {
                leftUpperCell,
                leftBottomCell
            };
            var rightList = new List<IPlaceable>()
            {
                rightUpperCell,
                rightBottomCell
            };
            wall.LeftConnect(leftList);
            wall.RightConnect(rightList);
        }

        private void PutHorizontalWall(Wall wall)
        {
            var cellList = FindHorizontalWallNeighbours(wall);
            var upperRightCell = cellList[0];
            var upperLeftCell = cellList[1];
            var bottomRightCell = cellList[2];
            var bottomLeftCell = cellList[3];
            upperRightCell.BottomConnect(wall);
            upperLeftCell.BottomConnect(wall);
            bottomRightCell.UpperConnect(wall);
            bottomLeftCell.UpperConnect(wall);
            var upperList = new List<IPlaceable>()
            {
                upperRightCell,
                upperLeftCell
            };
            var bottomList = new List<IPlaceable>()
            {
                bottomRightCell,
                bottomLeftCell
            };
            wall.UpperConnect(upperList);
            wall.BottomConnect(bottomList);
        }

        internal void DropWall(Wall wall){
            if (wall.IsVertical)
            {
                DropVerticalWall(wall);
            }
            else
            {
                DropHorizontalWall(wall);
            }
        }

        private void DropVerticalWall(Wall wall)
        {
            var cellList = FindVerticalWallNeighbours(wall);
            var leftUpperCell = cellList[0];
            var leftBottomCell = cellList[1];
            var rightUpperCell = cellList[2];
            var rightBottomCell = cellList[3];
            leftUpperCell.RightConnect(rightUpperCell);
            leftBottomCell.RightConnect(rightBottomCell);
            rightUpperCell.LeftConnect(leftUpperCell);
            rightBottomCell.LeftConnect(leftBottomCell);
        }

        private void DropHorizontalWall(Wall wall)
        {
            var cellList = FindHorizontalWallNeighbours(wall);
            var upperRightCell = cellList[0];
            var upperLeftCell = cellList[1];
            var bottomRightCell = cellList[2];
            var bottomLeftCell = cellList[3];
            upperRightCell.BottomConnect(bottomRightCell);
            upperLeftCell.BottomConnect(bottomLeftCell);
            bottomRightCell.UpperConnect(upperRightCell);
            bottomLeftCell.UpperConnect(upperLeftCell);
        }

        private List<Cell> FindVerticalWallNeighbours(Wall wall)
        {
            var leftUpperCoords = new CellCoords(wall.Coords.Top, wall.Coords.Left - 50);
            var leftBottomCoords = new CellCoords(wall.Coords.Top + 75, wall.Coords.Left - 50);
            var rightUpperCoords = new CellCoords(wall.Coords.Top, wall.Coords.Left + 25);
            var rightBottomCoords = new CellCoords(wall.Coords.Top + 75, wall.Coords.Left + 25);
            var leftUpperCell = FindCellByCoords(leftUpperCoords);
            var leftBottomCell = FindCellByCoords(leftBottomCoords);
            var rightUpperCell = FindCellByCoords(rightUpperCoords);
            var rightBottomCell = FindCellByCoords(rightBottomCoords);
            var list = new List<Cell>()
            {
                leftUpperCell,
                leftBottomCell,
                rightUpperCell,
                rightBottomCell
            };
            return list;
        }

        private List<Cell> FindHorizontalWallNeighbours(Wall wall)
        {
            var upperRightCoords = new CellCoords(wall.Coords.Top - 50, wall.Coords.Left + 75);
            var upperLeftCoords = new CellCoords(wall.Coords.Top - 50, wall.Coords.Left);
            var bottomRightCoords = new CellCoords(wall.Coords.Top + 25, wall.Coords.Left + 75);
            var bottomLeftCoords = new CellCoords(wall.Coords.Top + 25, wall.Coords.Left);
            var upperRightCell = FindCellByCoords(upperRightCoords);
            var upperLeftCell = FindCellByCoords(upperLeftCoords);
            var bottomRightCell = FindCellByCoords(bottomRightCoords);
            var bottomLeftCell = FindCellByCoords(bottomLeftCoords);
            var list = new List<Cell>()
            {
                upperRightCell,
                upperLeftCell,
                bottomRightCell,
                bottomLeftCell
            };
            return list;
        }
    }
}