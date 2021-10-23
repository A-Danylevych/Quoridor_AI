using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Model
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
        public Cell FindCellByCoords(CellCoords coords)
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
            var LeftUpperCell = cellList[0];
            var LeftBottomCell = cellList[1];
            var RightUpperCell = cellList[2];
            var RightBottomCell = cellList[3];

            if (LeftUpperCell.DownCell is Wall && RightUpperCell.DownCell is Wall)
            {
                if (LeftUpperCell.DownCell == RightBottomCell.DownCell)
                {
                    return false;
                }
            }

            if (LeftBottomCell.UpCell is Wall && RightBottomCell.UpCell is Wall)
            {
                if (LeftBottomCell.UpCell == RightBottomCell.UpCell)
                {
                    return false;
                }
            }

            if (LeftBottomCell.RightCell is Wall || LeftUpperCell.RightCell is Wall)
            {
                return false;
            }

            if (RightBottomCell.LeftCell is Wall || RightUpperCell.LeftCell is Wall)
            {
                return false;
            }

            return true;
        }

        private bool CanBePlacedHorizontal(Wall wall)
        {
            var cellList = FindHorizontalWallNeighbours(wall);
            foreach (var cell in cellList)
            {
                if (cell == null)
                {
                    return false;
                }
            }
            var UpperRightCell = cellList[0];
            var UpperLeftCell = cellList[1];
            var BottomRightCell = cellList[2];
            var BottomLeftCell = cellList[3];

            if (UpperLeftCell.DownCell is Wall || UpperRightCell.DownCell is Wall)
            {
                return false;
            }

            if (BottomLeftCell.UpCell is Wall || BottomRightCell.UpCell is Wall)
            {
                return false;
            }

            if(BottomLeftCell.RightCell is Wall && UpperLeftCell.RightCell is Wall)
            {
                if (BottomLeftCell.RightCell == UpperLeftCell.RightCell)
                {
                    return false;
                }
            }

            if (BottomRightCell.LeftCell is Wall && UpperRightCell.LeftCell is Wall)
            {
                if (BottomRightCell.LeftCell == UpperRightCell.LeftCell)
                {
                    return false;
                }
            }

            return true;
        }
        private void PutVerticalWall(Wall wall)
        {
            var cellList = FindVerticalWallNeighbours(wall);
            var LeftUpperCell = cellList[0];
            var LeftBottomCell = cellList[1];
            var RightUpperCell = cellList[2];
            var RightBottomCell = cellList[3];
            LeftUpperCell.RightConnect(wall);
            LeftBottomCell.RightConnect(wall);
            RightUpperCell.LeftConnect(wall);
            RightBottomCell.LeftConnect(wall);
            var leftList = new List<IPlaceable>()
            {
                LeftUpperCell,
                LeftBottomCell
            };
            var rightList = new List<IPlaceable>()
            {
                RightUpperCell,
                RightBottomCell
            };
            wall.LeftConnect(leftList);
            wall.RightConnect(rightList);
        }

        private void PutHorizontalWall(Wall wall)
        {
            var cellList = FindHorizontalWallNeighbours(wall);
            var UpperRightCell = cellList[0];
            var UpperLeftCell = cellList[1];
            var BottomRightCell = cellList[2];
            var BottomLeftCell = cellList[3];
            UpperRightCell.BottomConnect(wall);
            UpperLeftCell.BottomConnect(wall);
            BottomRightCell.UpperConnect(wall);
            BottomLeftCell.UpperConnect(wall);
            var upperList = new List<IPlaceable>()
            {
                UpperRightCell,
                UpperLeftCell
            };
            var bottomList = new List<IPlaceable>()
            {
                BottomRightCell,
                BottomLeftCell
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
            var LeftUpperCell = cellList[0];
            var LeftBottomCell = cellList[1];
            var RightUpperCell = cellList[2];
            var RightBottomCell = cellList[3];
            LeftUpperCell.RightConnect(RightUpperCell);
            LeftBottomCell.RightConnect(RightBottomCell);
            RightUpperCell.LeftConnect(LeftUpperCell);
            RightBottomCell.LeftConnect(LeftBottomCell);
        }

        private void DropHorizontalWall(Wall wall)
        {
            var cellList = FindHorizontalWallNeighbours(wall);
            var UpperRightCell = cellList[0];
            var UpperLeftCell = cellList[1];
            var BottomRightCell = cellList[2];
            var BottomLeftCell = cellList[3];
            UpperRightCell.BottomConnect(BottomRightCell);
            UpperLeftCell.BottomConnect(BottomLeftCell);
            BottomRightCell.UpperConnect(UpperRightCell);
            BottomLeftCell.UpperConnect(UpperLeftCell);
        }

        private List<Cell> FindVerticalWallNeighbours(Wall wall)
        {
            var LeftUpperCoords = new CellCoords(wall.Coords.Top, wall.Coords.Left - 50);
            var LeftBottomCoords = new CellCoords(wall.Coords.Top + 75, wall.Coords.Left - 50);
            var RightUpperCoords = new CellCoords(wall.Coords.Top, wall.Coords.Left + 25);
            var RightBottomCoords = new CellCoords(wall.Coords.Top + 75, wall.Coords.Left + 25);
            var LeftUpperCell = FindCellByCoords(LeftUpperCoords);
            var LeftBottomCell = FindCellByCoords(LeftBottomCoords);
            var RightUpperCell = FindCellByCoords(RightUpperCoords);
            var RightBottomCell = FindCellByCoords(RightBottomCoords);
            var list = new List<Cell>()
            {
                LeftUpperCell,
                LeftBottomCell,
                RightUpperCell,
                RightBottomCell
            };
            return list;
        }

        private List<Cell> FindHorizontalWallNeighbours(Wall wall)
        {
            var UpperRightCoords = new CellCoords(wall.Coords.Top - 50, wall.Coords.Left + 75);
            var UpperLeftCoords = new CellCoords(wall.Coords.Top - 50, wall.Coords.Left);
            var BottomRightCoords = new CellCoords(wall.Coords.Top + 25, wall.Coords.Left + 75);
            var BottomLeftCoords = new CellCoords(wall.Coords.Top + 25, wall.Coords.Left);
            var UpperRightCell = FindCellByCoords(UpperRightCoords);
            var UpperLeftCell = FindCellByCoords(UpperLeftCoords);
            var BottomRightCell = FindCellByCoords(BottomRightCoords);
            var BottomLeftCell = FindCellByCoords(BottomLeftCoords);
            var list = new List<Cell>()
            {
                UpperRightCell,
                UpperLeftCell,
                BottomRightCell,
                BottomLeftCell
            };
            return list;
        }
    }
}