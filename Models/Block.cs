using System;
using System.Collections.Generic;

namespace TetrisGame.Models
{
    /// <summary>
    /// 表示俄罗斯方块游戏中的一个方块
    /// </summary>
    public abstract class Block
    {
        // 方块的位置（左上角坐标）
        public int X { get; set; }
        public int Y { get; set; }
        
        // 方块颜色
        public string? Color { get; protected set; }
        
        // 方块的形状矩阵，true表示有方块，false表示空
        protected bool[,] Shape { get; set; }
        
        // 当前旋转状态
        protected int RotationState { get; set; } = 0;
        
        // 所有可能的旋转状态
        protected List<bool[,]> RotationShapes { get; set; }
        
        protected Block()
        {
            RotationShapes = new List<bool[,]>();
            InitializeRotationShapes();
            Shape = RotationShapes[0];
        }
        
        /// <summary>
        /// 初始化所有可能的旋转状态
        /// </summary>
        protected abstract void InitializeRotationShapes();
        
        /// <summary>
        /// 顺时针旋转方块
        /// </summary>
        public void Rotate()
        {
            RotationState = (RotationState + 1) % RotationShapes.Count;
            Shape = RotationShapes[RotationState];
        }
        
        /// <summary>
        /// 获取方块当前形状
        /// </summary>
        public bool[,] GetShape()
        {
            return Shape;
        }
        
        /// <summary>
        /// 获取方块宽度
        /// </summary>
        public int GetWidth()
        {
            return Shape.GetLength(1);
        }
        
        /// <summary>
        /// 获取方块高度
        /// </summary>
        public int GetHeight()
        {
            return Shape.GetLength(0);
        }
        
        /// <summary>
        /// 移动方块
        /// </summary>
        public void Move(int deltaX, int deltaY)
        {
            X += deltaX;
            Y += deltaY;
        }
    }
}