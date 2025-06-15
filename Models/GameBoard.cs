using System;
using System.Collections.Generic;

namespace TetrisGame.Models
{
    /// <summary>
    /// 游戏板，管理游戏状态和逻辑
    /// </summary>
    public class GameBoard
    {
        // 游戏板的宽度和高度（以格子为单位）
        public const int Width = 10;
        public const int Height = 20;
        
        // 游戏板状态，true表示有方块，false表示空
        private bool[,] _board;
        
        // 游戏板上的方块颜色
        private string[,] _colors;
        
        // 当前活动的方块
        public Block CurrentBlock { get; private set; }
        
        // 下一个方块
        public Block NextBlock { get; private set; }
        
        // 游戏是否结束
        public bool IsGameOver { get; private set; }
        
        // 游戏是否暂停
        public bool IsPaused { get; private set; }
        
        // 游戏分数
        public int Score { get; private set; }
        
        // 消除的行数
        public int LinesCleared { get; private set; }
        
        // 当前关卡
        public int Level { get; private set; }
        
        // 游戏状态变化事件
        public event EventHandler GameStateChanged;
        
        // 关卡升级事件
        public event EventHandler<int> LevelChanged;
        
        // 行消除事件
        public event EventHandler<int> LinesClearedEvent;
        
        public GameBoard()
        {
            _board = new bool[Height, Width];
            _colors = new string[Height, Width];
            Reset();
        }
        
        /// <summary>
        /// 重置游戏
        /// </summary>
        public void Reset()
        {
            // 清空游戏板
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    _board[row, col] = false;
                    _colors[row, col] = string.Empty;
                }
            }
            
            // 重置游戏状态
            IsGameOver = false;
            IsPaused = false;
            Score = 0;
            LinesCleared = 0;
            Level = 1;
            
            // 创建初始方块
            NextBlock = BlockFactory.CreateRandomBlock();
            CreateNewBlock();
            
            // 触发状态变化事件
            OnGameStateChanged();
        }
        
        /// <summary>
        /// 创建新的方块
        /// </summary>
        private void CreateNewBlock()
        {
            CurrentBlock = NextBlock;
            NextBlock = BlockFactory.CreateRandomBlock();
            
            // 设置方块的初始位置（居中，顶部）
            CurrentBlock.X = (Width - CurrentBlock.GetWidth()) / 2;
            CurrentBlock.Y = 0;
            
            // 检查游戏是否结束（新方块与现有方块重叠）
            if (IsCollision())
            {
                IsGameOver = true;
            }
            
            // 触发状态变化事件
            OnGameStateChanged();
        }
        
        /// <summary>
        /// 移动当前方块
        /// </summary>
        public bool MoveBlock(int deltaX, int deltaY)
        {
            if (IsGameOver || IsPaused) return false;
            
            // 特殊处理穿透方块的向下移动
            if (CurrentBlock is PBlock pBlock && deltaY > 0)
            {
                return MovePenetratingBlock(pBlock, deltaX, deltaY);
            }
            
            // 临时移动方块
            CurrentBlock.Move(deltaX, deltaY);
            
            // 检查是否发生碰撞
            if (IsCollision())
            {
                // 如果发生碰撞，撤销移动
                CurrentBlock.Move(-deltaX, -deltaY);
                
                // 如果是向下移动导致的碰撞，则固定方块
                if (deltaY > 0)
                {
                    PlaceBlock();
                }
                
                return false;
            }
            
            // 触发状态变化事件
            OnGameStateChanged();
            return true;
        }
        
        /// <summary>
        /// 检查穿透方块是否可以穿透一格
        /// </summary>
        /// <param name="pBlock">穿透方块</param>
        /// <returns>是否可以穿透一格</returns>
        private bool CanPenetrateOneStep(PBlock pBlock)
        {
            int nextY = pBlock.Y + 1;
            
            // 检查是否超出底部边界
            if (nextY >= Height)
            {
                return false;
            }
            
            // 检查下方是否有空格可以穿透到
            for (int y = nextY + 1; y < Height; y++)
            {
                if (!_board[y, pBlock.X])
                {
                    return true; // 找到空格，可以穿透
                }
            }
            
            return false; // 下方没有空格，无法穿透
        }
        
        /// <summary>
        /// 移动穿透方块的特殊逻辑
        /// </summary>
        /// <param name="pBlock">穿透方块</param>
        /// <param name="deltaX">X方向移动距离</param>
        /// <param name="deltaY">Y方向移动距离</param>
        /// <returns>是否移动成功</returns>
        private bool MovePenetratingBlock(PBlock pBlock, int deltaX, int deltaY)
        {
            // 先尝试水平移动
            if (deltaX != 0)
            {
                pBlock.Move(deltaX, 0);
                if (IsCollision())
                {
                    pBlock.Move(-deltaX, 0);
                    return false;
                }
            }
            
            // 处理穿透向下移动
            if (deltaY > 0)
            {
                // 先尝试正常向下移动一格
                pBlock.Move(0, deltaY);
                
                // 检查是否发生碰撞
                if (IsCollision())
                {
                    // 发生碰撞，撤销移动
                    pBlock.Move(0, -deltaY);
                    
                    // 检查是否可以穿透（逐格穿透）
                    if (CanPenetrateOneStep(pBlock))
                    {
                        // 可以穿透一格，继续向下移动
                        pBlock.Move(0, deltaY);
                        OnGameStateChanged();
                        return true;
                    }
                    else
                    {
                        // 无法穿透，固定方块
                        PlaceBlock();
                        return false;
                    }
                }
                else
                {
                    // 没有碰撞，检查是否到达底部
                    if (pBlock.Y >= Height - 1)
                    {
                        PlaceBlock();
                        return false;
                    }
                    
                    OnGameStateChanged();
                    return true;
                }
            }
            
            OnGameStateChanged();
            return true;
        }
        
        /// <summary>
        /// 旋转当前方块
        /// </summary>
        public bool RotateBlock()
        {
            if (IsGameOver || IsPaused) return false;
            
            // 保存当前状态
            int originalX = CurrentBlock.X;
            int originalY = CurrentBlock.Y;
            bool[,] originalShape = CurrentBlock.GetShape();
            
            // 尝试旋转
            CurrentBlock.Rotate();
            
            // 检查是否发生碰撞
            if (IsCollision())
            {
                // 如果发生碰撞，尝试进行墙踢（wall kick）
                // 尝试向左移动
                CurrentBlock.Move(-1, 0);
                if (!IsCollision())
                {
                    OnGameStateChanged();
                    return true;
                }
                
                // 尝试向右移动
                CurrentBlock.Move(2, 0);
                if (!IsCollision())
                {
                    OnGameStateChanged();
                    return true;
                }
                
                // 尝试向上移动（适用于I形方块）
                CurrentBlock.Move(-1, -1);
                if (!IsCollision())
                {
                    OnGameStateChanged();
                    return true;
                }
                
                // 如果所有尝试都失败，恢复原始状态
                CurrentBlock.X = originalX;
                CurrentBlock.Y = originalY;
                
                // 由于我们无法直接恢复Shape（它是由Rotate方法内部管理的），
                // 我们需要继续旋转直到回到原始状态
                while (!ShapesEqual(CurrentBlock.GetShape(), originalShape))
                {
                    CurrentBlock.Rotate();
                }
                
                return false;
            }
            
            // 触发状态变化事件
            OnGameStateChanged();
            return true;
        }
        
        /// <summary>
        /// 比较两个形状是否相同
        /// </summary>
        private bool ShapesEqual(bool[,] shape1, bool[,] shape2)
        {
            if (shape1.GetLength(0) != shape2.GetLength(0) || shape1.GetLength(1) != shape2.GetLength(1))
                return false;
                
            for (int i = 0; i < shape1.GetLength(0); i++)
            {
                for (int j = 0; j < shape1.GetLength(1); j++)
                {
                    if (shape1[i, j] != shape2[i, j])
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 快速下落当前方块
        /// </summary>
        public void DropBlock()
        {
            if (IsGameOver || IsPaused) return;
            
            // 特殊处理穿透方块的快速下落
            if (CurrentBlock is PBlock pBlock)
            {
                int targetY = pBlock.FindLowestPenetrablePosition(_board, pBlock.Y);
                if (targetY > pBlock.Y)
                {
                    pBlock.Y = targetY;
                    OnGameStateChanged();
                }
                PlaceBlock();
                return;
            }
            
            // 持续向下移动方块，直到发生碰撞
            while (MoveBlock(0, 1)) { }
        }
        
        /// <summary>
        /// 检查当前方块是否与游戏板或边界发生碰撞
        /// </summary>
        private bool IsCollision()
        {
            bool[,] shape = CurrentBlock.GetShape();
            int blockHeight = CurrentBlock.GetHeight();
            int blockWidth = CurrentBlock.GetWidth();
            
            for (int row = 0; row < blockHeight; row++)
            {
                for (int col = 0; col < blockWidth; col++)
                {
                    // 只检查方块中有实际方块的部分
                    if (shape[row, col])
                    {
                        int boardRow = CurrentBlock.Y + row;
                        int boardCol = CurrentBlock.X + col;
                        
                        // 检查是否超出边界
                        if (boardRow < 0 || boardRow >= Height || boardCol < 0 || boardCol >= Width)
                        {
                            return true;
                        }
                        
                        // 检查是否与游戏板上的方块重叠
                        if (_board[boardRow, boardCol])
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 将当前方块固定到游戏板上
        /// </summary>
        private void PlaceBlock()
        {
            bool[,] shape = CurrentBlock.GetShape();
            int blockHeight = CurrentBlock.GetHeight();
            int blockWidth = CurrentBlock.GetWidth();
            
            for (int row = 0; row < blockHeight; row++)
            {
                for (int col = 0; col < blockWidth; col++)
                {
                    if (shape[row, col])
                    {
                        int boardRow = CurrentBlock.Y + row;
                        int boardCol = CurrentBlock.X + col;
                        
                        // 确保在游戏板范围内
                        if (boardRow >= 0 && boardRow < Height && boardCol >= 0 && boardCol < Width)
                        {
                            _board[boardRow, boardCol] = true;
                            _colors[boardRow, boardCol] = CurrentBlock.Color ?? "#FFFFFF"; // Default to white if color is null
                        }
                    }
                }
            }
            
            // 检查并清除已完成的行
            ClearCompletedRows();
            
            // 创建新的方块
            CreateNewBlock();
        }
        
        /// <summary>
        /// 清除已完成的行
        /// </summary>
        private void ClearCompletedRows()
        {
            int linesCleared = 0;
            
            for (int row = Height - 1; row >= 0; row--)
            {
                bool isRowComplete = true;
                
                // 检查当前行是否已满
                for (int col = 0; col < Width; col++)
                {
                    if (!_board[row, col])
                    {
                        isRowComplete = false;
                        break;
                    }
                }
                
                if (isRowComplete)
                {
                    // 清除当前行并将上方的行下移
                    for (int r = row; r > 0; r--)
                    {
                        for (int col = 0; col < Width; col++)
                        {
                            _board[r, col] = _board[r - 1, col];
                            _colors[r, col] = _colors[r - 1, col];
                        }
                    }
                    
                    // 清空顶部行
                    for (int col = 0; col < Width; col++)
                    {
                        _board[0, col] = false;
                        _colors[0, col] = string.Empty;
                    }
                    
                    // 由于行已下移，需要重新检查当前行
                    row++;
                    linesCleared++;
                }
            }
            
            // 更新分数和已清除的行数
            if (linesCleared > 0)
            {
                // 根据一次清除的行数计算分数（1行=100分，2行=300分，3行=500分，4行=800分）
                int points = linesCleared switch
                {
                    1 => 100,
                    2 => 300,
                    3 => 500,
                    4 => 800,
                    _ => 0
                };
                
                Score += points;
                LinesCleared += linesCleared;
                
                // 触发行消除事件
                OnLinesCleared(linesCleared);
                
                // 检查是否需要升级关卡
                CheckLevelUp();
                
                // 触发状态变化事件
                OnGameStateChanged();
            }
        }
        
        /// <summary>
        /// 获取游戏板状态
        /// </summary>
        public bool[,] GetBoard()
        {
            // 创建游戏板的副本
            bool[,] boardCopy = new bool[Height, Width];
            Array.Copy(_board, boardCopy, _board.Length);
            
            // 将当前方块添加到游戏板副本中
            if (!IsGameOver)
            {
                bool[,] shape = CurrentBlock.GetShape();
                int blockHeight = CurrentBlock.GetHeight();
                int blockWidth = CurrentBlock.GetWidth();
                
                for (int row = 0; row < blockHeight; row++)
                {
                    for (int col = 0; col < blockWidth; col++)
                    {
                        if (shape[row, col])
                        {
                            int boardRow = CurrentBlock.Y + row;
                            int boardCol = CurrentBlock.X + col;
                            
                            // 确保在游戏板范围内
                            if (boardRow >= 0 && boardRow < Height && boardCol >= 0 && boardCol < Width)
                            {
                                boardCopy[boardRow, boardCol] = true;
                            }
                        }
                    }
                }
            }
            
            return boardCopy;
        }
        
        /// <summary>
        /// 获取游戏板上的颜色
        /// </summary>
        public string[,] GetColors()
        {
            // 创建颜色数组的副本
            string[,] colorsCopy = new string[Height, Width];
            Array.Copy(_colors, colorsCopy, _colors.Length);
            
            // 将当前方块的颜色添加到副本中
            if (!IsGameOver)
            {
                bool[,] shape = CurrentBlock.GetShape();
                int blockHeight = CurrentBlock.GetHeight();
                int blockWidth = CurrentBlock.GetWidth();
                
                for (int row = 0; row < blockHeight; row++)
                {
                    for (int col = 0; col < blockWidth; col++)
                    {
                        if (shape[row, col])
                        {
                            int boardRow = CurrentBlock.Y + row;
                            int boardCol = CurrentBlock.X + col;
                            
                            // 确保在游戏板范围内
                            if (boardRow >= 0 && boardRow < Height && boardCol >= 0 && boardCol < Width)
                            {
                                colorsCopy[boardRow, boardCol] = CurrentBlock.Color ?? "#FFFFFF"; // Default to white if color is null
                            }
                        }
                    }
                }
            }
            
            return colorsCopy;
        }
        
        /// <summary>
        /// 切换游戏暂停状态
        /// </summary>
        public void TogglePause()
        {
            if (!IsGameOver)
            {
                IsPaused = !IsPaused;
                OnGameStateChanged();
            }
        }
        
        /// <summary>
        /// 检查是否需要升级关卡
        /// </summary>
        private void CheckLevelUp()
        {
            // 每1000分升一级
            int newLevel = (Score / 1000) + 1;
            
            if (newLevel > Level)
            {
                Level = newLevel;
                // 触发关卡升级事件
                LevelChanged?.Invoke(this, Level);
            }
        }
        
        /// <summary>
        /// 触发游戏状态变化事件
        /// </summary>
        private void OnGameStateChanged()
        {
            GameStateChanged?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// 触发行消除事件
        /// </summary>
        private void OnLinesCleared(int clearedLines)
        {
            LinesClearedEvent?.Invoke(this, clearedLines);
        }
    }
}