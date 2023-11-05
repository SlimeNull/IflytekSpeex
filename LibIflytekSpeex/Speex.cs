// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;

namespace LibIflytekSpeex
{
    /// <summary>
    /// 讯飞定制 Speex 编解码算法
    /// </summary>
    public static unsafe class Speex
    {
        const int WideSampleRate = 16000;

        static readonly int[] s_wbFrameSizes =
            new int[] { 11, 16, 21, 26, 33, 43, 53, 61, 71, 87, 107 };
        static readonly int[] s_nbFrameSizes =
            new int[] { 7, 11, 16, 21, 21, 29, 29, 39, 39, 47, 63 };


        static void EnsureSampleRate(int sampleRate, out int mode)
        {
            if (sampleRate == 8000)
            {
                mode = 0;
                return;
            }

            if (sampleRate == 16000)
            {
                mode = 1;
                return;
            }

            throw new ArgumentException("Invalid sample rate, must be 8000 or 16000", nameof(sampleRate));
        }

        static void EnsureEncodingLevel(int encodingLevel)
        {
            if (encodingLevel < 0 || encodingLevel > 10)
                throw new ArgumentException("Invalid encoding level, must between 0 and 10", nameof(encodingLevel));
        }

        static void EnsureBlockSizeForDecoding(int blockSize, int mode, int speexLevel)
        {
            if (mode == 0)
            {
                int baseSize = s_nbFrameSizes[speexLevel];
                if (blockSize % baseSize != 0)
                    throw new ArgumentException("Invalid block size", nameof(blockSize));
            }
            else if (mode == 1)
            {
                int baseSize = s_wbFrameSizes[speexLevel];
                if (blockSize % baseSize != 0)
                    throw new ArgumentException("Invalid block size", nameof(blockSize));
            }
            else
            {
                throw new InvalidOperationException("This would never happen");
            }
        }

        static void CommonInit(int sampleRate, int speexLevel, out int frameLen, out int sizeOfOnePcmFrame)
        {
            if (sampleRate == WideSampleRate)
            {
                frameLen = SpeexApi.GetWbFrameLen(speexLevel);
                sizeOfOnePcmFrame = 640;
            }
            else
            {
                frameLen = SpeexApi.GetNbFrameLen(speexLevel);
                sizeOfOnePcmFrame = 320;
            }
        }



        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="sourceStream">编码源</param>
        /// <param name="destStream">编码输出目标</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="speexLevel">Speex 编码等级</param>
        /// <param name="blockSize">一次编码大小</param>
        /// <exception cref="SpeexException">Speex 初始化或编码失败</exception>
        public static void Encode(Stream sourceStream, Stream destStream, int sampleRate, int speexLevel, int blockSize)
        {
            EnsureSampleRate(sampleRate, out int mode);
            EnsureEncodingLevel(speexLevel);

            CommonInit(sampleRate, speexLevel, out int frameLen, out int sizeOfOnePcmFrame);

            void* speexHandle = null;

            int inputOffset = 0;
            int inputSize;

            bool isLast = false;

            int err = SpeexApi.EncodeInit(&speexHandle, (short)(sampleRate / WideSampleRate));
            if (err != 0)
                throw new SpeexException("Speex init failed");

            try
            {
                byte[] sourceBuffer = new byte[blockSize];
                fixed (byte* sourceBufferPtr = sourceBuffer)
                {
                    while (true)
                    {
                        int readCount = sourceStream.Read(sourceBuffer, 0, blockSize);

                        if (readCount == 0)
                            break;

                        if (readCount < blockSize)
                        {
                            inputSize = readCount;
                            isLast = true;
                        }
                        else
                        {
                            inputSize = blockSize;
                        }

                        int encodeFrameNum = inputSize / sizeOfOnePcmFrame;
                        uint encodedSize = (uint)(frameLen * (encodeFrameNum + 1));
                        byte[] encodedBuffer = new byte[encodedSize];

                        fixed (byte* encodedBufferPtr = encodedBuffer)
                        {
                            int ret = SpeexApi.Encode(speexHandle, sourceBufferPtr, (uint)inputSize, encodedBufferPtr, &encodedSize, (short)speexLevel);
                            if (ret != 0)
                                throw new SpeexException("Speex encode failed");
                        }

                        inputOffset += inputSize;
                        destStream.Write(encodedBuffer, 0, (int)encodedSize);

                        if (isLast)
                            break;
                    }
                }
            }
            finally
            {
                SpeexApi.EncodeFini(speexHandle);
            }
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="sourceStream">解码源</param>
        /// <param name="destStream">解码输出目标</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="speexLevel">Speex 编码等级</param>
        /// <param name="blockSize">一次编码大小</param>
        /// <exception cref="SpeexException">Speex 初始化或解码失败</exception>
        public static void Decode(Stream sourceStream, Stream destStream, int sampleRate, int speexLevel, int blockSize)
        {
            EnsureSampleRate(sampleRate, out int mode);
            EnsureEncodingLevel(speexLevel);
            EnsureBlockSizeForDecoding(blockSize, mode, speexLevel);

            CommonInit(sampleRate, speexLevel, out int frameLen, out int sizeOfOnePcmFrame);

            void* speexHandle = null;

            int inputOffset = 0;
            int inputSize;

            bool isLast = false;

            int err = SpeexApi.DecodeInit(&speexHandle, (short)(sampleRate / WideSampleRate));
            if (err != 0)
                throw new SpeexException("Speex init failed");

            try
            {
                byte[] sourceBuffer = new byte[blockSize];
                fixed (byte* sourceBufferPtr = sourceBuffer)
                {
                    while (true)
                    {
                        int readCount = sourceStream.Read(sourceBuffer, 0, blockSize);

                        if (readCount == 0)
                            break;

                        if (readCount < blockSize)
                        {
                            inputSize = readCount;
                            isLast = true;
                        }
                        else
                        {
                            inputSize = blockSize;
                        }

                        int decodedFrameLen = inputSize;
                        int decodeFrameNum = inputSize / frameLen;
                        uint decodedSize = (uint)(sizeOfOnePcmFrame * decodeFrameNum);
                        byte[] decodedBuffer = new byte[decodedSize];

                        fixed (byte* decodeBufferPtr = decodedBuffer)
                        {
                            err = SpeexApi.Decode(speexHandle, sourceBufferPtr, (uint)inputSize, decodeBufferPtr, &decodedSize);
                            if (err != 0)
                                throw new SpeexException("Decode failed");
                        }


                        inputOffset += inputSize;
                        destStream.Write(decodedBuffer, 0, (int)decodedSize);

                        if (isLast)
                            break;
                    }
                }
            }
            finally
            {
                SpeexApi.DecodeFini(speexHandle);
            }
        }

        /// <summary>
        /// 将字节数组进行编码
        /// </summary>
        /// <param name="source">源字节数组</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="speexLevel">Speex 编码等级</param>
        /// <param name="blockSize">一次编码大小</param>
        /// <returns>编码结果</returns>
        /// <exception cref="SpeexException">Speex 初始化或编码失败</exception>
        public static byte[] EncodeBytes(byte[] source, int sampleRate, int speexLevel, int blockSize)
        {
            MemoryStream sourceStream = new MemoryStream(source);
            MemoryStream destStream = new MemoryStream();

            Encode(sourceStream, destStream, sampleRate, speexLevel, blockSize);
            return destStream.ToArray();
        }


        /// <summary>
        /// 将字节数组进行解码
        /// </summary>
        /// <param name="source">源字节数组</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="speexLevel">Speex 编码等级</param>
        /// <param name="blockSize">一次编码大小</param>
        /// <returns>解码结果</returns>
        /// <exception cref="SpeexException">Speex 初始化或解码失败</exception>
        public static byte[] DecodeBytes(byte[] source, int sampleRate, int speexLevel, int blockSize)
        {
            EnsureSampleRate(sampleRate, out int mode);
            EnsureEncodingLevel(speexLevel);
            EnsureBlockSizeForDecoding(blockSize, mode, speexLevel);

            CommonInit(sampleRate, speexLevel, out int frameLen, out int sizeOfOnePcmFrame);

            void* speexHandle = null;

            int totalDecodedFrameLen = source.Length;
            int totalDecodeFrameNum = source.Length / frameLen;
            int totalDecodeSize = sizeOfOnePcmFrame * totalDecodeFrameNum;

            int inputOffset = 0;
            int inputSize;
            int remainSize = source.Length;

            bool isLast = false;
            MemoryStream output = new MemoryStream();

            int err = SpeexApi.DecodeInit(&speexHandle, (short)(sampleRate / WideSampleRate));
            if (err != 0)
                throw new SpeexException("Speex init failed");

            fixed (byte* sourcePtr = source)
            {
                while (true)
                {
                    if (remainSize <= blockSize)
                    {
                        inputSize = remainSize;
                        isLast = true;
                    }
                    else
                    {
                        inputSize = blockSize;
                        remainSize -=  blockSize;
                    }

                    int decodedFrameLen = inputSize;
                    int decodeFrameNum = inputSize / frameLen;
                    uint decodedSize = (uint)(sizeOfOnePcmFrame * decodeFrameNum);
                    byte[] decodedBuffer = new byte[decodedSize];

                    fixed (byte* decodeBufferPtr = decodedBuffer)
                    {
                        err = SpeexApi.Decode(speexHandle, sourcePtr + inputOffset, (uint)inputSize, decodeBufferPtr, &decodedSize);
                        if (err != 0)
                            throw new SpeexException("Decode failed");
                    }


                    inputOffset += inputSize;
                    output.Write(decodedBuffer, 0, (int)decodedSize);

                    if (isLast)
                        break;
                }
            }

            return output.ToArray();
        }


        /// <summary>
        /// 检查 BlockSize 是否是有效的
        /// </summary>
        /// <param name="sampleRate">采样率</param>
        /// <param name="speexLevel">Speex 编码等级</param>
        /// <param name="blockSize">要检查的 BlockSize</param>
        /// <returns>是否有效</returns>
        /// <exception cref="ArgumentOutOfRangeException">传入的采样率不是有效值</exception>
        public static bool IsValidBlockSize(int sampleRate, int speexLevel, int blockSize)
        {
            int mode = sampleRate / WideSampleRate;

            if (mode == 0)
            {
                int baseSize = s_nbFrameSizes[speexLevel];
                if (blockSize % baseSize != 0)
                    return false;
            }
            else if (mode == 1)
            {
                int baseSize = s_wbFrameSizes[speexLevel];
                if (blockSize % baseSize != 0)
                    return false;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), "Sample rate must be 8000 or 16000");
            }

            return true;
        }
    }
}