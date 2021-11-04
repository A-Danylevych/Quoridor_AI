using System;
using System.Collections.Generic;
using System.Linq;

namespace Quoridor_AI.Model
{
    internal class Bot : Player
    {
        private List<Wall> _wallsSpots;
        private readonly int _winningTop;
        private readonly int _losingTop;
        private readonly Board _board;
        private readonly GameState _gameState;
        private readonly bool _topPlayer;
        public void MakeAMove(IController controller, Player otherPlayer)
        {
            var (path, score) = Minimax(this, otherPlayer, 3, int.MinValue,
                int.MaxValue, new Dictionary<Cell, Action>() {{CurrentCell, Action.Move}},
                true, _winningTop);
            var (wall, wallScore) = MinimaxWall(this, otherPlayer, 3);
            
            var jumpingCells = MoveValidator.PossibleToMoveCells(this, otherPlayer, true);
            if (jumpingCells.Count>0)
            {
                foreach (var jump in jumpingCells.Where(jump => Math.Abs(jump.Coords.Top - _winningTop) 
                    - Math.Abs(CurrentCell.Coords.Top - _winningTop) < 0))
                {
                    controller.SetAction(Action.Jump);
                    controller.SetCell(jump.Coords.Top,jump.Coords.Left);
                    return;
                }
            }
            Player topPlayer;
            Player bottomPlayer;
            if (_topPlayer)
            {
                topPlayer = this;
                bottomPlayer = otherPlayer;
            }
            else
            {
                topPlayer = otherPlayer;
                bottomPlayer = this;
            }
            if (wallScore > score && _board.CanBePlaced(wall) && PlaceWall())
            {
                _board.PutWall(wall);
                if (MoveValidator.IsThereAWay(_gameState, topPlayer, bottomPlayer))
                {
                    controller.SetAction(Action.Wall);
                    controller.SetWall(wall.Coords.Top, wall.Coords.Left, wall.IsVertical);
                    UnPlaceWall();
                    _board.DropWall(wall);
                    return;
                }
                _board.DropWall(wall);
            }
            
            var cells = new List<Cell>(path.Keys);
            var actions = new List<Action>(path.Values);
            var (road, goal) = AStar(CurrentCell, _winningTop);
            var cell = GetCell(road, CurrentCell, goal);
            if (actions[1] == Action.Jump || !MoveValidator.IsValidMove(cell, this, otherPlayer))
            {
                if (actions[1] == Action.Jump && !MoveValidator.IsValidJump(cells[1], this, otherPlayer))
                {
                    if (CurrentCell.GetNeighbors().Any(neighbor => 
                        MoveValidator.IsValidMove(neighbor, this, otherPlayer)))
                    {
                        controller.SetAction(actions[1]);
                        controller.SetCell(cells[1].Coords.Top, cells[1].Coords.Left);
                        return;
                    }
                }
                controller.SetAction(actions[1]);
                controller.SetCell(cells[1].Coords.Top, cells[1].Coords.Left);
                return;
            }

            const int startCoords = 25;
            const int cellDistance = 75;
            var temp = CurrentCell;
            _board.MovePlayer(this, cell);
            if (MoveValidator.PossibleToMoveCells(otherPlayer, this, true).Count > 0 && 
                Math.Abs(temp.Coords.Top - _winningTop - startCoords)/cellDistance -
                Math.Abs(otherPlayer.CurrentCell.Coords.Top - _losingTop - startCoords)/cellDistance < 0)
            {
                _board.MovePlayer(this, temp);
                var list = new List<Cell>();
                if (CurrentCell.LeftCell is not Wall)
                {
                    list.Add((Cell)CurrentCell.LeftCell);
                }

                if (CurrentCell.RightCell is not Wall)
                {
                    list.Add((Cell)CurrentCell.RightCell);
                }
                list.Add(cell);
                foreach (var neighbor in list.Where(neighbor => 
                    MoveValidator.IsValidMove(neighbor, this, otherPlayer)))
                {
                    var tempCell = CurrentCell;
                    _board.MovePlayer(this, neighbor);
                    if (MoveValidator.PossibleToMoveCells(otherPlayer,this, true).Count>0)
                    {
                        _board.MovePlayer(this, tempCell);
                        continue;
                    }
                    _board.MovePlayer(this, tempCell);
                    controller.SetAction(Action.Move);
                    controller.SetCell(neighbor.Coords.Top, neighbor.Coords.Left);
                    return;
                }
            }
            _board.MovePlayer(this, temp);
            
            controller.SetAction(Action.Move);
            controller.SetCell(cell.Coords.Top,cell.Coords.Left);
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
            var actions = new List<Action>(dict.Values);

            const int cellDistance = 75;
            const int rightDownWall = 50;
            const int leftUpWall = 25;
            int leftCoord;
            if (actions[1] == Action.Move)
            {
                var (road, goal) = AStar(otherPlayer.CurrentCell, _losingTop);
                var cell = GetCell(road, otherPlayer.CurrentCell, goal);
                if (otherPlayer.CurrentCell.Coords.Top == cell.Coords.Top)
                {
                    var topCoord = otherPlayer.CurrentCell.Coords.Top;
                    if (otherPlayer.CurrentCell.Coords.Left -  cellDistance == cell.Coords.Left)
                    {
                        leftCoord = otherPlayer.CurrentCell.Coords.Left - leftUpWall;
                        return (new Wall(new CellCoords(topCoord, leftCoord), true), score);
                    }

                    leftCoord = otherPlayer.CurrentCell.Coords.Left + rightDownWall;
                    return (new Wall(new CellCoords(topCoord, leftCoord), true), score);
                }

                leftCoord = otherPlayer.CurrentCell.Coords.Left;
                if (otherPlayer.CurrentCell.Coords.Top - cellDistance == cell.Coords.Top)
                {
                    var topCoord = otherPlayer.CurrentCell.Coords.Top - leftUpWall;
                    return (new Wall(new CellCoords(topCoord, leftCoord), false), score);
                }
                else
                {
                    var topCoord = otherPlayer.CurrentCell.Coords.Top + rightDownWall;
                    return (new Wall(new CellCoords(topCoord, leftCoord), false), score);
                }
            }

            if (Math.Abs(currentPlayer.CurrentCell.Coords.Top - _losingTop) 
                > Math.Abs(otherPlayer.CurrentCell.Coords.Top - _losingTop))
            {
                return (new Wall(new CellCoords(0, 0), true), int.MinValue);
            }

            if (otherPlayer.CurrentCell.Coords.Top == currentPlayer.CurrentCell.Coords.Top)
            {
                var topCoord = otherPlayer.CurrentCell.Coords.Top;
                if (otherPlayer.CurrentCell.Coords.Left - cellDistance == currentPlayer.CurrentCell.Coords.Left)
                {
                    leftCoord = otherPlayer.CurrentCell.Coords.Left - leftUpWall;
                    return (new Wall(new CellCoords(topCoord, leftCoord), true), score);
                }

                leftCoord = otherPlayer.CurrentCell.Coords.Left + rightDownWall;
                return (new Wall(new CellCoords(topCoord, leftCoord), true), score);
            }

            leftCoord = otherPlayer.CurrentCell.Coords.Left;
            if (otherPlayer.CurrentCell.Coords.Top - cellDistance == currentPlayer.CurrentCell.Coords.Top)
            {
                var topCoord = otherPlayer.CurrentCell.Coords.Left - leftUpWall;
                return (new Wall(new CellCoords(topCoord, leftCoord), false), score);
            }
            else
            {
                var topCoord = otherPlayer.CurrentCell.Coords.Left + rightDownWall;
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

        public Bot(Color color, Cell cell, int winningTop, Board board, GameState gameState) : base(color, cell)
        {
            _winningTop = winningTop;
            _board = board;
            _gameState = gameState;
            _losingTop = cell.Coords.Top;
            _topPlayer = color == Color.Black;
            WallSpots();
        }

        private static int Evaluation(Dictionary<Cell, Action> path, int top, int currentTop)
        {
            var value = 0;
            var coef = -1;
            const int cellDistance = 75;
            const int deadlockScore = -100000;
            foreach (var (cell, action) in path)
            {
                var tempValue = Math.Abs(currentTop - top) - Math.Abs(cell.Coords.Top - top);
                currentTop = cell.Coords.Top;
                var (road,_) = AStar(cell, top);
                if (road != null)
                {
                    tempValue -= cellDistance * road.Count;
                }
                var list = cell.GetNeighbors();
                if (list.Count == 1)
                {
                    tempValue = deadlockScore;
                }
                if (Action.Move == action)
                {
                    value += tempValue;
                }
                else
                {
                    value += coef*tempValue * 2;
                }
                coef *= -1;
            }
            return value;
        }

        private static (Dictionary<Cell, Cell> path, Cell goal) AStar(Cell start, int goal)
        {
            var queue = new PriorityQueue<Cell>();
            queue.Add(start, 0);
            var cameFrom = new Dictionary<Cell, Cell>();
            var costSoFar = new Dictionary<Cell, int>();
            cameFrom[start] = null;
            costSoFar[start] = 0;

            while (queue.TryDequeue(out var item, out var priority))
            {
                if (item.Coords.Top == goal)
                {
                    return (cameFrom, item);
                }
                foreach (var next in item.GetNeighbors())
                {
                    var newCost = costSoFar[item] + 1;
                    if (costSoFar.ContainsKey(next) && newCost >= costSoFar[next]) continue;
                    costSoFar[next] = newCost;
                    priority = newCost + Math.Abs(next.Coords.Top - goal)- Math.Abs(item.Coords.Top - goal);
                    queue.Add(next, priority);
                    cameFrom[next] = item;
                }
            }
            return (null, null);
        }

        private static Cell GetCell(IReadOnlyDictionary<Cell, Cell> path, Cell start, Cell goal)
        {
            var road = new List<Cell>();
            var current = goal;
            while (current != start)
            {
                road.Add(current);
                current = path[current];
            }
            return road[^1];
        }
    }
}
