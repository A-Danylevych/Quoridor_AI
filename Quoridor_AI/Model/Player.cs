namespace Model
{
    class Player
    {
        public Player(Color color, Cell cell)
        {
            CurrentCell = cell; 
            Color = color;
            WallsCount = 10;
        }
        public Cell CurrentCell{ get; private set;}
        public Color Color{get; private set;}
        public int WallsCount{ get; private set;}
         public bool PlaceWall()
        {
            if(WallsCount > 0){
                WallsCount --;
                return true;
            }
            return false;
        }
        public void UnPlaceWall(){
            WallsCount ++;
        }

        public void ChangeCell(Cell cell)
        {
            CurrentCell = cell;
        }
    }
}