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
