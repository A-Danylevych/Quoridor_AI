using System;

namespace Quoridor_AI.Model
{
    public class Game
    {
        private IController Controller {get; set;}
        private IViewer Viewer {get; set; }
        private Player _topPlayer;
        private Player _bottomPlayer;
        private Player _currentPlayer;
        private Player _otherPlayer;
        private GameState _gameState;
        private static Game _instance;
        private readonly Board _board;
        private static readonly object SyncRoot = new object();
        private Game(IController controller, IViewer viewer, Color botColor)
        {
            Controller = controller;
            Viewer = viewer;
            _board = new Board();
           NewGame(botColor);
        }

        public void NewGame(Color botColor)
        {
            _board.NewBoard();
            var topStartPosition = _board.TopStartPosition();
            var bottomStartPosition = _board.BottomStartPosition();
            if (botColor == Color.Black)
            {
                _topPlayer = new Bot(botColor, topStartPosition);
                _bottomPlayer = new Player(Color.White, bottomStartPosition);
            }
            else
            {
                _bottomPlayer = new Bot(botColor, bottomStartPosition);
                _topPlayer = new Player(Color.Black, topStartPosition);
            }
            _currentPlayer = _bottomPlayer;
            _otherPlayer = _topPlayer;
            var topWinningCells = _board.TopWinningCells();
            var bottomWinningCells = _board.BottomWinningCells();
            _gameState = new GameState(topWinningCells, bottomWinningCells);
        }
        private void ChangeCurrentPlayer()
        {
            if(_currentPlayer == _bottomPlayer)
            {
                _otherPlayer = _bottomPlayer;
                _currentPlayer = _topPlayer;
            }
            else
            {
                _otherPlayer = _topPlayer;
                _currentPlayer = _bottomPlayer;
            }
        }

        private void BotMove()
        {
            if(_currentPlayer is Bot bot)
            {
                bot.MakeAMove(Controller, _otherPlayer);
                Update();
            }
        }
        private void CheckWinning()
        {
            var unused = _currentPlayer == _bottomPlayer ? _gameState.CheckBottomWinning(_bottomPlayer) : 
                _gameState.CheckTopWinning(_topPlayer);
        }

        private void RenderPlayer(int top, int left, Action action)
        {
            if (_currentPlayer == _bottomPlayer)
            {
                Viewer.RenderBottomPlayer(top, left, action);
            }
            else
            {
                Viewer.RenderUpperPlayer(top, left, action);
            }
        }

        public void Update()
        {
            Cell cell;
            switch (Controller.GetAction())
                {
                    case Action.Move:
                        cell = Controller.GetCell();
                        if (MoveValidator.IsValidMove(cell, _currentPlayer, _otherPlayer))
                        {
                            _board.MovePlayer(_currentPlayer, cell);
                            var playerCoords = _currentPlayer.CurrentCell.Coords;
                            RenderPlayer(playerCoords.Top, playerCoords.Left, Action.Move);
                            CheckWinning();
                            ChangeCurrentPlayer();
                        }

                        break;
                    case Action.Jump:
                        cell = Controller.GetCell();
                        if (MoveValidator.IsValidJump(cell, _currentPlayer, _otherPlayer))
                        {
                            _board.MovePlayer(_currentPlayer, cell);
                            var playerCoords = _currentPlayer.CurrentCell.Coords;
                            RenderPlayer(playerCoords.Top, playerCoords.Left, Action.Jump);
                            CheckWinning();
                            ChangeCurrentPlayer();
                        }
                        break;
                    case Action.Wall:
                        var wall = Controller.GetWall();
                        if (_board.CanBePlaced(wall) && _currentPlayer.PlaceWall())
                        {
                            
                            _board.PutWall(wall);
                             if (MoveValidator.IsThereAWay(_gameState, _topPlayer, _bottomPlayer))
                             {
                                var wallCoords = wall.Coords;
                                Viewer.RenderWall(wallCoords.Top, wallCoords.Left, wall.IsVertical);
                                ChangeCurrentPlayer();
                             }
                             else
                             {
                                 _board.DropWall(wall);
                                 _currentPlayer.UnPlaceWall();
                             }
                        }

                        Viewer.RenderRemainingWalls(_topPlayer.WallsCount, _bottomPlayer.WallsCount);
                        break;
                }

                if (!_gameState.InPlay)
                {
                    Viewer.RenderEnding(_gameState.Winner.Color + " player won!");
                }
            BotMove();
        }

        public static Game GetInstance(IController controller, IViewer viewer, Color botColor)
        {
            if(_instance == null)
            {
                lock (SyncRoot)
                {
                    _instance ??= new Game(controller, viewer, botColor);
                }   
            }
            return _instance;
        }
    }
}