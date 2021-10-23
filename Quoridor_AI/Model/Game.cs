using System;
using System.Collections.Generic;

namespace Model
{
    public class Game
    {
        private IController Controller {get; set;}
        private IViewer Viewer {get; set;}
        private Player _topPlayer;
        private Player _bottomPlayer;
        private Player _currentPlayer;
        private Player _otherPlayer;
        private GameState _gameState;
        private static Game _instance;
        private readonly Board _board;
        private static readonly object SyncRoot = new object();
        private bool blocked;
        private Game(IController controller, IViewer viewer, bool withBot)
        {
            Controller = controller;
            Viewer = viewer;
            _board = new Board();
           NewGame(withBot);
        }

        public void NewGame(bool withBot)
        {
            _board.NewBoard();
            var topStartPosition = _board.TopStartPosition();
            if (withBot)
            {
                _topPlayer = new Bot(Color.Green, topStartPosition);
            }
            else
            {
                _topPlayer = new Player(Color.Green, topStartPosition);
            }
            var bottomStartPosition = _board.BottomStartPosition();
            _bottomPlayer = new Player(Color.Red, bottomStartPosition);
            _currentPlayer = _bottomPlayer;
            _otherPlayer = _topPlayer;
            var topWinningCells = _board.TopWinningCells();
            var bottomWinningCells = _board.BottomWinningCells();
            _gameState = new GameState(topWinningCells, bottomWinningCells);
            blocked = false;
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
                blocked = false;
                Update();
            }
        }
        private bool CheckWinning()
        {
            return _currentPlayer == _bottomPlayer ? _gameState.CheckBottomWinning(_bottomPlayer) : 
                _gameState.CheckTopWinning(_topPlayer);
        }

        private void RenderPlayer(int top, int left)
        {
            if (_currentPlayer == _bottomPlayer)
            {
                Viewer.RenderBottomPlayer(top, left);
            }
            else
            {
                Viewer.RenderUpperPlayer(top, left);
            }
        }

        public void Update()
        {
            switch (Controller.GetAction())
                {
                    case Action.MakeMove:
                        var cell = Controller.GetCell();
                        if (MoveValidator.IsValidMove(cell, _currentPlayer, _otherPlayer))
                        {
                            _board.MovePlayer(_currentPlayer, cell);
                            var playerCoords = _currentPlayer.CurrentCell.Coords;
                            RenderPlayer(playerCoords.Top, playerCoords.Left);
                            CheckWinning();
                            ChangeCurrentPlayer();
                        }

                        break;
                    case Action.PlaceWall:
                        var wall = Controller.GetWall();
                        if (_board.CanBePlaced(wall) && _currentPlayer.PlaceWall())
                        {
                            
                            _board.PutWall(wall);
                             if (MoveValidator.IsThereAWay(_gameState, _topPlayer, _bottomPlayer))
                             {
                                var wallCoords = wall.Coords;
                                Viewer.RenderWall(wallCoords.Top, wallCoords.Left);
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
                    default:
                        throw new ArgumentException();
                }

                if (!_gameState.InPlay)
                {
                    Viewer.RenderEnding(_gameState.Winner.Color + " player won!");
                }
            BotMove();
        }

        public static Game GetInstance(IController controller, IViewer viewer, bool withBot = false)
        {
            if(_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new Game(controller, viewer, withBot);
                    }
                }   
            }
            return _instance;
        }
    }
}