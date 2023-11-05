# IflytekSpeex

�ƴ�Ѷ�ɶ��� Speex ����������� C# ʵ��.

> �ο�����: https://www.xfyun.cn/doc/asr/voicedictation/Audio.html

## �ӿ�

1. �������뵽��
   `Encode(Stream sourceStream, Stream destStream, int sampleRate, int speexLevel, int blockSize) -> void`
2. �������뵽��
   `Decode(Stream sourceStream, Stream destStream, int sampleRate, int speexLevel, int blockSize) -> void`
3. ���ֽ��������
   `EncodeBytes(byte[] source, int sampleRate, int speexLevel, int blockSize) -> byte[]`
4. ���ֽ��������
   `DecodeBytes(byte[] source, int sampleRate, int speexLevel, int blockSize) -> byte[]`
5. �������� BlockSize �����Ƿ���Ч \
   `IsValidBlockSize(int sampleRate, int speexLevel, int blockSize) -> bool`

## ʾ��

������һ���� `Oriens.wav` �ļ������ٽ��������

```csharp
using LibIflytekSpeex;

Console.WriteLine("Hello, World!");

{
    using FileStream input = File.OpenRead("Oriens.wav");
    using FileStream pcmOutput = File.Create("Oriens.pcm");
    using FileStream speexOutput = File.Create("Oriens.speex");

    MemoryStream encoded = new MemoryStream();

    Speex.Encode(input, encoded, 16000, 7, 1280);

    encoded.Seek(0, SeekOrigin.Begin);
    Speex.Decode(encoded, pcmOutput, 16000, 7, 122);

    encoded.Seek(0, SeekOrigin.Begin);
    encoded.CopyTo(speexOutput);
}

Console.WriteLine("OK");
```

## �����й���

�����ͨ���ֿ��е� IflytekSpeexTool ����ݵĽ��� Speex �����, ����ʹ�÷�ʽ����:

```
IflytekSpeexTool Encode/Decode SampleRate SpeexLevel BlockSize InputFile OutputFile
```

## ���ڲ���

����ʱ, BlockSize ��ֵ�������Ƶ�, �����������±���ֵ��������:

| ������ \ ����ȼ� | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
| - | - | - | - | - | - | - | - | - | - | - | - |
| 8000Hz | 7 | 11 | 16 | 21 | 21 | 29 | 29 | 39 | 39 | 47 | 63 |
| 16000Hz | 11 | 16 | 21 | 26 | 33 | 43 | 53 | 61 | 71 | 87 | 107 |

## ע������

���ڿ������õ� `speex.dll` �� 32 λ��, 