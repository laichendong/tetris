using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using TetrisGame.Models;
using TetrisGame.Views;

namespace TetrisGame.Controllers
{
    /// <summary>
    /// 游戏控制器，处理用户输入和游戏逻辑
    /// </summary>
    public class GameController : IDisposable
    {
        // 游戏板
        private readonly GameBoard _gameBoard;
        
        // 游戏计时器
        private DispatcherTimer _gameTimer;
        
        // 游戏速度（毫秒）
        private int _gameSpeed = 500;
        
        // 游戏难度
        private GameDifficulty _difficulty = GameDifficulty.Medium;
        
        // 音频管理器
        private readonly AudioManager _audioManager;
        
        // 游戏状态变化事件
        public event EventHandler? GameStateChanged;
        
        // 返回菜单事件
        public event EventHandler? ReturnToMenu;
        
        public GameController()
        {
            _gameBoard = new GameBoard();
            _gameBoard.GameStateChanged += OnGameStateChanged;
            _gameBoard.LevelChanged += OnLevelChanged;
            _gameBoard.LinesClearedEvent += OnLinesCleared;
            
            // 初始化音频管理器
            _audioManager = new AudioManager();
            
            // 初始化游戏计时器
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_gameSpeed)
            };
            _gameTimer.Tick += GameTimerTick;
        }
        
        /// <summary>
        /// 游戏状态变化事件处理
        /// </summary>
        private void OnGameStateChanged(object? sender, EventArgs e)
        {
            // 如果游戏结束，播放游戏结束音效
            if (_gameBoard.IsGameOver)
            {
                _audioManager.PlayGameOverSound();
            }
            
            // 触发外部事件
            GameStateChanged?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// 行消除事件处理
        /// </summary>
        private void OnLinesCleared(object? sender, int clearedLines)
        {
            // 播放消除行音效
            _audioManager.PlayLineClearSound();
        }
        
        /// <summary>
        /// 游戏计时器回调，控制方块自动下落
        /// </summary>
        private void GameTimerTick(object? sender, EventArgs e)
        {
            if (!_gameBoard.IsGameOver && !_gameBoard.IsPaused)
            {
                _gameBoard.MoveBlock(0, 1); // 向下移动一格
            }
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            _gameBoard.Reset();
            // 根据难度设置初始速度
            SetInitialSpeed();
            _gameTimer.Start();
            
            // 开始播放背景音乐
            _audioManager.PlayBackgroundMusic();
        }
        
        /// <summary>
        /// 设置游戏难度
        /// </summary>
        public void SetDifficulty(GameDifficulty difficulty)
        {
            _difficulty = difficulty;
        }
        
        /// <summary>
        /// 停止游戏
        /// </summary>
        public void StopGame()
        {
            _gameTimer.Stop();
            // 停止背景音乐
            _audioManager.StopBackgroundMusic();
        }
        
        /// <summary>
        /// 获取当前游戏难度
        /// </summary>
        public GameDifficulty GetDifficulty()
        {
            return _difficulty;
        }
        
        /// <summary>
        /// 处理键盘输入
        /// </summary>
        public void HandleKeyInput(string key)
        {
            if (_gameBoard.IsGameOver && key != "Enter") return;
            
            switch (key)
            {
                case "Left":
                    _gameBoard.MoveBlock(-1, 0); // 向左移动
                    _audioManager.PlayMoveSound(); // 播放移动音效
                    break;
                    
                case "Right":
                    _gameBoard.MoveBlock(1, 0); // 向右移动
                    _audioManager.PlayMoveSound(); // 播放移动音效
                    break;
                    
                case "Down":
                    _gameBoard.MoveBlock(0, 1); // 向下移动
                    break;
                    
                case "Up":
                    _gameBoard.RotateBlock(); // 旋转
                    _audioManager.PlayRotateSound(); // 播放旋转音效
                    break;
                    
                case "Space":
                    _gameBoard.DropBlock(); // 快速下落
                    _audioManager.PlayFallSound(); // 播放下落音效
                    break;
                    
                case "Enter":
                    if (_gameBoard.IsGameOver)
                    {
                        ReturnToMenu?.Invoke(this, EventArgs.Empty); // 如果游戏结束，返回菜单
                    }
                    else
                    {
                        _gameBoard.TogglePause(); // 暂停/继续
                        // 根据游戏状态控制音乐
                        if (_gameBoard.IsPaused)
                        {
                            _audioManager.PauseBackgroundMusic();
                        }
                        else
                        {
                            _audioManager.ResumeBackgroundMusic();
                        }
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 获取游戏板
        /// </summary>
        public GameBoard GetGameBoard()
        {
            return _gameBoard;
        }
        
        /// <summary>
        /// 关卡升级事件处理
        /// </summary>
        private void OnLevelChanged(object? sender, int newLevel)
        {
            AdjustGameSpeed(newLevel);
        }
        
        /// <summary>
        /// 设置初始速度
        /// </summary>
        private void SetInitialSpeed()
        {
            int baseSpeed = _difficulty switch
            {
                GameDifficulty.Easy => 800,    // 简单：800ms
                GameDifficulty.Medium => 500,  // 中等：500ms
                GameDifficulty.Hard => 300,    // 困难：300ms
                GameDifficulty.Expert => 150,  // 专家：150ms
                _ => 500
            };
            
            _gameSpeed = baseSpeed;
            _gameTimer.Interval = TimeSpan.FromMilliseconds(_gameSpeed);
        }
        
        /// <summary>
        /// 调整游戏速度
        /// </summary>
        public void AdjustGameSpeed(int level)
        {
            // 根据难度和级别调整游戏速度
            int baseSpeed = _difficulty switch
            {
                GameDifficulty.Easy => 800,    // 简单：起始800ms
                GameDifficulty.Medium => 500,  // 中等：起始500ms
                GameDifficulty.Hard => 300,    // 困难：起始300ms
                GameDifficulty.Expert => 150,  // 专家：起始150ms
                _ => 500
            };
            
            // 根据难度调整速度递减幅度
            int speedDecrease = _difficulty switch
            {
                GameDifficulty.Easy => 30,     // 简单：每级减少30ms
                GameDifficulty.Medium => 50,   // 中等：每级减少50ms
                GameDifficulty.Hard => 40,     // 困难：每级减少40ms
                GameDifficulty.Expert => 20,   // 专家：每级减少20ms
                _ => 50
            };
            
            // 设置最低速度限制
            int minSpeed = _difficulty switch
            {
                GameDifficulty.Easy => 200,    // 简单：最快200ms
                GameDifficulty.Medium => 100,  // 中等：最快100ms
                GameDifficulty.Hard => 80,     // 困难：最快80ms
                GameDifficulty.Expert => 50,   // 专家：最快50ms
                _ => 100
            };
            
            _gameSpeed = Math.Max(minSpeed, baseSpeed - ((level - 1) * speedDecrease));
            _gameTimer.Interval = TimeSpan.FromMilliseconds(_gameSpeed);
        }
        
        /// <summary>
        /// 获取音频管理器
        /// </summary>
        public AudioManager GetAudioManager()
        {
            return _audioManager;
        }
        
        /// <summary>
        /// 播放点击音效（供菜单使用）
        /// </summary>
        public void PlayClickSound()
        {
            _audioManager.PlayClickSound();
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _gameTimer?.Stop();
            _audioManager?.Dispose();
        }
    }
}