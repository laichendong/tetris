using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using TetrisGame.Controllers;
using TetrisGame.Models;

namespace TetrisGame.Views
{
    public partial class GameView : UserControl
    {
        // 游戏控制器
        private readonly GameController _gameController;
        
        // UI元素
        private Canvas? _gameCanvas;
        private Canvas? _nextBlockCanvas;
        private TextBlock? _scoreText;
        private TextBlock? _linesText;
        private TextBlock? _levelText;
        private TextBlock? _difficultyText;
        private TextBlock? _gameStatusText;
        private Border? _gameOverPanel;
        private Border? _pausePanel;
        
        // 方块大小
        private const int BlockSize = 28;
        // 下一个方块预览的方块大小
        private const int NextBlockSize = 22;
        
        public GameView()
        {
            InitializeComponent();
            
            // 获取UI元素
            _gameCanvas = this.FindControl<Canvas>("GameCanvas");
            _nextBlockCanvas = this.FindControl<Canvas>("NextBlockCanvas");
            _scoreText = this.FindControl<TextBlock>("ScoreText");
            _linesText = this.FindControl<TextBlock>("LinesText");
            _levelText = this.FindControl<TextBlock>("LevelText");
            _difficultyText = this.FindControl<TextBlock>("DifficultyText");
            _gameStatusText = this.FindControl<TextBlock>("GameStatusText");
            _gameOverPanel = this.FindControl<Border>("GameOverPanel");
            _pausePanel = this.FindControl<Border>("PausePanel");

            if (_gameCanvas != null)
            {
                _gameCanvas.Width = GameBoard.Width * BlockSize;
                _gameCanvas.Height = GameBoard.Height * BlockSize;
            }
            
            // 创建游戏控制器
            _gameController = new GameController();
            _gameController.GameStateChanged += OnGameStateChanged;
            // 不自动开始游戏，等待菜单选择难度后再开始
            
            // 设置键盘焦点和事件处理
            this.Focusable = true;
            this.KeyDown += OnKeyDown;
            this.Loaded += (sender, args) => this.Focus();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        
        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    _gameController.HandleKeyInput("Left");
                    break;
                    
                case Key.Right:
                    _gameController.HandleKeyInput("Right");
                    break;
                    
                case Key.Down:
                    _gameController.HandleKeyInput("Down");
                    break;
                    
                case Key.Up:
                    _gameController.HandleKeyInput("Up");
                    break;
                    
                case Key.Space:
                    _gameController.HandleKeyInput("Space");
                    break;
                    
                case Key.Enter:
                    _gameController.HandleKeyInput("Enter");
                    break;
            }
        }
        
        /// <summary>
        /// 获取游戏控制器
        /// </summary>
        public GameController GetGameController()
        {
            return _gameController;
        }
        

        /// <summary>
        /// 游戏状态变化处理
        /// </summary>
        private void OnGameStateChanged(object? sender, EventArgs e)
        {
            // 在UI线程上更新界面
            Dispatcher.UIThread.Post(() =>
            {
                UpdateGameView();
            });
        }
        
        /// <summary>
        /// 更新游戏视图
        /// </summary>
        private void UpdateGameView()
        {
            GameBoard gameBoard = _gameController.GetGameBoard();
            
            // 更新分数、行数和关卡
            if (_scoreText != null) _scoreText.Text = gameBoard.Score.ToString();
            if (_linesText != null) _linesText.Text = gameBoard.LinesCleared.ToString();
            if (_levelText != null) _levelText.Text = gameBoard.Level.ToString();
            
            // 更新难度显示
            if (_difficultyText != null) _difficultyText.Text = GetDifficultyDisplayText(_gameController.GetDifficulty());
            
            // 更新游戏状态文本和弹层显示
            if (gameBoard.IsGameOver)
            {
                if (_gameStatusText != null) _gameStatusText.Text = "游戏结束";
                if (_gameOverPanel != null) _gameOverPanel.IsVisible = true;
                if (_pausePanel != null) _pausePanel.IsVisible = false;
            }
            else if (gameBoard.IsPaused)
            {
                if (_gameStatusText != null) _gameStatusText.Text = "游戏暂停";
                if (_pausePanel != null) _pausePanel.IsVisible = true;
                if (_gameOverPanel != null) _gameOverPanel.IsVisible = false;
            }
            else
            {
                if (_gameStatusText != null) _gameStatusText.Text = "游戏进行中";
                if (_gameOverPanel != null) _gameOverPanel.IsVisible = false;
                if (_pausePanel != null) _pausePanel.IsVisible = false;
            }
            
            // 绘制游戏板
            DrawGameBoard(gameBoard);
            
            // 绘制下一个方块
            DrawNextBlock(gameBoard.NextBlock);
        }
        
        /// <summary>
        /// 获取难度显示文本
        /// </summary>
        private string GetDifficultyDisplayText(GameDifficulty difficulty)
        {
            return difficulty switch
            {
                GameDifficulty.Easy => "简单",
                GameDifficulty.Medium => "中等",
                GameDifficulty.Hard => "困难",
                GameDifficulty.Expert => "专家",
                _ => "中等"
            };
        }
        
        /// <summary>
        /// 绘制游戏板
        /// </summary>
        private void DrawGameBoard(GameBoard gameBoard)
        {
            if (_gameCanvas == null) return;
            _gameCanvas.Children.Clear();
            
            bool[,] board = gameBoard.GetBoard();
            string[,] colors = gameBoard.GetColors();
            
            for (int row = 0; row < GameBoard.Height; row++)
            {
                for (int col = 0; col < GameBoard.Width; col++)
                {
                    if (board[row, col])
                    {
                        // 创建方块
                        var rect = new Avalonia.Controls.Shapes.Rectangle
                        {
                            Width = BlockSize,
                            Height = BlockSize,
                            Fill = new SolidColorBrush(Color.Parse(colors[row, col])),
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };
                        
                        // 设置方块位置
                        Canvas.SetLeft(rect, col * BlockSize);
                        Canvas.SetTop(rect, row * BlockSize);
                        
                        // 添加到画布
                        _gameCanvas.Children.Add(rect);
                    }
                }
            }
            
            // 绘制网格线（可选）
            DrawGrid();
        }
        
        /// <summary>
        /// 绘制网格线
        /// </summary>
        private void DrawGrid()
        {
            if (_gameCanvas == null) return;

            // 绘制水平线
            for (int row = 0; row <= GameBoard.Height; row++)
            {
                var line = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = new Point(0, row * BlockSize),
                    EndPoint = new Point(GameBoard.Width * BlockSize, row * BlockSize),
                    Stroke = new SolidColorBrush(Color.Parse("#222")),
                    StrokeThickness = 1
                };
                
                _gameCanvas.Children.Add(line);
            }
            
            // 绘制垂直线
            for (int col = 0; col <= GameBoard.Width; col++)
            {
                var line = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = new Point(col * BlockSize, 0),
                    EndPoint = new Point(col * BlockSize, GameBoard.Height * BlockSize),
                    Stroke = new SolidColorBrush(Color.Parse("#222")),
                    StrokeThickness = 1
                };
                
                _gameCanvas.Children.Add(line);
            }
        }
        
        /// <summary>
        /// 绘制下一个方块
        /// </summary>
        private void DrawNextBlock(Block nextBlock)
        {
            if (_nextBlockCanvas == null) return;
            _nextBlockCanvas.Children.Clear();
            
            if (nextBlock == null) return;
            
            bool[,] shape = nextBlock.GetShape();
            int blockHeight = nextBlock.GetHeight();
            int blockWidth = nextBlock.GetWidth();
            
            // 计算居中位置
            double centerX = 0;
            double centerY = 0;
            if (_nextBlockCanvas != null)
            {
                centerX = (_nextBlockCanvas.Bounds.Width - blockWidth * NextBlockSize) / 2;
                centerY = (_nextBlockCanvas.Bounds.Height - blockHeight * NextBlockSize) / 2;
            }
            
            for (int row = 0; row < blockHeight; row++)
            {
                for (int col = 0; col < blockWidth; col++)
                {
                    if (shape[row, col])
                    {
                        // 创建方块
                        var rect = new Avalonia.Controls.Shapes.Rectangle
                        {
                            Width = NextBlockSize,
                            Height = NextBlockSize,
                            Fill = new SolidColorBrush(Color.Parse(nextBlock.Color ?? "#FFFFFF")), // Default to white if color is null
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };
                        
                        // 设置方块位置
                        Canvas.SetLeft(rect, centerX + col * NextBlockSize);
                        Canvas.SetTop(rect, centerY + row * NextBlockSize);
                        
                        // 添加到画布
                        if (_nextBlockCanvas != null)
                        {
                            _nextBlockCanvas.Children.Add(rect);
                        }
                    }
                }
            }
        }
    }
}