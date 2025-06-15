using System.Collections.Generic;

namespace TetrisGame.Models
{
    /// <summary>
    /// P形方块（穿透方块）- 单格方块，具有穿透能力
    /// </summary>
    public class PBlock : Block
    {
        public PBlock()
        {
            Color = "#CC66CC"; // 柔和紫色，表示特殊能力
        }
        
        protected override void InitializeRotationShapes()
        {
            // 穿透方块只有一种形状：单个格子
            RotationShapes.Add(new bool[,]
            {
                { true }
            });
        }
        
        /// <summary>
        /// 检查穿透方块是否可以穿透到指定位置
        /// </summary>
        /// <param name="board">游戏板状态</param>
        /// <param name="targetY">目标Y坐标</param>
        /// <returns>是否可以穿透到目标位置</returns>
        public bool CanPenetrateTo(bool[,] board, int targetY)
        {
            int boardHeight = board.GetLength(0);
            int boardWidth = board.GetLength(1);
            
            // 检查目标位置是否在边界内
            if (targetY < 0 || targetY >= boardHeight || X < 0 || X >= boardWidth)
            {
                return false;
            }
            
            // 检查目标位置是否为空
            return !board[targetY, X];
        }
        
        /// <summary>
        /// 找到穿透方块可以到达的最低位置
        /// </summary>
        /// <param name="board">游戏板状态</param>
        /// <param name="startY">起始Y坐标</param>
        /// <returns>可以到达的最低Y坐标，如果无法穿透则返回起始位置</returns>
        public int FindLowestPenetrablePosition(bool[,] board, int startY)
        {
            int boardHeight = board.GetLength(0);
            int boardWidth = board.GetLength(1);
            
            // 检查X坐标是否在边界内
            if (X < 0 || X >= boardWidth)
            {
                return startY;
            }
            
            // 从底部开始向上查找第一个空位
            for (int y = boardHeight - 1; y >= startY; y--)
            {
                if (!board[y, X])
                {
                    return y;
                }
            }
            
            // 如果没有找到空位，返回起始位置
            return startY;
        }
    }
}