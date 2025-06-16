using NAudio.Wave;

namespace TetrisGame.Models
{
    /// <summary>
    /// 循环播放的音频样本提供器
    /// </summary>
    public class LoopingSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private bool _isLooping = true;

        public LoopingSampleProvider(ISampleProvider source)
        {
            _source = source;
        }

        public WaveFormat WaveFormat => _source.WaveFormat;

        public bool IsLooping
        {
            get => _isLooping;
            set => _isLooping = value;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int totalSamplesRead = 0;
            
            while (totalSamplesRead < count && _isLooping)
            {
                int samplesRead = _source.Read(buffer, offset + totalSamplesRead, count - totalSamplesRead);
                
                if (samplesRead == 0)
                {
                    // 到达文件末尾，重新开始
                    if (_source is AudioFileReader audioFileReader)
                    {
                        audioFileReader.Position = 0;
                    }
                    else
                    {
                        // 如果不是AudioFileReader，停止循环
                        break;
                    }
                }
                else
                {
                    totalSamplesRead += samplesRead;
                }
            }
            
            return totalSamplesRead;
        }
        
        public void Stop()
        {
            _isLooping = false;
        }
    }
}