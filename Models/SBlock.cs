using System;

namespace TetrisGame.Models
{
    /// <summary>
    /// S形方块
    /// </summary>
    public class SBlock : Block
    {
        public SBlock()
        {
            Color = "#6BC56B"; // 柔和绿色
        }
        
        protected override void InitializeRotationShapes()
        {
            // S形水平
            RotationShapes.Add(new bool[,]
            {
                { false, false, false },
                { false, true, true },
                { true, true, false }
            });
            
            // S形垂直
            RotationShapes.Add(new bool[,]
            {
                { false, true, false },
                { false, true, true },
                { false, false, true }
            });
        }
    }
}