# IflytekSpeex

科大讯飞定制 Speex 语音编解码库的 C# 实现.

> 参考链接: https://www.xfyun.cn/doc/asr/voicedictation/Audio.html

## 接口

1. 从流编码到流
   `Encode(Stream sourceStream, Stream destStream, int sampleRate, int speexLevel, int blockSize) -> void`
2. 从流解码到流
   `Decode(Stream sourceStream, Stream destStream, int sampleRate, int speexLevel, int blockSize) -> void`
3. 将字节数组编码
   `EncodeBytes(byte[] source, int sampleRate, int speexLevel, int blockSize) -> byte[]`
4. 将字节数组解码
   `DecodeBytes(byte[] source, int sampleRate, int speexLevel, int blockSize) -> byte[]`
5. 检查编解码的 BlockSize 参数是否有效 \
   `IsValidBlockSize(int sampleRate, int speexLevel, int blockSize) -> bool`

## 示例

下面是一个将 `Oriens.wav` 文件编码再解码的例子

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

## 命令行工具

你可以通过仓库中的 IflytekSpeexTool 来快捷的进行 Speex 编解码, 它的使用方式如下:

```
IflytekSpeexTool Encode/Decode SampleRate SpeexLevel BlockSize InputFile OutputFile
```

## 关于参数

解码时, BlockSize 的值是有限制的, 它必须是以下表中值的整数倍:

| 采样率 \ 编码等级 | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
| - | - | - | - | - | - | - | - | - | - | - | - |
| 8000Hz | 7 | 11 | 16 | 21 | 21 | 29 | 29 | 39 | 39 | 47 | 63 |
| 16000Hz | 11 | 16 | 21 | 26 | 33 | 43 | 53 | 61 | 71 | 87 | 107 |

## 注意事项

由于库所调用的 `speex.dll` 是 32 位的, 