using System;
using System.Collections.Generic;

namespace KSynthesizer.Sources
{
    /// <summary>
    /// 周期関数ソース用のベースクラス
    /// 1周期分のオーディオデータを保管するため、周波数を変更しても1周期が終わるまでは前の周波数を提供する
    /// </summary>
    public abstract class PeriodicSourceBase : FunctionalAudioSourceBase, IAudioPeriodicSource
    {
        /// <summary>
        /// Period in millisecond
        /// </summary>
        public virtual float Period { get; set; } = 1000;

        /// <summary>
        /// Next(size)で消費されなかった1周期の残バッファ
        /// </summary>
        private Queue<float> LeftOvers { get; } = new Queue<float>();

        public void SetFrequency(float frequency)
        {
            Period = 1000 / frequency;
        }

        public override float[] Next(int size)
        {
            var buffer = GetNextBuffer(size);

            int offset = 0;
            if (LeftOvers.Count > 0)
            {
                int count = Math.Min(LeftOvers.Count, size);
                while (count-- > 0)
                {
                    buffer[offset++] = LeftOvers.Dequeue();
                }
            }

            int leftCount = size - offset;
            while (leftCount > 0)
            {
                var next = new float[(int)(Format.SampleRate * Period / 1000)];
                GenerateNextBufferForPeriod(next);

                int copy = Math.Min(next.Length, leftCount);
                int pos = 0;

                while (copy > pos)
                {
                    buffer[offset] = next[pos];
                    offset++;
                    pos++;
                }

                leftCount = size - offset;

                if (leftCount == 0)
                {
                    while (next.Length > pos)
                    {
                        LeftOvers.Enqueue(next[pos++]);
                    }
                }
            }

            Position += (double)size / Format.SampleRate;

            return buffer;
        }

        public void Reset()
        {
            LeftOvers.Clear();
        }

        /// <summary>
        /// 1周期分のデータを返却する抽象メソッド
        /// </summary>
        /// <param name="buffer"></param>
        protected abstract void GenerateNextBufferForPeriod(float[] buffer);
    }
}