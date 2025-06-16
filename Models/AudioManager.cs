using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform;
using System.Media;
using System.Threading;

namespace TetrisGame.Models
{
    public class AudioManager : IDisposable
    {
        private Process? _audioProcess;
        private string? _tempAudioFile;
        private string? _tempClearSoundFile; // 预加载的消行音效临时文件
        private bool _isPlaying;
        private bool _disposed;
        private CancellationTokenSource? _backgroundMusicCancellation; // 用于取消背景音乐播放
        private SoundPlayer? _backgroundMusicPlayer; // Windows下的背景音乐播放器

        public bool IsPlaying => _isPlaying;
        
        public AudioManager()
        {
            // 预加载消行音效文件以减少播放延迟
            PreloadLineClearSound();
        }
        
        private void PreloadLineClearSound()
        {
            try
            {
                // 从嵌入资源加载消除行音效文件
                var assets = AssetLoader.Open(new Uri("avares://TetrisGame/bgm/clear.wav"));
                
                // 创建临时文件
                _tempClearSoundFile = Path.GetTempFileName();
                _tempClearSoundFile = Path.ChangeExtension(_tempClearSoundFile, ".wav");
                
                using (var fileStream = File.Create(_tempClearSoundFile))
                {
                    assets.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"预加载消行音效失败: {ex.Message}");
            }
        }

        public void PlayBackgroundMusic()
        {
            try
            {
                if (_isPlaying) return;
                
                // 停止当前播放的音频
                StopBackgroundMusic();
                
                // 从嵌入资源加载音频文件
                var assets = AssetLoader.Open(new Uri("avares://TetrisGame/bgm/bgm.wav"));
                
                // 创建临时文件
                _tempAudioFile = Path.GetTempFileName();
                _tempAudioFile = Path.ChangeExtension(_tempAudioFile, ".wav");
                
                using (var fileStream = File.Create(_tempAudioFile))
                {
                    assets.CopyTo(fileStream);
                }
                
                // 根据操作系统选择播放器
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS 使用 afplay，添加音量控制参数
                    _audioProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"-v 0.3 \"{_tempAudioFile}\"", // 设置音量为30%
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows 使用 SoundPlayer 播放音频
                    _backgroundMusicCancellation = new CancellationTokenSource();
                    var cancellationToken = _backgroundMusicCancellation.Token;
                    
                    _backgroundMusicPlayer = new SoundPlayer(_tempAudioFile);
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
                        Arguments = $"\"{_tempAudioFile}\"",
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
                        
                        // 清理临时文件
                        if (!string.IsNullOrEmpty(_tempAudioFile) && File.Exists(_tempAudioFile))
                        {
                            try
                            {
                                File.Delete(_tempAudioFile);
                            }
                            catch
                            {
                                // 忽略删除临时文件的错误
                            }
                        }
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
                
                // 清理临时文件
                if (!string.IsNullOrEmpty(_tempAudioFile) && File.Exists(_tempAudioFile))
                {
                    try
                    {
                        File.Delete(_tempAudioFile);
                        _tempAudioFile = null;
                    }
                    catch
                    {
                        // 忽略删除临时文件的错误
                    }
                }
            }
        }

        public void PlayGameOverSound()
        {
            try
            {
                // 停止背景音乐
                StopBackgroundMusic();
                
                // 从嵌入资源加载游戏结束音效文件
                var assets = AssetLoader.Open(new Uri("avares://TetrisGame/bgm/gameover.wav"));
                
                // 创建临时文件
                var tempGameOverFile = Path.GetTempFileName();
                tempGameOverFile = Path.ChangeExtension(tempGameOverFile, ".wav");
                
                using (var fileStream = File.Create(tempGameOverFile))
                {
                    assets.CopyTo(fileStream);
                }
                
                // 根据操作系统选择播放器
                Process? gameOverProcess = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS 使用 afplay
                    gameOverProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"\"{tempGameOverFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Windows 使用 SoundPlayer 播放音效，使用异步播放减少延迟
                        Task.Run(async () =>
                        {
                            try
                            {
                                using var soundPlayer = new SoundPlayer(tempGameOverFile);
                                // 使用异步播放减少延迟
                                soundPlayer.Play();
                                
                                // 等待音效播放完成（估算时间）
                                await Task.Delay(2000); // 游戏结束音效较长
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Windows游戏结束音效播放错误: {ex.Message}");
                            }
                        });
                        
                        // 创建一个虚拟进程对象以保持兼容性
                        gameOverProcess = new Process();
                    }
                else
                {
                    // Linux 使用 paplay
                    gameOverProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "paplay",
                        Arguments = $"\"{tempGameOverFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                
                // 异步等待播放完成并清理临时文件
                if (gameOverProcess != null)
                {
                    Task.Run(() =>
                    {
                        gameOverProcess.WaitForExit();
                        gameOverProcess.Dispose();
                        
                        // 清理临时文件
                        if (File.Exists(tempGameOverFile))
                        {
                            try
                            {
                                File.Delete(tempGameOverFile);
                            }
                            catch
                            {
                                // 忽略删除临时文件的错误
                            }
                        }
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
                if (string.IsNullOrEmpty(_tempClearSoundFile) || !File.Exists(_tempClearSoundFile))
                {
                    // 如果预加载失败，尝试重新加载
                    PreloadLineClearSound();
                }
                
                if (!string.IsNullOrEmpty(_tempClearSoundFile))
                {
                    // 根据操作系统选择播放器
                    Process? clearSoundProcess = null;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        // macOS 使用 afplay，设置音量为50%
                        clearSoundProcess = Process.Start(new ProcessStartInfo
                        {
                            FileName = "afplay",
                            Arguments = $"-v 1.0 \"{_tempClearSoundFile}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Windows 使用 SoundPlayer 播放音效，使用异步播放减少延迟
                        Task.Run(async () =>
                        {
                            try
                            {
                                using var soundPlayer = new SoundPlayer(_tempClearSoundFile);
                                // 使用异步播放减少延迟
                                soundPlayer.Play();
                                
                                // 等待音效播放完成（估算时间）
                                await Task.Delay(800); // 消行音效时长
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Windows消行音效播放错误: {ex.Message}");
                            }
                        });
                        
                        // 创建一个虚拟进程对象以保持兼容性
                        clearSoundProcess = new Process();
                    }
                    else
                    {
                        // Linux 使用 paplay
                        clearSoundProcess = Process.Start(new ProcessStartInfo
                        {
                            FileName = "paplay",
                            Arguments = $"\"{_tempClearSoundFile}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        });
                    }
                    
                    // 异步等待播放完成
                    if (clearSoundProcess != null)
                    {
                        Task.Run(() =>
                        {
                            clearSoundProcess.WaitForExit();
                            clearSoundProcess.Dispose();
                        });
                    }
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
                // 从嵌入资源加载音效文件
                var assets = AssetLoader.Open(new Uri(soundPath));
                
                // 创建临时文件
                var tempSoundFile = Path.GetTempFileName();
                tempSoundFile = Path.ChangeExtension(tempSoundFile, ".wav");
                
                using (var fileStream = File.Create(tempSoundFile))
                {
                    assets.CopyTo(fileStream);
                }
                
                // 根据操作系统选择播放器
                Process? soundProcess = null;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS 使用 afplay，设置音量
                    soundProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"-v {volume} \"{tempSoundFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows 使用 SoundPlayer 播放音效
                    Task.Run(() =>
                    {
                        try
                        {
                            using var soundPlayer = new SoundPlayer(tempSoundFile);
                            soundPlayer.LoadAsync();
                            
                            // 等待加载完成
                            Thread.Sleep(200);
                            
                            // 使用同步播放确保音效完整播放
                            soundPlayer.PlaySync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Windows音效播放错误: {ex.Message}");
                        }
                        finally
                        {
                            // 清理临时文件
                            if (File.Exists(tempSoundFile))
                            {
                                try
                                {
                                    File.Delete(tempSoundFile);
                                }
                                catch
                                {
                                    // 忽略删除临时文件的错误
                                }
                            }
                        }
                    });
                    
                    // 创建一个虚拟进程对象以保持兼容性
                    soundProcess = new Process();
                }
                else
                {
                    // Linux 使用 paplay
                    soundProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "paplay",
                        Arguments = $"\"{tempSoundFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                
                // 异步等待播放完成并清理临时文件
                if (soundProcess != null)
                {
                    Task.Run(() =>
                    {
                        soundProcess.WaitForExit();
                        soundProcess.Dispose();
                        
                        // 清理临时文件
                        if (File.Exists(tempSoundFile))
                        {
                            try
                            {
                                File.Delete(tempSoundFile);
                            }
                            catch
                            {
                                // 忽略删除临时文件的错误
                            }
                        }
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
            
            // 清理背景音乐临时文件
            if (!string.IsNullOrEmpty(_tempAudioFile) && File.Exists(_tempAudioFile))
            {
                try
                {
                    File.Delete(_tempAudioFile);
                }
                catch
                {
                    // 忽略删除临时文件的错误
                }
            }
            
            // 清理消行音效临时文件
            if (!string.IsNullOrEmpty(_tempClearSoundFile) && File.Exists(_tempClearSoundFile))
            {
                try
                {
                    File.Delete(_tempClearSoundFile);
                }
                catch
                {
                    // 忽略删除临时文件的错误
                }
            }
        }
    }
}