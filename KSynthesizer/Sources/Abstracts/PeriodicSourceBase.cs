using System;
using System.Collections.Generic;

namespace KSynthesizer.Sources
{
    /// <summary>
    /// �����֐��\�[�X�p�̃x�[�X�N���X
    /// 1�������̃I�[�f�B�I�f�[�^��ۊǂ��邽�߁A���g����ύX���Ă�1�������I���܂ł͑O�̎��g����񋟂���
    /// </summary>
    public abstract class PeriodicSourceBase : FunctionalAudioSourceBase, IAudioPeriodicSource
    {
        /// <summary>
        /// Period in millisecond
        /// </summary>
        public virtual float Period { get; set; } = 1000;

        /// <summary>
        /// Next(size)�ŏ����Ȃ�����1�����̎c�o�b�t�@
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
        /// 1�������̃f�[�^��ԋp���钊�ۃ��\�b�h
        /// </summary>
        /// <param name="buffer"></param>
        protected abstract void GenerateNextBufferForPeriod(float[] buffer);
    }
}