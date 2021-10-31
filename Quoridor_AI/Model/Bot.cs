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
        private Board _board;

        public void MakeAMove(IController controller, Player otherPlayer)
        {
            var (path, score) = Minimax(this, otherPlayer, 3, int.MinValue,
                int.MaxValue, new Dictionary<Cell, Action>() { { CurrentCell, Action.Move } },
                true, _winningTop);
            var (wall, wallScore) = MinimaxWall(this, otherPlayer, 3);
            if (wallScore > score && _board.CanBePlaced(wall))
            {
                controller.SetAction(Action.Wall);
                controller.SetWall(wall.Coords.Top, wall.Coords.Left, wall.IsVertical);
            }
            else
            {
                var cells = new List<Cell>(path.Keys);
                var actions = new List<Action>(path.Values);
                controller.SetAction(actions[1]);
                controller.SetCell(cells[1].Coords.Top, cells[1].Coords.Left);
            }

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

        private static (Dictionary<Cell, Action> path, int score) Minimax(Player player, Player otherPlayer, int depth, 
            int alpha, int beta, Dictionary<Cell, Action> visited, bool maximizingPlayer, int targetTop)
        {
            var unVisitedNeighbors = player.CurrentCell.GetNeighbors().Except(visited.Keys);
            if (!unVisitedNeighbors.Any() || depth == 0)
            {
                return(visited, Evaluation(visited, targetTop, player.CurrentCell.Coords.Top));   
            }

            if (maximizingPlayer)
            {
                var best = int.MinValue;
                var bestPath = new Dictionary<Cell, Action>(visited);
                Cell currentBest = null;
                var placedToPath = false;
                foreach (var jumping in new[] {false, true})
                {
                    foreach (var neighbor in MoveValidator.PossibleToMoveCells(player, otherPlayer,
                        jumping))
                    {
                        visited[neighbor] = jumping ? Action.Jump : Action.Move;
                        var (_, score) = Minimax(player, otherPlayer, depth - 1,
                            alpha, beta, visited, false, targetTop);
                        if (score > best)
                        {
                            best = score;
                            if (placedToPath)
                            {
                                bestPath.Remove(currentBest);
                            }

                            placedToPath = true;
                            currentBest = neighbor;

                            bestPath[neighbor] = jumping ? Action.Jump : Action.Move;

                        }

                        visited.Remove(neighbor);
                        alpha = Math.Max(alpha, best);
                        if (beta <= alpha)
                        {
                            break;
                        }
                    }
                }

                return (bestPath, best);
                
            }
            else
            {
                var best = int.MaxValue;
                var bestPath = new Dictionary<Cell, Action>(visited);
                Cell currentBest = null;
                var placedToPath = false;
                foreach (var jumping in new[] {false, true})
                {
                    foreach (var neighbor in MoveValidator.PossibleToMoveCells(player, otherPlayer, 
                        jumping))
                    {
                        visited[neighbor] = jumping ? Action.Jump : Action.Move;
                        var (_, score) = Minimax(player, otherPlayer, depth - 1,
                            alpha, beta, visited, true, targetTop);
                        if (score < best)
                        {
                            best = score;
                            if (placedToPath)
                            {
                                bestPath.Remove(currentBest);
                            }

                            currentBest = neighbor;
                            placedToPath = true;

                            bestPath[neighbor] = jumping ? Action.Jump : Action.Move;

                        }

                        visited.Remove(neighbor);
                        beta = Math.Min(beta, best);
                        if (beta <= alpha)
                        {
                            break;
                        }
                    }
                }

                return (bestPath, best);
            }
            
        }

        private (Wall wall, int wallScore) MinimaxWall(Player currentPlayer, Player otherPlayer, int depth)
        {
            var (dict, score) = Minimax(otherPlayer, currentPlayer, depth, 
                int.MinValue, int.MaxValue, new Dictionary<Cell, Action>()
                    {{otherPlayer.CurrentCell, Action.Move}}, true, 
                _losingTop);
            
            var list = new List<Cell>(dict.Keys);
            
            int leftCoord;
            if (otherPlayer.CurrentCell.Coords.Top == list[1].Coords.Top)
            {
                var topCoord = otherPlayer.CurrentCell.Coords.Top;
                if (otherPlayer.CurrentCell.Coords.Left == list[1].Coords.Left-75)
                {
                    leftCoord = otherPlayer.CurrentCell.Coords.Left + 50;
                    return (new Wall(new CellCoords(topCoord, leftCoord), true), score);
                }

                leftCoord = otherPlayer.CurrentCell.Coords.Left - 50;
                return (new Wall(new CellCoords(topCoord, leftCoord), true), score);
            }

            leftCoord = otherPlayer.CurrentCell.Coords.Left;
            if (otherPlayer.CurrentCell.Coords.Top == list[1].Coords.Top - 75)
            {
                var topCoord = otherPlayer.CurrentCell.Coords.Left + 50;
                return (new Wall(new CellCoords(topCoord, leftCoord), false), score);
            }
            else
            {
                var topCoord = otherPlayer.CurrentCell.Coords.Left - 50;
                return (new Wall(new CellCoords(topCoord, leftCoord), false), score);
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

        public Bot(Color color, Cell cell, int winningTop, Board board) : base(color, cell)
        {
            _winningTop = winningTop;
            _board = board;
            _losingTop = cell.Coords.Top;
            WallSpots();
        }

        private static int Evaluation(Dictionary<Cell, Action> path, int top, int currentTop)
        {  
            var value = 0;
            foreach (var(cell,action) in path)
            {

                var tempValue = Math.Abs(currentTop - top) - Math.Abs(cell.Coords.Top - top);
                currentTop = cell.Coords.Top;
                var list = cell.GetNeighbors();
                if(list.Count == 1)
                {
                    tempValue = -1000;
                }
                if(Action.Move == action)
                {
                    value += tempValue;
                }
                else
                {
                    value += tempValue*2;
                }
            }
            return value;
        }
    }
}