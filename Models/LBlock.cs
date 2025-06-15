using System;

namespace TetrisGame.Models
{
    /// <summary>
    /// L形方块
    /// </summary>
    public class LBlock : Block
    {
        public LBlock()
        {
            Color = "#E6A85C"; // 柔和橙色
        }
        
        protected override void InitializeRotationShapes()
        {
            // L形向右
            RotationShapes.Add(new bool[,]
            {
                { false, false, false },
                { true, true, true },
                { true, false, false }
            });
            
            // L形向上
            RotationShapes.Add(new bool[,]
            {
                { true, true, false },
                { false, true, false },
                { false, true, false }
            });
            
            // L形向左
            RotationShapes.Add(new bool[,]
            {
                { false, false, true },
                { true, true, true },
                { false, false, false }
            });
            
            // L形向下
            RotationShapes.Add(new bool[,]
            {
                { false, true, false },
                { false, true, false },
                { false, true, true }
            });
        }
    }
}