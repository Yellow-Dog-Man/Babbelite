using Babbelite.Server.Core;
using EchoSharp.Onnx.SileroVad;

var config = new Config();

config.ServerName = "TestSever";
config.Port = 12052;

config.Transcription = new WhisperConfig()
{
    WhisperModelPath = @"C:\Workspace\TestEchoSharp\TestEchoSharp\models\ggml-large-v3-turbo.bin",
    SileroVadModelPath = @"C:\Workspace\TestEchoSharp\TestEchoSharp\models\silero_vad.onnx"
};

var server = new BabbeliteServer(config);

Console.WriteLine($"Server started on: {server.Port}");

Console.ReadLine();