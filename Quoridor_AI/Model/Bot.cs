using System;
using System.Collections.Generic;

namespace Quoridor_AI.Model
{
    internal class Bot : Player
    {
        private List<Wall> _wallsSpots;

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

        private void Minimax(Cell playerPosition, Cell otherPlayerPosition, int depth, int alpha, int beta, 
            List<Cell> visited, bool maximizingPlayer)
        {
            if (visited.Count == 81)
            {
                
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

        public Bot(Color color, Cell cell) : base(color, cell)
        {
            WallSpots();
        }
    }
}