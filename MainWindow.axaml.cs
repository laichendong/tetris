using System;
using Avalonia.Controls;
using TetrisGame.Views;
using TetrisGame.Controllers;

namespace TetrisGame;

public partial class MainWindow : Window
{
    private MenuView? _menuView;
    private GameView? _gameView;
    private GameController? _gameController;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // 获取控件引用
        _menuView = this.FindControl<MenuView>("MenuView");
        _gameView = this.FindControl<GameView>("GameView");
        
        // 绑定菜单事件
        if (_menuView != null)
        {
            _menuView.DifficultySelected += OnDifficultySelected;
            
            // 为菜单设置游戏控制器引用（用于播放音效）
            if (_gameView != null)
            {
                var gameController = _gameView.GetGameController();
                if (gameController != null)
                {
                    _menuView.SetGameController(gameController);
                }
            }
        }
    }
    
    private void OnDifficultySelected(object? sender, GameDifficulty difficulty)
    {
        // 隐藏菜单，显示游戏
        if (_menuView != null) _menuView.IsVisible = false;
        if (_gameView != null) 
        {
            _gameView.IsVisible = true;
            
            // 获取游戏控制器并设置难度
            _gameController = _gameView.GetGameController();
            if (_gameController != null)
            {
                _gameController.SetDifficulty(difficulty);
                _gameController.StartNewGame();
                
                // 订阅返回菜单事件
                _gameController.ReturnToMenu += OnReturnToMenu;
            }
            
            // 确保游戏视图获得键盘焦点
            _gameView.Focus();
        }
    }
    
    /// <summary>
    /// 处理返回菜单事件
    /// </summary>
    private void OnReturnToMenu(object? sender, EventArgs e)
    {
        // 获取游戏分数并添加到排行榜
        if (_gameController != null && _menuView != null)
        {
            var gameBoard = _gameController.GetGameBoard();
            if (gameBoard != null && gameBoard.Score > 0)
            {
                // 添加分数到排行榜
                bool isTopThree = _menuView.AddScore(gameBoard.Score, "玩家", _gameController.GetDifficulty());
                
                // 如果进入前三名，可以在这里添加祝贺消息
                if (isTopThree)
                {
                    // 可以添加祝贺对话框或其他提示
                }
            }
            
            _gameController.StopGame();
            _gameController.ReturnToMenu -= OnReturnToMenu; // 取消订阅
            _gameController.Dispose(); // 释放资源
        }
        
        // 隐藏游戏视图，显示菜单
        if (_gameView != null) _gameView.IsVisible = false;
        if (_menuView != null) 
        {
            _menuView.IsVisible = true;
            _menuView.UpdateLeaderboard(); // 更新排行榜显示
        }
    }
    
    /// <summary>
    /// 返回菜单
    /// </summary>
    public void ShowMenu()
    {
        if (_menuView != null) _menuView.IsVisible = true;
        if (_gameView != null) _gameView.IsVisible = false;
    }
    
    /// <summary>
    /// 窗口关闭时释放资源
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _gameController?.Dispose();
        base.OnClosed(e);
    }
}