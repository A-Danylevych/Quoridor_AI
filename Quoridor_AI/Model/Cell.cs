using System.Collections.Generic;


namespace Model
{
    public class Cell : IPlaceable
    {
        public CellCoords Coords { get; private set;}
        public IPlaceable LeftCell { get; private set; }
        public IPlaceable RightCell { get; private set; }
        public IPlaceable UpCell { get; private set; }
        public IPlaceable DownCell { get; private set; }
        public void UpperConnect(IPlaceable placeable) => Connect(placeable, Direction.Up);
        public void BottomConnect(IPlaceable placeable) => Connect(placeable, Direction.Down);
        public void RightConnect(IPlaceable placeable) => Connect(placeable, Direction.Right);
        public void LeftConnect(IPlaceable placeable) => Connect(placeable, Direction.Left);
        private void Connect(IPlaceable placeable, Direction direction)
        {
            switch(direction){
                case Direction.Up:
                    UpCell = placeable;
                    break;
                case Direction.Down:
                    DownCell = placeable;
                    break;
                case Direction.Left:
                    LeftCell = placeable;
                    break;
                case Direction.Right:
                    RightCell = placeable;
                    break;
                default:
                    throw new System.ArgumentException("Unexpected direction");
            }
        }
        public List<Cell> GetNeighbors()
        {
            var neighbors = new List<Cell>();
            if(!(UpCell is Wall))
            {
                neighbors.Add((Cell)UpCell);
            }
            if(!(DownCell is Wall))
            {
                neighbors.Add((Cell)DownCell);
            }
            if(!(LeftCell is Wall))
            {
                neighbors.Add((Cell)LeftCell);
            }
            if(!(RightCell is Wall))
            {
                neighbors.Add((Cell)RightCell);
            }
            return neighbors;
        }
        public Cell(CellCoords coords)
        {
            Coords = coords;
            var abstractWall = new Wall();
            LeftCell = abstractWall;
            RightCell = abstractWall;
            UpCell = abstractWall;
            DownCell = abstractWall;
        }
    }
}