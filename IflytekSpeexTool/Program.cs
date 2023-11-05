using LibIflytekSpeex;

if (args.Length != 6)
{
    Console.WriteLine("Usage: IflytekSpeexTool Encode/Decode SampleRate SpeexLevel BlockSize InputFile OutputFile");
    return;
}

if (!int.TryParse(args[1], out var sampleRate) ||
    (sampleRate != 8000 && sampleRate != 16000))
{
    Console.WriteLine("Invalid sample rate, only 8000 and 16000 is supported");
    return;
}

if (!int.TryParse(args[2], out var speexLevel) ||
    speexLevel < 0 ||
    speexLevel > 10)
{
    Console.WriteLine("Invalid speex level, only values between 0 and 10 are supported");
    return;
}

if (!int.TryParse(args[3], out var blockSize))
{
    Console.WriteLine("Invalid block size, value is not integer");
    return;
}

if (args[4] is not string inputFile ||
    !File.Exists(inputFile))
{
    Console.WriteLine("Input file not exist");
    return;
}

string mode = args[0];
var outputFile = args[5];

using FileStream input = File.OpenRead(inputFile);
using FileStream output = File.Create(outputFile);

if (mode.Equals("Encode", StringComparison.OrdinalIgnoreCase) ||
    mode.Equals("Enc", StringComparison.OrdinalIgnoreCase) ||
    mode.Equals("-Encode", StringComparison.OrdinalIgnoreCase) ||
    mode.Equals("-Enc", StringComparison.OrdinalIgnoreCase))
{
    PrintInfo(mode, sampleRate, speexLevel, blockSize, inputFile, outputFile);

    Console.WriteLine("Started");
    Speex.Encode(input, output, sampleRate, speexLevel, blockSize);

    Console.WriteLine("OK");
}
else if (
    mode.Equals("Decode", StringComparison.OrdinalIgnoreCase) ||
    mode.Equals("Dec", StringComparison.OrdinalIgnoreCase) ||
    mode.Equals("-Decode", StringComparison.OrdinalIgnoreCase) ||
    mode.Equals("-Dec", StringComparison.OrdinalIgnoreCase))
{
    if (!Speex.IsValidBlockSize(sampleRate, speexLevel, blockSize))
    {
        Console.WriteLine("Invalid block size, value doesn't match sample rate and speex level");
        return;
    }

    PrintInfo(mode, sampleRate, speexLevel, blockSize, inputFile, outputFile);

    Console.WriteLine("Started");
    Speex.Decode(input, output, sampleRate, speexLevel, blockSize);

    Console.WriteLine("OK");
}
else
{
    Console.WriteLine("Invalid mode, must be Encode or Decode");
}

void PrintInfo(string mode, int sampleRate, int speexLevel, int blockSize, string input, string output)
{
    Console.WriteLine($"Mode: {mode}");
    Console.WriteLine($"SampleRate: {sampleRate}");
    Console.WriteLine($"SpeexLevel: {speexLevel}");
    Console.WriteLine($"BlockSize: {blockSize}");
    Console.WriteLine($"Input: {input}");
    Console.WriteLine($"Ouput: {output}");
}