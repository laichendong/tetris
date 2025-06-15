using System;

namespace TetrisGame.Models
{
    /// <summary>
    /// J形方块（L形的镜像）
    /// </summary>
    public class JBlock : Block
    {
        public JBlock()
        {
            Color = "#5A7BC7"; // 柔和蓝色
        }
        
        protected override void InitializeRotationShapes()
        {
            // J形向左
            RotationShapes.Add(new bool[,]
            {
                { false, false, false },
                { true, true, true },
                { false, false, true }
            });
            
            // J形向下
            RotationShapes.Add(new bool[,]
            {
                { false, true, false },
                { false, true, false },
                { true, true, false }
            });
            
            // J形向右
            RotationShapes.Add(new bool[,]
            {
                { true, false, false },
                { true, true, true },
                { false, false, false }
            });
            
            // J形向上
            RotationShapes.Add(new bool[,]
            {
                { false, true, true },
                { false, true, false },
                { false, true, false }
            });
        }
    }
}