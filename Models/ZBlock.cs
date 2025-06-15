using System;

namespace TetrisGame.Models
{
    /// <summary>
    /// Z形方块（S形的镜像）
    /// </summary>
    public class ZBlock : Block
    {
        public ZBlock()
        {
            Color = "#D85656"; // 柔和红色
        }
        
        protected override void InitializeRotationShapes()
        {
            // Z形水平
            RotationShapes.Add(new bool[,]
            {
                { false, false, false },
                { true, true, false },
                { false, true, true }
            });
            
            // Z形垂直
            RotationShapes.Add(new bool[,]
            {
                { false, false, true },
                { false, true, true },
                { false, true, false }
            });
        }
    }
}