namespace Quoridor_AI.Model
{
    public class CellCoords
    {
        public int Top {get; }
        public int Left {get; }

        private bool Equals(CellCoords other)
        {
            return Top == other.Top && Left == other.Left;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((CellCoords) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Top * 397) ^ Left;
            }
        }

        public CellCoords(int top, int left)
        {
            Top = top;
            Left = left;
        }
    }
     
}