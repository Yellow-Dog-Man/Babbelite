using Babbelite.Server.Core;
using EchoSharp.Onnx.SileroVad;

var config = new Config();

config.Transcription = new WhisperConfig()
{
    WhisperModelPath = @"C:\Workspace\TestEchoSharp\TestEchoSharp\models\ggml-large-v3-turbo.bin",
    SileroVadModelPath = @"C:\Workspace\TestEchoSharp\TestEchoSharp\models\silero_vad.onnx"
};

var server = new BabbeliteServer(12052, config);

Console.WriteLine($"Server started on: {server.Port}");

Console.ReadLine();