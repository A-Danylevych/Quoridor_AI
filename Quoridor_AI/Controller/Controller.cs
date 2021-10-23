using Model;

namespace Quoridor.Controller
{
    public class Controller : IController
    {
        private Action Action { get; set; }
        private Cell Cell { get; set; }
        private Wall Wall { get; set; }

        public void SetAction(Action action)
        {
            Action = action;
        }
        public Action GetAction()
        {
            return Action;
        }
        public void SetCell(int top, int left)
        {
            var coords = new CellCoords(top, left);
            Cell = new Cell(coords);
        }

        public Cell GetCell()
        {
            return Cell;
        }
        public void SetWall(int top, int left, bool isVertical)
        {
            var coords = new CellCoords(top, left);
            Wall = new Wall(coords, isVertical);
        }
        public Wall GetWall()
        {
            return Wall;
        }
    }
}

