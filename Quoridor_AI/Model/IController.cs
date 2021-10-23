namespace Model
{
    public interface IController
    {
        Action GetAction();
        Cell GetCell();
        Wall GetWall();

        void SetAction(Action action);
        void SetCell(int top, int left);
        void SetWall(int top, int left, bool isVertical);
    }
}