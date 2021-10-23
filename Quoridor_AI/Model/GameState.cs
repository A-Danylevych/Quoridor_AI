using System.Collections.Generic;
using System.Linq;
using Quoridor;

namespace Quoridor_AI.Model
{
    internal class GameState
    {
        public bool InPlay {get; private set;}
        public List<Cell> TopWinningCells { get; private set;}
        public List<Cell> BottomWinningCells { get; private set;}
        public  Player Winner { get; private set; }

        public bool CheckTopWinning(Player player)
        {
            if (!CheckWinning(player.CurrentCell, TopWinningCells)) return false;
            Winner = player;
            return true;
        }
        public bool CheckBottomWinning(Player player)
        {
            if (!CheckWinning(player.CurrentCell, BottomWinningCells)) return false;
            Winner = player;
            return true;

        }

        private bool CheckWinning(Cell currentCell, List<Cell> winningCells)
        {
            if (winningCells.Any(cell => currentCell == cell))
            {
                InPlay = false;
                return true;
            }

            return false;
        }
        public GameState(List<Cell> topWinningCells, List<Cell> bottomWinningCells)
        {
            TopWinningCells = topWinningCells;
            BottomWinningCells = bottomWinningCells;
            InPlay = true;
        }
    }
}