using System;
using NAudio.Wave;

namespace TetrisGame.Models
{
    /// <summary>
    /// 一次性播放的音频样本提供器
    /// </summary>
    public class OneShotSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly Action? _onComplete;
        private bool _hasCompleted = false;

        public OneShotSampleProvider(ISampleProvider source, Action? onComplete = null)
        {
            _source = source;
            _onComplete = onComplete;
        }

        public WaveFormat WaveFormat => _source.WaveFormat;

        public bool HasCompleted => _hasCompleted;

        public int Read(float[] buffer, int offset, int count)
        {
            if (_hasCompleted)
            {
                return 0;
            }

            int samplesRead = _source.Read(buffer, offset, count);
            
            if (samplesRead == 0)
            {
                // 播放完成
                _hasCompleted = true;
                _onComplete?.Invoke();
            }
            
            return samplesRead;
        }
    }
}