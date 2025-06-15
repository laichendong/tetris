using System;

namespace TetrisGame.Models
{
    /// <summary>
    /// O形方块（正方形）
    /// </summary>
    public class OBlock : Block
    {
        public OBlock()
        {
            Color = "#E6D55A"; // 柔和黄色
        }
        
        protected override void InitializeRotationShapes()
        {
            // O形方块旋转后形状不变，只有一种状态
            RotationShapes.Add(new bool[,]
            {
                { true, true },
                { true, true }
            });
        }
    }
}