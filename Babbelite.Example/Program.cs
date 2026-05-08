using Babbelite.Client;
using System.Runtime.InteropServices;

try
{
    var client = new BabbeliteConnection();
    await client.Connect(new Uri("ws://localhost:12052"), CancellationToken.None);

    var session = await client.CreateTranscriptionSession();
    session.TranscriptionUpdated += Session_TranscriptionUpdated;

    var rawData = File.ReadAllBytes(@"C:\Workspace\Whisper.Net\tests\TestData\bush_float.wav");
    var source = MemoryMarshal.Cast<byte, float>(rawData.AsSpan()).ToArray();

    var data = new float[16000];

    int index = 0;

    for (; ; )
    {
        await Task.Delay(1000);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = source[index++];
            index %= source.Length;
        }

        Console.WriteLine("Pushing data");
        await session.PushAudioData(data);
    }

    Console.ReadLine();

    session.Dispose();
}
catch(Exception ex)
{
    Console.WriteLine(ex);
}

void Session_TranscriptionUpdated(Babbelite.Shared.TranscriptionChunk chunk)
{
    Console.WriteLine(chunk);
}
