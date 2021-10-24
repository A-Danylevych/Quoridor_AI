using Quoridor_AI.Model;
using System;
using Action = Quoridor_AI.Model.Action;

namespace Quoridor_AI.View
{
    public class ConsoleView: IViewer
    {
        private bool _displayMove;
        private bool _gameNotEnded;
        private readonly Game _game;
        private readonly IController _controller;

        public void Run()
        {
            StartGame();
            while (_gameNotEnded)
            {
                var input = ReadCommand();
                if (input is not {Length: 2})
                {
                    continue;
                }

                var action = ReadAction(input[0]);
                var coords = ReadCoords(input[1]);
                if (action == null || coords == null)
                {
                    continue;
                }

                if (action != Action.Wall && input.Length == 3)
                {
                    continue;
                }
                SendCommand((Action) action, (Coords) coords);
                _game.Update();
            }
        }

        private static string[] ReadCommand()
        {
            var input = Console.ReadLine();
            return input?.Split(' ');
        }

        private static Action? ReadAction(string input)
        {
            switch (input)
            {
                case "move":
                    return Action.Move;
                case "jump":
                    return Action.Jump;
                case "wall":
                    return Action.Wall;
                default:
                    return null;
            }
        }

        private static Coords? ReadCoords(string input)
        {
            if (input.Length is not (2 or 3)) return null;
            Coords coords = default;
            var left = char.ToUpper(input[0]) - 64;
            if (!int.TryParse(input[1].ToString(), out var top))
            {
                return null;
            }

            coords.SetLeft(left);
            coords.SetTop(top);
            if (input.Length != 3) return coords;
            left -= 18;
            coords.SetLeft(left);
            switch (input[2].ToString().ToUpper())
            {
                case "H":
                    coords.SetPosition(false);
                    break;
                case "V":
                    coords.SetPosition(true);
                    break;
                default:
                    return null;
            }
            return coords;
        }

        private void SendCommand(Action action, Coords coords)
        {
            _controller.SetAction(action);
            if (action == Action.Wall)
            {
                _controller.SetWall(coords.Top, coords.Left, coords.Vertical);
            }
            else
            {
                _controller.SetCell(coords.Top, coords.Left);
            }
        }
        
        private void StartGame()
        {
            while (true)
            {
                var input = Console.ReadLine();
                switch (input)
                {
                    case "black":
                        _game.NewGame(Color.Black);
                        _displayMove = true;
                        _gameNotEnded = true;
                        return;
                    case "white":
                        _game.NewGame(Color.White);
                        _displayMove = false;
                        _gameNotEnded = true;
                        _game.Update();
                        return;
                }
            }

        }

        public ConsoleView(IController controller)
        {
            _controller = controller;
            _game = Game.GetInstance(_controller, this, Color.Black);
        }

        public void RenderEnding(string message)
        {
            _gameNotEnded = false;
            Console.WriteLine(message);
        }

        public void RenderUpperPlayer(int top, int left, Action action)
        {
            RenderMove(top, left, action);
        }

        public void RenderBottomPlayer(int top, int left, Action action)
        {
           RenderMove(top, left, action);
        }

        private void RenderMove(int top, int left, Action action)
        {
            _displayMove = !_displayMove;
            if (!_displayMove) return;
            var text = CoordsString(top, left);
            Console.WriteLine(action.ToString().ToLower() + " "+ text);
        }

        private static string CoordsString(int top, int left, bool isWall = false)
        {
            const int difference = 75;
            const int firstMember = 25;
            
            left = (left - firstMember) / difference + 1;
            top = (top - firstMember) / difference + 1;
            var startSymbol = 65;
            
            if (isWall)
            {
                startSymbol = 83;
            }

            return (char)(startSymbol + (left - 1)) + top.ToString();
        }
        public void RenderWall(int top, int left, bool isVertical)
        {
            _displayMove = !_displayMove;
            if (!_displayMove) return;
            var position = isVertical ? "v" : "h";
            const int offset = 50;
            if (isVertical)
            {
                left -= offset;
            }
            else
            {
                top -= offset;
            }

            var text = CoordsString(top, left, true);
            Console.WriteLine("wall " + text + position);
        }

        public void RenderRemainingWalls(int topCount, int bottomCount)
        {
            
        }
    }

    public struct Coords
    {
        public int Top { get; private set; }
        public int Left { get; private set; }
        public bool Vertical { get; private set; }
        private const int FirstMember = 25;
        private const int Difference = 75;

        public void SetLeft(int index)
        {
            Left = FirstMember + (index - 1) * Difference;
        }

        public void SetTop(int index)
        {
            Top = FirstMember + (index - 1) * Difference;
        }

        public void SetPosition(bool isVertical)
        {
            const int offset = 50;
            Vertical = isVertical;
            if (isVertical)
            {
                Left += offset;
            }
            else
            {
                Top += offset;
            }
        }
    }
}