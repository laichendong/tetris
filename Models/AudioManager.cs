using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace TetrisGame.Models
{
    public class AudioManager : IDisposable
    {
        private Process? _audioProcess;
        private bool _isPlaying;
        private bool _disposed;
        
        // NAudio Windows平台音频管理
        private WaveOutEvent? _waveOut;
        private MixingSampleProvider? _mixer;
        private VolumeSampleProvider? _backgroundMusicProvider;

        public bool IsPlaying => _isPlaying;
        
        public AudioManager()
        {
            // 初始化NAudio音频系统 (仅Windows平台)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                InitializeNAudio();
            }
        }
        
        /// <summary>
        /// 初始化NAudio音频系统
        /// </summary>
        private void InitializeNAudio()
        {
            try
            {
                _waveOut = new WaveOutEvent();
                _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
                _waveOut.Init(_mixer);
            }
            catch
            {
                // 如果初始化失败，静默处理
                _waveOut = null;
                _mixer = null;
            }
        }
        
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayBackgroundMusic()
        {
            try
            {
                const string backgroundMusicResource = "avares://TetrisGame/bgm/bgm.wav";
                
                // 创建临时文件用于播放
                var tempFile = Path.GetTempFileName();
                tempFile = Path.ChangeExtension(tempFile, ".wav");
                
                using (var assets = AssetLoader.Open(new Uri(backgroundMusicResource)))
                using (var fileStream = File.Create(tempFile))
                {
                    assets.CopyTo(fileStream);
                }
                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS 使用 afplay，设置音量为30%
                    _audioProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"-v 0.3 \"{tempFile}\"", // 设置音量为30%
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows平台使用NAudio播放背景音乐
                    if (_mixer != null)
                    {
                        var audioFileReader = new AudioFileReader(tempFile);
                        var sampleProvider = audioFileReader.ToSampleProvider();
                        
                        // 创建循环播放提供器
                        var loopingSampleProvider = new LoopingSampleProvider(sampleProvider);
                        
                        // 设置背景音乐音量 (30%)
                        _backgroundMusicProvider = new VolumeSampleProvider(loopingSampleProvider)
                        {
                            Volume = 0.3f
                        };
                        
                        _mixer.AddMixerInput(_backgroundMusicProvider);
                            
                        // 确保WaveOut开始播放
                        if (_waveOut?.PlaybackState != PlaybackState.Playing)
                        {
                            _waveOut?.Play();
                        }
                    }
                }
                else
                {
                    // Linux 使用 paplay
                    _audioProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "paplay",
                        Arguments = $"\"{tempFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                
                _isPlaying = true;
            }
            catch
            {
                // 静默处理音频播放错误，不影响游戏运行
            }
        }

        public void PauseBackgroundMusic()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _waveOut?.Pause();
            }
            else
            {
                StopBackgroundMusic();
            }
        }

        public void ResumeBackgroundMusic()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _waveOut?.Play();
            }
            else
            {
                PlayBackgroundMusic();
            }
        }

        public void StopBackgroundMusic()
        {
            try
            {                
                // 停止Windows下的背景音乐播放
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (_backgroundMusicProvider != null)
                    {
                        _mixer?.RemoveMixerInput(_backgroundMusicProvider);
                        _backgroundMusicProvider = null;
                    }
                    
                    // 完全停止WaveOut设备以确保背景音乐停止
                    if (_waveOut != null)
                    {
                        _waveOut.Stop();
                        _waveOut.Dispose();
                        _waveOut = null;
                    }
                        
                    // 重新初始化音频系统以备后续使用
                    InitializeNAudio();
                }
                else
                {
                    // 停止其他平台的音频进程
                    _audioProcess?.Kill();
                    _audioProcess?.Dispose();
                    _audioProcess = null;
                }
                
                _isPlaying = false;
            }
            catch
            {
                // 静默处理停止错误
            }
        }

        /// <summary>
        /// 播放游戏结束音效
        /// </summary>
        public void PlayGameOverSound()
        {
            StopBackgroundMusic();
            PlaySoundEffect("avares://TetrisGame/bgm/gameover.wav", 1.0f);
        }

        /// <summary>
        /// 播放消行音效
        /// </summary>
        public void PlayLineClearSound()
        {
            PlaySoundEffect("avares://TetrisGame/bgm/clear.wav", 1.0f);   
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
        /// 播放方块下落音效
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
                // 创建临时文件用于播放
                var tempFile = Path.GetTempFileName();
                tempFile = Path.ChangeExtension(tempFile, ".wav");
                
                using (var assets = AssetLoader.Open(new Uri(soundPath)))
                using (var fileStream = File.Create(tempFile))
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
                        Arguments = $"-v {volume} \"{tempFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows平台使用NAudio播放音效
                    if (_mixer != null)
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                // 检查音频设备和混音器状态
                                if (_waveOut == null || _mixer == null)
                                {
                                    System.Diagnostics.Debug.WriteLine("NAudio未正确初始化，无法播放音效");
                                    return;
                                }
                                
                                var audioFileReader = new AudioFileReader(tempFile);
                                var sampleProvider = audioFileReader.ToSampleProvider();
                                
                                // 设置音效音量
                                var volumeProvider = new VolumeSampleProvider(sampleProvider)
                                {
                                    Volume = volume
                                };
                                
                                // 创建一次性播放提供器
                                OneShotSampleProvider oneShotProvider = null;
                                oneShotProvider = new OneShotSampleProvider(volumeProvider, () =>
                                {
                                    // 播放完成后自动移除
                                    try
                                    {
                                        _mixer?.RemoveMixerInput(oneShotProvider);
                                        audioFileReader.Dispose();
                                    }
                                    catch
                                    {
                                        // 静默处理移除错误
                                    }
                                });
                                
                                _mixer.AddMixerInput(oneShotProvider);
                                
                                // 确保WaveOut开始播放
                                if (_waveOut?.PlaybackState != PlaybackState.Playing)
                                {
                                    _waveOut?.Play();
                                }
                            }
                            catch (Exception ex)
                            {
                                // 输出详细的播放错误信息
                                System.Diagnostics.Debug.WriteLine($"Windows音效播放失败: {ex.Message}");
                            }
                        });
                    }
                }
                else
                {
                    // Linux 使用 paplay
                    soundProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "paplay",
                        Arguments = $"\"{tempFile}\"",
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
                    });
                }
            }
            catch
            {
                // 静默处理音频播放错误，不影响游戏运行
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
                        
            _audioProcess?.Dispose();
            _audioProcess = null;
            
            // 清理NAudio资源 (Windows平台)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    if (_backgroundMusicProvider != null)
                    {
                        _mixer?.RemoveMixerInput(_backgroundMusicProvider);
                        _backgroundMusicProvider = null;
                    }
                    
                    _waveOut?.Stop();
                    _waveOut?.Dispose();
                    _waveOut = null;
                    _mixer = null;
                }
                catch
                {
                    // 静默处理清理错误
                }
            }
        }
    }
}