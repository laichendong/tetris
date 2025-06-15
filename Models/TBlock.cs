using System;

namespace TetrisGame.Models
{
    /// <summary>
    /// T形方块
    /// </summary>
    public class TBlock : Block
    {
        public TBlock()
        {
            Color = "#9966CC"; // 柔和紫色
        }
        
        protected override void InitializeRotationShapes()
        {
            // T形向下
            RotationShapes.Add(new bool[,]
            {
                { false, true, false },
                { true, true, true },
                { false, false, false }
            });
            
            // T形向右
            RotationShapes.Add(new bool[,]
            {
                { false, true, false },
                { false, true, true },
                { false, true, false }
            });
            
            // T形向上
            RotationShapes.Add(new bool[,]
            {
                { false, false, false },
                { true, true, true },
                { false, true, false }
            });
            
            // T形向左
            RotationShapes.Add(new bool[,]
            {
                { false, true, false },
                { true, true, false },
                { false, true, false }
            });
        }
    }
}