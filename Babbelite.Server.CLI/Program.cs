using Babbelite.Server.Core;
using EchoSharp.Onnx.SileroVad;

var configFile = @"Config.json";

Config config;

if(File.Exists(configFile))
{
    Console.WriteLine("Loading Config.json");
    config = System.Text.Json.JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile)); 
}
else
{
    Console.WriteLine("Using debug config");

    config = new Config();

    config.ServerName = "TestSever";
    config.Port = 12052;

    config.Transcription = new WhisperConfig()
    {
        WhisperModelPath = @"C:\Workspace\TestEchoSharp\TestEchoSharp\models\ggml-large-v3-turbo.bin",
        SileroVadModelPath = @"C:\Workspace\TestEchoSharp\TestEchoSharp\models\silero_vad.onnx"
    };
}

var server = new BabbeliteServer();
await server.Initialize(config);

Console.WriteLine($"Server started on: {server.Port}");

Console.ReadLine();