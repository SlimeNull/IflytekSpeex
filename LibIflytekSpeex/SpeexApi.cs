// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;

namespace LibIflytekSpeex
{
    internal static unsafe class SpeexApi
    {
        const string DllName = "speex.dll";

        static readonly int[] s_wbBlockFrameLengths =
            new int[] { 11, 16, 21, 26, 33, 43, 53, 61, 71, 87, 107 };
        static readonly int[] s_nbBlockFrameLengths =
            new int[] { 7, 11, 16, 21, 21, 29, 29, 39, 39, 47, 63 };

        [DllImport(DllName, EntryPoint = "SpeexEncodeInit")]
        public static extern int EncodeInit(void** encodeHandle, short speexMode);

        [DllImport(DllName, EntryPoint = "SpeexEncodeFini")]
        public static extern int EncodeFini(void* encodeHandle);

        [DllImport(DllName, EntryPoint = "SpeexEncode")]
        public static extern int Encode(void* encodeHandle, byte* audio, uint audioLen, byte* speex, uint* speexLen, short quality);


        [DllImport(DllName, EntryPoint = "SpeexDecodeInit")]
        public static extern int DecodeInit(void** decodeHandle, short speexMode);

        [DllImport(DllName, EntryPoint = "SpeexDecodeInit")]
        public static extern int DecodeFini(void* decodeHandle);

        [DllImport(DllName, EntryPoint = "SpeexDecode")]
        public static extern int Decode(void* decodeHandle, byte* speex, uint speexLen, byte* audio, uint* audioLen);


        [DllImport(DllName, EntryPoint = "SpeexGetNbFrameLen")]
        public static extern int GetNbFrameLen(int mode);

        [DllImport(DllName, EntryPoint = "SpeexGetWbFrameLen")]
        public static extern int GetWbFrameLen(int mode);

        [DllImport(DllName, EntryPoint = "SpeexDetectFrameLen")]
        public static extern int DetectFrameLen(byte* frame);
    }
}