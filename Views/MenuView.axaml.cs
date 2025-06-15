using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TetrisGame.Controllers;
using TetrisGame.Models;

namespace TetrisGame.Views
{
    public partial class MenuView : UserControl
    {
        // 难度选择事件
        public event EventHandler<GameDifficulty>? DifficultySelected;
        
        // 排行榜管理器
        private readonly LeaderboardManager _leaderboardManager;
        
        // 游戏控制器引用（用于播放音效）
        private GameController? _gameController;
        
        // 排行榜显示控件
        private TextBlock? _firstPlaceScore;
        private TextBlock? _firstPlaceInfo;
        private TextBlock? _secondPlaceScore;
        private TextBlock? _secondPlaceInfo;
        private TextBlock? _thirdPlaceScore;
        private TextBlock? _thirdPlaceInfo;
        
        public MenuView()
        {
            AvaloniaXamlLoader.Load(this);
            
            _leaderboardManager = new LeaderboardManager();
            
            // 获取按钮并绑定事件
            var easyButton = this.FindControl<Button>("EasyButton");
            var mediumButton = this.FindControl<Button>("MediumButton");
            var hardButton = this.FindControl<Button>("HardButton");
            var expertButton = this.FindControl<Button>("ExpertButton");
            
            if (easyButton != null) 
            {
                easyButton.Click += (s, e) => OnDifficultySelected(GameDifficulty.Easy);
                easyButton.PointerEntered += OnButtonPointerEntered;
            }
            if (mediumButton != null) 
            {
                mediumButton.Click += (s, e) => OnDifficultySelected(GameDifficulty.Medium);
                mediumButton.PointerEntered += OnButtonPointerEntered;
            }
            if (hardButton != null) 
            {
                hardButton.Click += (s, e) => OnDifficultySelected(GameDifficulty.Hard);
                hardButton.PointerEntered += OnButtonPointerEntered;
            }
            if (expertButton != null) 
            {
                expertButton.Click += (s, e) => OnDifficultySelected(GameDifficulty.Expert);
                expertButton.PointerEntered += OnButtonPointerEntered;
            }
            
            // 获取排行榜显示控件
            _firstPlaceScore = this.FindControl<TextBlock>("FirstPlaceScore");
            _firstPlaceInfo = this.FindControl<TextBlock>("FirstPlaceInfo");
            _secondPlaceScore = this.FindControl<TextBlock>("SecondPlaceScore");
            _secondPlaceInfo = this.FindControl<TextBlock>("SecondPlaceInfo");
            _thirdPlaceScore = this.FindControl<TextBlock>("ThirdPlaceScore");
            _thirdPlaceInfo = this.FindControl<TextBlock>("ThirdPlaceInfo");
            
            // 更新排行榜显示
            UpdateLeaderboard();
        }
        
        private void OnDifficultySelected(GameDifficulty difficulty)
        {
            DifficultySelected?.Invoke(this, difficulty);
        }
        
        /// <summary>
        /// 鼠标进入按钮时播放音效
        /// </summary>
        private void OnButtonPointerEntered(object? sender, PointerEventArgs e)
        {
            _gameController?.PlayClickSound();
        }
        
        /// <summary>
        /// 设置游戏控制器引用
        /// </summary>
        public void SetGameController(GameController gameController)
        {
            _gameController = gameController;
        }
        
        /// <summary>
        /// 更新排行榜显示
        /// </summary>
        public void UpdateLeaderboard()
        {
            var topScores = _leaderboardManager.GetTopScores();
            
            // 更新第一名
            if (topScores.Count > 0 && _firstPlaceScore != null && _firstPlaceInfo != null)
            {
                _firstPlaceScore.Text = topScores[0].Score.ToString();
                _firstPlaceInfo.Text = $"{topScores[0].PlayerName} - {topScores[0].DifficultyText}";
            }
            else if (_firstPlaceScore != null && _firstPlaceInfo != null)
            {
                _firstPlaceScore.Text = "暂无记录";
                _firstPlaceInfo.Text = "";
            }
            
            // 更新第二名
            if (topScores.Count > 1 && _secondPlaceScore != null && _secondPlaceInfo != null)
            {
                _secondPlaceScore.Text = topScores[1].Score.ToString();
                _secondPlaceInfo.Text = $"{topScores[1].PlayerName} - {topScores[1].DifficultyText}";
            }
            else if (_secondPlaceScore != null && _secondPlaceInfo != null)
            {
                _secondPlaceScore.Text = "暂无记录";
                _secondPlaceInfo.Text = "";
            }
            
            // 更新第三名
            if (topScores.Count > 2 && _thirdPlaceScore != null && _thirdPlaceInfo != null)
            {
                _thirdPlaceScore.Text = topScores[2].Score.ToString();
                _thirdPlaceInfo.Text = $"{topScores[2].PlayerName} - {topScores[2].DifficultyText}";
            }
            else if (_thirdPlaceScore != null && _thirdPlaceInfo != null)
            {
                _thirdPlaceScore.Text = "暂无记录";
                _thirdPlaceInfo.Text = "";
            }
        }
        
        /// <summary>
        /// 添加新分数到排行榜
        /// </summary>
        /// <param name="score">分数</param>
        /// <param name="playerName">玩家名称</param>
        /// <param name="difficulty">游戏难度</param>
        /// <returns>是否进入前三名</returns>
        public bool AddScore(int score, string playerName, Views.GameDifficulty difficulty)
        {
            bool isTopThree = _leaderboardManager.AddScore(score, playerName, difficulty);
            UpdateLeaderboard();
            return isTopThree;
        }
    }
    
    /// <summary>
    /// 游戏难度枚举
    /// </summary>
    public enum GameDifficulty
    {
        Easy,    // 简单
        Medium,  // 中等
        Hard,    // 困难
        Expert   // 专家
    }
}