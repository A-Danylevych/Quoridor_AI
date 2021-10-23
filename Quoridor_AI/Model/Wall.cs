using System.Collections.Generic;

namespace Model
{
    public class Wall : IPlaceable
    {
        public CellCoords Coords { get; private set;}
        public bool IsVertical { get; private set; }
        public List<IPlaceable> LeftCell { get; private set; }
        public List<IPlaceable> RightCell { get; private set; }
        public List<IPlaceable> UpCell { get; private set; }
        public List<IPlaceable> DownCell { get; private set; }
        public Wall(CellCoords coords, bool isVertical)
        {
            Coords = coords;
            IsVertical = isVertical;
            LeftCell = new List<IPlaceable>();
            RightCell = new List<IPlaceable>();
            UpCell = new List<IPlaceable>();
            DownCell = new List<IPlaceable>();
        }

        public Wall()
        {
            
        }
        public void UpperConnect(List<IPlaceable> placeable) => Connect(placeable, Direction.Up);
        public void BottomConnect(List<IPlaceable> placeable) => Connect(placeable, Direction.Down);
        public void RightConnect(List<IPlaceable> placeable) => Connect(placeable, Direction.Right);
        public void LeftConnect(List<IPlaceable> placeable) => Connect(placeable, Direction.Left);
        private void Connect(List<IPlaceable> placeable, Direction direction)
        {
            switch (direction)
            {
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
        void IPlaceable.UpperConnect(IPlaceable placeable) => Connect(placeable, Direction.Up);
        void IPlaceable.BottomConnect(IPlaceable placeable) => Connect(placeable, Direction.Down);
        void IPlaceable.RightConnect(IPlaceable placeable) => Connect(placeable, Direction.Right);
        void IPlaceable.LeftConnect(IPlaceable placeable) => Connect(placeable, Direction.Left);
        private void Connect(IPlaceable placeable, Direction direction)
        {
            switch(direction){
                case Direction.Up:
                    UpCell.Add(placeable);
                    break;
                case Direction.Down:
                    DownCell.Add(placeable);
                    break;
                case Direction.Left:
                    LeftCell.Add(placeable);
                    break;
                case Direction.Right:
                    RightCell.Add(placeable);
                    break;
                default:
                    throw new System.ArgumentException("Unexpected direction");
            }
        }
    }
}