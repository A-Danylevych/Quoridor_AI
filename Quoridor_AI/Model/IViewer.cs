using System.Collections.Generic;

namespace Model
{
    public interface IViewer
    {
        void RenderEnding(string message);
        void RenderUpperPlayer(int top, int left);
        void RenderBottomPlayer(int top, int left);
        void RenderWall(int top, int left);
        void RenderRemainingWalls(int topCount, int bottomCount);
    }
}