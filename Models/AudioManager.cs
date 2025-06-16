using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform;
using System.Media;
using System.Threading;
using System.Reflection;
using System.Collections.Concurrent;

namespace TetrisGame.Models
{
    public class AudioManager : IDisposable
    {
        private Process? _audioProcess;
        private bool _isPlaying;
        private bool _disposed;
        private CancellationTokenSource? _backgroundMusicCancellation; // 用于取消背景音乐播放
        private SoundPlayer? _backgroundMusicPlayer; // Windows下的背景音乐播放器
        
        // 预加载的音频文件临时路径
        private readonly Dictionary<string, string> _preloadedAudioFiles = new();
        
        // Windows平台音效播放器池
        private readonly ConcurrentDictionary<string, SoundPlayer> _soundPlayerPool = new ConcurrentDictionary<string, SoundPlayer>();
        
        private readonly string[] _audioResources = {
            "avares://TetrisGame/bgm/bgm.wav",
            "avares://TetrisGame/bgm/clear.wav",
            "avares://TetrisGame/bgm/gameover.wav",
            "avares://TetrisGame/bgm/click.wav",
            "avares://TetrisGame/bgm/move.wav",
            "avares://TetrisGame/bgm/rotate.wav",
            "avares://TetrisGame/bgm/fall.wav"
        };

        public bool IsPlaying => _isPlaying;
        
        public AudioManager()
        {
            // 预加载所有音频文件
            PreloadAllAudioFiles();
        }
        
        /// <summary>
        /// 预加载所有音频文件到临时文件，减少播放时的延迟
        /// </summary>
        private void PreloadAllAudioFiles()
        {
            foreach (var audioResource in _audioResources)
            {
                try
                {
                    // 从嵌入资源加载音频文件
                    var assets = AssetLoader.Open(new Uri(audioResource));
                    
                    // 创建临时文件
                    var tempFile = Path.GetTempFileName();
                    tempFile = Path.ChangeExtension(tempFile, ".wav");
                    
                    using (var fileStream = File.Create(tempFile))
                    {
                        assets.CopyTo(fileStream);
                    }
                    
                    // 存储预加载的文件路径
                    _preloadedAudioFiles[audioResource] = tempFile;
                    
                    // Windows平台预加载SoundPlayer对象
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        try
                        {
                            var soundPlayer = new SoundPlayer(tempFile);
                            soundPlayer.LoadAsync();
                            _soundPlayerPool[audioResource] = soundPlayer;
                            Console.WriteLine($"预加载Windows音效播放器成功: {Path.GetFileName(audioResource)}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"预加载Windows音效播放器失败 {audioResource}: {ex.Message}");
                        }
                    }
                    
                    Console.WriteLine($"预加载音频文件成功: {Path.GetFileName(audioResource)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"预加载音频文件失败 {audioResource}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 获取预加载的音频文件路径
        /// </summary>
        /// <param name="resourcePath">资源路径</param>
        /// <returns>临时文件路径，如果未找到则返回null</returns>
        private string? GetPreloadedAudioFile(string resourcePath)
        {
            return _preloadedAudioFiles.TryGetValue(resourcePath, out var tempFile) ? tempFile : null;
        }

        public void PlayBackgroundMusic()
        {
            try
            {
                if (_isPlaying) return;
                
                // 停止当前播放的音频
                StopBackgroundMusic();
                
                // 使用预加载的背景音乐文件
                var bgmFile = GetPreloadedAudioFile("avares://TetrisGame/bgm/bgm.wav");
                if (string.IsNullOrEmpty(bgmFile) || !File.Exists(bgmFile))
                {
                    Console.WriteLine("背景音乐文件未预加载或不存在");
                    return;
                }
                
                // 根据操作系统选择播放器
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS 使用 afplay，添加音量控制参数
                    _audioProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"-v 0.3 \"{bgmFile}\"", // 设置音量为30%
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows 使用 SoundPlayer 播放音频
                    _backgroundMusicCancellation = new CancellationTokenSource();
                    var cancellationToken = _backgroundMusicCancellation.Token;
                    
                    _backgroundMusicPlayer = new SoundPlayer(bgmFile);
                    _backgroundMusicPlayer.LoadAsync();
                    
                    // 使用PlayLooping进行循环播放，这样可以更好地控制停止
                    Task.Run(() =>
                    {
                        try
                        {
                            // 等待加载完成
                            Thread.Sleep(500);
                            
                            if (!cancellationToken.IsCancellationRequested && _isPlaying && !_disposed)
                            {
                                _backgroundMusicPlayer.PlayLooping();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Windows音频播放错误: {ex.Message}");
                        }
                    }, cancellationToken);
                    
                    // 创建一个虚拟进程对象以保持兼容性
                    _audioProcess = new Process();
                }
                else
                {
                    // Linux 使用 paplay
                    _audioProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "paplay",
                        Arguments = $"\"{bgmFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                
                _isPlaying = true;
                
                // 异步等待播放完成
                if (_audioProcess != null)
                {
                    Task.Run(() =>
                    {
                        _audioProcess.WaitForExit();
                        _isPlaying = false;
                        // 不再清理临时文件，保留复用
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"播放背景音乐时出错: {ex.Message}");
                _isPlaying = false;
            }
        }

        public void PauseBackgroundMusic()
        {
            // 简单实现：停止当前播放
            StopBackgroundMusic();
        }

        public void ResumeBackgroundMusic()
        {
            // 简单实现：重新开始播放
            PlayBackgroundMusic();
        }

        public void StopBackgroundMusic()
        {
            try
            {
                _isPlaying = false;
                
                // 取消Windows下的背景音乐播放
                if (_backgroundMusicCancellation != null)
                {
                    _backgroundMusicCancellation.Cancel();
                    _backgroundMusicCancellation.Dispose();
                    _backgroundMusicCancellation = null;
                }
                
                // 停止Windows下的SoundPlayer
                if (_backgroundMusicPlayer != null)
                {
                    _backgroundMusicPlayer.Stop();
                    _backgroundMusicPlayer.Dispose();
                    _backgroundMusicPlayer = null;
                }
                
                // 停止其他平台的音频进程
                if (_audioProcess != null && !_audioProcess.HasExited)
                {
                    _audioProcess.Kill();
                    _audioProcess.WaitForExit(1000); // 等待最多1秒
                }
                _audioProcess?.Dispose();
                _audioProcess = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"停止背景音乐时出错: {ex.Message}");
            }
            finally
            {
                _isPlaying = false;
                // 不再清理临时文件，保留复用
            }
        }

        public void PlayGameOverSound()
        {
            try
            {
                // 停止背景音乐
                StopBackgroundMusic();
                
                // 使用预加载的游戏结束音效文件
                var gameOverFile = GetPreloadedAudioFile("avares://TetrisGame/bgm/gameover.wav");
                if (string.IsNullOrEmpty(gameOverFile) || !File.Exists(gameOverFile))
                {
                    Console.WriteLine("游戏结束音效文件未预加载或不存在");
                    return;
                }
                
                // 根据操作系统选择播放器
                Process? gameOverProcess = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS 使用 afplay
                    gameOverProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"\"{gameOverFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows 使用预加载的 SoundPlayer 播放音效
                    if (_soundPlayerPool.TryGetValue("avares://TetrisGame/bgm/gameover.wav", out var soundPlayer))
                    {
                        try
                        {
                            // 使用异步播放，不阻塞背景音乐
                            soundPlayer.Play();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Windows游戏结束音效播放错误: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("未找到预加载的游戏结束音效播放器");
                    }
                    
                    // 创建一个虚拟进程对象以保持兼容性
                    gameOverProcess = new Process();
                }
                else
                {
                    // Linux 使用 paplay
                    gameOverProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "paplay",
                        Arguments = $"\"{gameOverFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                
                // 异步等待播放完成，不再清理临时文件
                if (gameOverProcess != null)
                {
                    Task.Run(() =>
                    {
                        gameOverProcess.WaitForExit();
                        gameOverProcess.Dispose();
                        // 不再清理临时文件，保留复用
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"播放游戏结束音效时出错: {ex.Message}");
            }
        }

        public void PlayLineClearSound()
        {
            try
            {
                // 使用预加载的消行音效文件
                var clearSoundFile = GetPreloadedAudioFile("avares://TetrisGame/bgm/clear.wav");
                if (string.IsNullOrEmpty(clearSoundFile) || !File.Exists(clearSoundFile))
                {
                    Console.WriteLine("消行音效文件未预加载或不存在");
                    return;
                }
                
                // 根据操作系统选择播放器
                Process? clearSoundProcess = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS 使用 afplay，设置音量为100%
                    clearSoundProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"-v 1.0 \"{clearSoundFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows 使用预加载的 SoundPlayer 播放音效
                    if (_soundPlayerPool.TryGetValue("avares://TetrisGame/bgm/clear.wav", out var soundPlayer))
                    {
                        try
                        {
                            // 使用异步播放，不阻塞背景音乐
                            soundPlayer.Play();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Windows消行音效播放错误: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("未找到预加载的消行音效播放器");
                    }
                    
                    // 创建一个虚拟进程对象以保持兼容性
                    clearSoundProcess = new Process();
                }
                else
                {
                    // Linux 使用 paplay
                    clearSoundProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "paplay",
                        Arguments = $"\"{clearSoundFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                
                // 异步等待播放完成，不再清理临时文件
                if (clearSoundProcess != null)
                {
                    Task.Run(() =>
                    {
                        clearSoundProcess.WaitForExit();
                        clearSoundProcess.Dispose();
                        // 不再清理临时文件，保留复用
                    });
                }
            }
            catch (Exception ex)
            {
                // 静默处理音频播放错误，不影响游戏运行
                Console.WriteLine($"消除行音效播放错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 播放鼠标划过难度选择按钮音效
        /// </summary>
        public void PlayClickSound()
        {
            PlaySoundEffect("avares://TetrisGame/bgm/click.wav", 1.0f);
        }
        
        /// <summary>
        /// 播放方块左右移动音效
        /// </summary>
        public void PlayMoveSound()
        {
            PlaySoundEffect("avares://TetrisGame/bgm/move.wav", 1.0f);
        }
        
        /// <summary>
        /// 播放方块旋转音效
        /// </summary>
        public void PlayRotateSound()
        {
            PlaySoundEffect("avares://TetrisGame/bgm/rotate.wav", 1.0f);
        }
        
        /// <summary>
        /// 播放空格键方块下落音效
        /// </summary>
        public void PlayFallSound()
        {
            PlaySoundEffect("avares://TetrisGame/bgm/fall.wav", 1.0f);
        }
        
        /// <summary>
        /// 通用音效播放方法
        /// </summary>
        /// <param name="soundPath">音效文件路径</param>
        /// <param name="volume">音量（0.0-1.0）</param>
        private void PlaySoundEffect(string soundPath, float volume)
        {
            try
            {
                // 使用预加载的音效文件
                var soundFile = GetPreloadedAudioFile(soundPath);
                if (string.IsNullOrEmpty(soundFile) || !File.Exists(soundFile))
                {
                    Console.WriteLine($"音效文件未预加载或不存在: {soundPath}");
                    return;
                }
                
                // 根据操作系统选择播放器
                Process? soundProcess = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS 使用 afplay，设置音量
                    soundProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"-v {volume} \"{soundFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows 使用预加载的 SoundPlayer 播放音效
                    if (_soundPlayerPool.TryGetValue(soundPath, out var soundPlayer))
                    {
                        try
                        {
                            // 使用异步播放，不阻塞背景音乐
                            soundPlayer.Play();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Windows音效播放错误: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"未找到预加载的音效播放器: {soundPath}");
                    }
                    
                    // 创建一个虚拟进程对象以保持兼容性
                    soundProcess = new Process();
                }
                else
                {
                    // Linux 使用 paplay
                    soundProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "paplay",
                        Arguments = $"\"{soundFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                
                // 异步等待播放完成，不再清理临时文件
                if (soundProcess != null)
                {
                    Task.Run(() =>
                    {
                        soundProcess.WaitForExit();
                        soundProcess.Dispose();
                        // 不再清理临时文件，保留复用
                    });
                }
            }
            catch (Exception ex)
            {
                // 静默处理音频播放错误，不影响游戏运行
                Console.WriteLine($"音效播放错误: {ex.Message}");
            }
        }

        public void SetVolume(float volume)
        {
            // 音量控制的实现留空，因为我们在播放时直接设置音量
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopBackgroundMusic();
            
            // 清理Windows下的背景音乐资源
            if (_backgroundMusicCancellation != null)
            {
                _backgroundMusicCancellation.Cancel();
                _backgroundMusicCancellation.Dispose();
                _backgroundMusicCancellation = null;
            }
            
            if (_backgroundMusicPlayer != null)
            {
                _backgroundMusicPlayer.Stop();
                _backgroundMusicPlayer.Dispose();
                _backgroundMusicPlayer = null;
            }
            
            _audioProcess?.Dispose();
            _audioProcess = null;
            
            // 清理Windows平台的SoundPlayer池
            foreach (var soundPlayer in _soundPlayerPool.Values)
            {
                try
                {
                    soundPlayer?.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"清理SoundPlayer失败: {ex.Message}");
                }
            }
            _soundPlayerPool.Clear();
            
            // 统一清理所有预加载的临时音频文件
            foreach (var tempFile in _preloadedAudioFiles.Values)
            {
                if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile))
                {
                    try
                    {
                        File.Delete(tempFile);
                        Console.WriteLine($"清理临时音频文件: {Path.GetFileName(tempFile)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"清理临时音频文件失败 {tempFile}: {ex.Message}");
                    }
                }
            }
            
            _preloadedAudioFiles.Clear();
        }
    }
}