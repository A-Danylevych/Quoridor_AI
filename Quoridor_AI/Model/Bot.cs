using System;
using System.Collections.Generic;
using System.Linq;

namespace Quoridor_AI.Model
{
    internal class Bot : Player
    {
        private List<Wall> _wallsSpots;
        private int _winningTop;
        private int _losingTop;

        public void MakeAMove(IController controller, Player otherPlayer)
        {
            MakeARandomMove(controller, otherPlayer);
        }
        private void MakeARandomMove(IController controller, Player otherPlayer)
        {            
            var rand = new Random();
            var type = typeof(Action);
            var values = type.GetEnumValues();

            var index = rand.Next(values.Length);
			var action = (Action) values.GetValue(index);
            if (WallsCount == 0)
            {
                action = Action.Move;
            }

            if (action == Action.Jump && 
                MoveValidator.PossibleToMoveCells(this, otherPlayer, true).Count == 0)
            {
                action = Action.Move;
            }
            controller.SetAction(action);
            
            switch (action)
            {
                case Action.Move:
                {
                    var cells = MoveValidator.PossibleToMoveCells(this, otherPlayer, false);
                    var i = rand.Next(cells.Count);
                    var cell = cells[i];
                    controller.SetCell(cell.Coords.Top, cell.Coords.Left);
                    break;
                }
                case Action.Jump:
                {
                    var cells = MoveValidator.PossibleToMoveCells(this, otherPlayer, true);
                    var i = rand.Next(cells.Count);
                    var cell = cells[i];
                    controller.SetCell(cell.Coords.Top, cell.Coords.Left);
                    break;
                }
                case Action.Wall:
                {
                    var i = rand.Next(_wallsSpots.Count);
                    var wall = _wallsSpots[i];
                    _wallsSpots.Remove(wall);
                    controller.SetWall(wall.Coords.Top, wall.Coords.Left, wall.IsVertical);
                    break;
                }
            }
        }

        private (List<Cell> path, int score) Minimax(Cell playerPosition, Cell otherPlayerPosition, int depth, 
            int alpha, int beta, ICollection<Cell> visited, bool maximizingPlayer)
        {
            var unVisitedNeighbors = playerPosition.GetNeighbors().Except(visited);
            if (!unVisitedNeighbors.Any() || depth == 0)
            {
                return (null, 0);
            }

            if (maximizingPlayer)
            {
                var best = int.MinValue;
                var bestPath = new List<Cell>(visited);
                var placedToPath = false;
                foreach (var neighbor in playerPosition.GetNeighbors().Except(visited))
                {
                    visited.Add(neighbor);
                    var (_, score) = Minimax(playerPosition, otherPlayerPosition, depth - 1,
                        alpha, beta, visited, false);
                    if (score > best)
                    {
                        best = score;
                        if (placedToPath)
                        {
                            bestPath.RemoveAt(bestPath.Count -1);
                        }

                        placedToPath = true;
                            
                        bestPath.Add(neighbor);
                            
                    }

                    visited.Remove(neighbor);
                    alpha = Math.Max(alpha, best);
                    if(beta <= alpha)
                    {
                        break;
                    }
                }
                return (bestPath, best);
            }
            else
            {
                var best = int.MaxValue;
                var bestPath = new List<Cell>(visited);
                var placedToPath = false;
                foreach (var neighbor in playerPosition.GetNeighbors().Except(visited))
                {
                    visited.Add(neighbor);
                    var (_, score) = Minimax(playerPosition, otherPlayerPosition, depth - 1,
                        alpha, beta, visited, true);
                    if (score < best)
                    {
                        best = score;
                        if (placedToPath)
                        {
                            bestPath.RemoveAt(bestPath.Count -1);
                        }

                        placedToPath = true;
                            
                        bestPath.Add(neighbor);
                            
                    }

                    visited.Remove(neighbor);
                    beta = Math.Min(beta, best);
                    if(beta <= alpha)
                    {
                        break;
                    }
                }

                return (bestPath, best);
            }
        }
        
        private Wall MinimaxWall(Cell otherPlayerPosition, int depth, 
            int alpha, int beta, ICollection<Cell> visited, bool maximizingPlayer)
        {
            var (list, _) = Minimax(otherPlayerPosition, CurrentCell, depth, int.MinValue,
                int.MaxValue, new List<Cell>(), true);

            int leftCoord;
            if (otherPlayerPosition.Coords.Top == list[0].Coords.Top)
            {
                var topCoord = otherPlayerPosition.Coords.Top;
                if (otherPlayerPosition.Coords.Left == list[0].Coords.Left-75)
                {
                    leftCoord = otherPlayerPosition.Coords.Left + 50;
                    return new Wall(new CellCoords(topCoord, leftCoord), true);
                }

                leftCoord = otherPlayerPosition.Coords.Left - 50;
                return new Wall(new CellCoords(topCoord, leftCoord), true);
            }

            leftCoord = otherPlayerPosition.Coords.Left;
            if (otherPlayerPosition.Coords.Top == list[0].Coords.Top - 75)
            {
                var topCoord = otherPlayerPosition.Coords.Left + 50;
                return new Wall(new CellCoords(topCoord, leftCoord), false);
            }
            else
            {
                var topCoord = otherPlayerPosition.Coords.Left - 50;
                return new Wall(new CellCoords(topCoord, leftCoord), false);
            }
        }

        private void WallSpots()
        {
            _wallsSpots = new List<Wall>();
            FillHorizontal();
            FillVertical();
        }

        private void FillHorizontal()
        {
            var top = 75;
            var left = 25;
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var coord = new CellCoords(top, left);
                    _wallsSpots.Add(new Wall(coord, false));
                    left += 75;
                }
                left = 25;
                top += 75;
            } 
        }
        private void FillVertical()
        {
            var top = 25;
            var left = 75;
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var coord = new CellCoords(top, left);
                    _wallsSpots.Add(new Wall(coord, true));
                    left += 75;
                }
                left = 75;
                top += 75;
            } 
        }

        public Bot(Color color, Cell cell, int winningTop) : base(color, cell)
        {
            _winningTop = winningTop;
            _losingTop = cell.Coords.Top;
            WallSpots();
        }
    }
}