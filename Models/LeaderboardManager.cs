using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TetrisGame.Views;

namespace TetrisGame.Models
{
    /// <summary>
    /// 排行榜管理器，负责保存和加载历史最高分数
    /// </summary>
    public class LeaderboardManager
    {
        private const string LeaderboardFileName = "leaderboard.json";
        private const int MaxScores = 3; // 保存前三名
        
        private List<ScoreEntry> _scores;
        
        public LeaderboardManager()
        {
            _scores = new List<ScoreEntry>();
            LoadScores();
        }
        
        /// <summary>
        /// 获取前三名分数
        /// </summary>
        public List<ScoreEntry> GetTopScores()
        {
            return _scores.Take(MaxScores).ToList();
        }
        
        /// <summary>
        /// 添加新分数
        /// </summary>
        /// <param name="score">分数</param>
        /// <param name="playerName">玩家名称</param>
        /// <param name="difficulty">游戏难度</param>
        /// <returns>是否进入前三名</returns>
        public bool AddScore(int score, string playerName, GameDifficulty difficulty)
        {
            var newEntry = new ScoreEntry
            {
                Score = score,
                PlayerName = playerName,
                Difficulty = difficulty,
                Date = DateTime.Now
            };
            
            _scores.Add(newEntry);
            _scores = _scores.OrderByDescending(s => s.Score).ToList();
            
            // 只保留前三名
            bool isTopThree = _scores.IndexOf(newEntry) < MaxScores;
            if (_scores.Count > MaxScores)
            {
                _scores = _scores.Take(MaxScores).ToList();
            }
            
            SaveScores();
            return isTopThree;
        }
        
        /// <summary>
        /// 从文件加载分数
        /// </summary>
        private void LoadScores()
        {
            try
            {
                if (File.Exists(LeaderboardFileName))
                {
                    string json = File.ReadAllText(LeaderboardFileName);
                    var scores = JsonSerializer.Deserialize<List<ScoreEntry>>(json);
                    if (scores != null)
                    {
                        _scores = scores.OrderByDescending(s => s.Score).Take(MaxScores).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果加载失败，使用空列表
                Console.WriteLine($"加载排行榜失败: {ex.Message}");
                _scores = new List<ScoreEntry>();
            }
        }
        
        /// <summary>
        /// 保存分数到文件
        /// </summary>
        private void SaveScores()
        {
            try
            {
                string json = JsonSerializer.Serialize(_scores, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(LeaderboardFileName, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存排行榜失败: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// 分数记录条目
    /// </summary>
    public class ScoreEntry
    {
        public int Score { get; set; }
        public string PlayerName { get; set; } = "匿名玩家";
        public GameDifficulty Difficulty { get; set; }
        public DateTime Date { get; set; }
        
        /// <summary>
        /// 获取难度显示文本
        /// </summary>
        public string DifficultyText => Difficulty switch
        {
            GameDifficulty.Easy => "简单",
            GameDifficulty.Medium => "中等",
            GameDifficulty.Hard => "困难",
            GameDifficulty.Expert => "专家",
            _ => "未知"
        };
    }
}