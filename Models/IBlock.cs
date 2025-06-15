using System;

namespace TetrisGame.Models
{
    /// <summary>
    /// I形方块（长条形）
    /// </summary>
    public class IBlock : Block
    {
        public IBlock()
        {
            Color = "#5CBDBD"; // 柔和青色
        }
        
        protected override void InitializeRotationShapes()
        {
            // 水平方向
            RotationShapes.Add(new bool[,]
            {
                { false, false, false, false },
                { true, true, true, true },
                { false, false, false, false },
                { false, false, false, false }
            });
            
            // 垂直方向
            RotationShapes.Add(new bool[,]
            {
                { false, false, true, false },
                { false, false, true, false },
                { false, false, true, false },
                { false, false, true, false }
            });
        }
    }
}