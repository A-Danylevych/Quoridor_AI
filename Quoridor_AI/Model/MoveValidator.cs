using System.Collections.Generic;
using System.Linq;

namespace Model
{
    internal static class MoveValidator
    {
        public static bool IsValidMove(Cell cell, Player currentPlayer, Player otherPlayer)
        {
            var possibleMoves = PossibleToMoveCells(currentPlayer, otherPlayer);
            return possibleMoves.Any(possibleCell => possibleCell.Coords.Equals(cell.Coords));
        }
        public static List<Cell> PossibleToMoveCells(Player currentPlayer, Player otherPlayer)
        {
            var possibleToMove = new List<Cell>();
            possibleToMove = MoveIsValid(currentPlayer, possibleToMove);       
            possibleToMove = CheckForOtherPlayer(currentPlayer, otherPlayer, possibleToMove);
            return possibleToMove;
        }
        public static bool IsThereAWay(GameState gameState, Player topPlayer, Player bottomPlayer)
        {
            return FindAWay(gameState.BottomWinningCells, bottomPlayer.CurrentCell) && 
                   FindAWay(gameState.TopWinningCells, topPlayer.CurrentCell);
        }
        private static bool FindAWay(ICollection<Cell> cells, Cell cell)
        {
            var stackCells = new Stack<Cell>();
            var visited = new List<Cell>();
            
            stackCells.Push(cell);
            visited.Add(cell);

            while (stackCells.Count != 0)
            {
                var currentCell = stackCells.Pop();
                foreach (var next in currentCell.GetNeighbors().Where(next => !visited.Contains(next)))
                {
                    if(cells.Contains(next))
                    {
                        return true;
                    }
                    stackCells.Push(next);
                    visited.Add(next);
                }
            }
            return false;
        }
        private static List<Cell> MoveIsValid(Player player, List<Cell> possibleToMove)
        {
            if(player.CurrentCell.UpCell is Cell cell)
            {
                possibleToMove.Add(cell);
            }
            if(player.CurrentCell.DownCell is Cell downCell)
            {
                possibleToMove.Add(downCell);
            }
            if(player.CurrentCell.LeftCell is Cell leftCell)
            {
                possibleToMove.Add(leftCell);
            }
            if(player.CurrentCell.RightCell is Cell RightCell)
            {
                possibleToMove.Add(RightCell);
            }
            return possibleToMove;
        }

        private static List<Cell> CheckForOtherPlayer(Player currentPlayer, Player otherPlayer, 
            List<Cell> possibleToMove)
        {   
            if(currentPlayer.CurrentCell.DownCell == otherPlayer.CurrentCell)
            {   
                possibleToMove.Remove(otherPlayer.CurrentCell);
                if(otherPlayer.CurrentCell.DownCell is Cell cell)
                {
                    possibleToMove.Add(cell);
                }
                else
                {
                    if (otherPlayer.CurrentCell.RightCell is Cell rightCell)
                    {
                        possibleToMove.Add(rightCell);
                    }

                    if (otherPlayer.CurrentCell.LeftCell is Cell leftCell)
                    {
                        possibleToMove.Add(leftCell);
                    }
                }
            }
            if(currentPlayer.CurrentCell.UpCell == otherPlayer.CurrentCell)
            {
                possibleToMove.Remove(otherPlayer.CurrentCell);
                if(otherPlayer.CurrentCell.UpCell is Cell cell)
                {
                    possibleToMove.Add(cell);
                }
                else
                {
                    if (otherPlayer.CurrentCell.RightCell is Cell rightCell)
                    {
                        possibleToMove.Add(rightCell);
                    }

                    if (otherPlayer.CurrentCell.LeftCell is Cell leftCell)
                    {
                        possibleToMove.Add(leftCell);
                    }
                }
            }
            if(currentPlayer.CurrentCell.LeftCell == otherPlayer.CurrentCell)
            {
                possibleToMove.Remove(otherPlayer.CurrentCell);
                if(otherPlayer.CurrentCell.LeftCell is Cell cell)
                {
                    possibleToMove.Add(cell);
                }
                else
                {
                    if (otherPlayer.CurrentCell.UpCell is Cell upCell)
                    {
                        possibleToMove.Add(upCell);
                    }

                    if (otherPlayer.CurrentCell.DownCell is Cell downCell)
                    {
                        possibleToMove.Add(downCell);
                    }
                }
            }
            if(currentPlayer.CurrentCell.RightCell == otherPlayer.CurrentCell)
            {
                possibleToMove.Remove(otherPlayer.CurrentCell);
                if(otherPlayer.CurrentCell.RightCell is Cell cell )
                {
                    possibleToMove.Add(cell);
                }
                else
                {
                    if (otherPlayer.CurrentCell.UpCell is Cell upCell)
                    {
                        possibleToMove.Add(upCell);
                    }

                    if (otherPlayer.CurrentCell.DownCell is Cell downCell)
                    {
                        possibleToMove.Add(downCell);
                    }
                }
            }
            return possibleToMove;
        }
    }
}