namespace Quoridor_AI.Model
{
    public interface IViewer
    {
        void RenderEnding(string message);
        void RenderUpperPlayer(int top, int left, Action action);
        void RenderBottomPlayer(int top, int left, Action action);
        void RenderWall(int top, int left, bool isVertical);
        void RenderRemainingWalls(int topCount, int bottomCount);
    }
}