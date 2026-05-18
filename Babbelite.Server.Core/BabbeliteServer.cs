using Babbelite.Shared;
using EchoSharp.VoiceActivityDetection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using WatsonWebsocket;

namespace Babbelite.Server.Core
{
    public class BabbeliteServer
    {
        WatsonWsServer _server;

        // All the currently connected clients to this server
        Dictionary<ClientMetadata, ClientSession> _sessions = new Dictionary<ClientMetadata, ClientSession>();

        #region CONNECTION
        public int Port { get; private set; }
        public string UniqueID { get; private set; }
        public string ServerName => Config.ServerName;

        #endregion

        public Config Config { get; private set; }

        #region SERVICES
        public TranscriptionService Transcription { get; private set; }
        public IReadOnlyList<TranslationService> TranslationServices => _translationServices;

        private List<TranslationService> _translationServices = new List<TranslationService>();

        public async ValueTask<TranslationService?> FindBestTranslationService(string sourceLanguage, string targetLanguage)
        {
            TranslationService? bestService = null;
            bool bestPrefersTargetLanguage = false;
            bool bestPrefersSourceLanguage = false;
            
            // Try to find a preferred one first if possible
            foreach (var service in _translationServices)
            {
                // Filter services that don't support the source or target languages
                if (!(await service.SupportsTranslation(sourceLanguage, targetLanguage)))
                    continue;
                
                var prefersSource = service.IsPreferredForLanguage(sourceLanguage);
                var prefersTarget = service.IsPreferredForLanguage(targetLanguage);

                // Best case scenario. Preferred for both source and target languages
                // We can just return it straight away
                if (prefersSource && prefersTarget)
                    return service;

                void Select()
                {
                    bestService = service;
                    
                    bestPrefersSourceLanguage = prefersSource;
                    bestPrefersTargetLanguage = prefersTarget;
                }

                // If we have a service that prefers the target language and this one doesn't
                // ignore it. We want to prefer the target language over the source
                if (bestPrefersTargetLanguage && !prefersTarget)
                    continue;

                // If the service prefers the target language, use it
                // We currently don't have any service that prefers target
                if (prefersTarget)
                {
                    Select();
                    continue;
                }

                // If we don't have service that prefers target, but this one prefers source
                // Then use it as well if we haven't found one that prefers source yet
                if (!bestPrefersSourceLanguage && prefersSource)
                {
                    Select();
                    continue;
                }
                
                // We we don't have anything yet, we use what we can
                if(bestService == null)
                    Select();
            }

            // Give what we found. This can also be null if none of the services support given language
            return bestService;
        }

        #endregion

        BabbeliteServerAnnouncer announcer;

        public async Task Initialize(Config config)
        {
            if (this.Config != null)
                throw new InvalidOperationException("Server is already initialized");
            
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            this.Port = config.Port;
            this.UniqueID = Guid.NewGuid().ToString();
            this.Config = config;

            _server = new WatsonWsServer(Config.HostName ?? "localhost", this.Port);

            _server.ClientConnected += ClientConnected;
            _server.ClientDisconnected += ClientDisconnected;
            _server.MessageReceived += MessageReceived;

            _server.Logger += msg => Console.WriteLine($"WS: {msg}");

            _server.Start();

            // This will automatically announce the server on LAN
            announcer = new BabbeliteServerAnnouncer(this);
            
            // Initialize the services
            InitializeTranscription(Config.Transcription);
            
            if(Config.TranslationServices != null)
                foreach (var serviceConfig in Config.TranslationServices)
                {
                    var service = InstantiateTranslation(serviceConfig);
                    await service.Initialize();
                    
                    _translationServices.Add(service);
                }
            
            // Sort translation engines by their priority
            _translationServices.Sort((a,b) => -a.Priority.CompareTo(b.Priority));
        }

        void InitializeTranscription(TranscriptionConfig config)
        {
            switch(config)
            {
                case WhisperConfig whisper:
                    Transcription = new WhisperTranscriptionService(whisper);
                    break;

                case null:
                    Console.WriteLine($"No transcription service configured");
                    break;

                default:
                    throw new NotImplementedException($"Unsupported config: {config}");
            }
        }

        TranslationService InstantiateTranslation(TranslationConfig config)
        {
            switch(config)
            {
                case LibreTranslateConfig libreTranslate:
                    return new LibreTranslateTranslationService(libreTranslate);
                
                case DeepLConfig deepL:
                    return new DeepLTranslationService(deepL);

                case null:
                    throw new ArgumentException("Null translation service config");

                default:
                    throw new NotImplementedException($"Unsupported config: {config}");
            }
        }

        void MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            _sessions[e.Client].EnqueueMessageForProcessing(e);
        }

        void ClientDisconnected(object? sender, DisconnectionEventArgs e)
        {
            Console.WriteLine($"Client disconnected: {e.Client}\n" + Environment.StackTrace);

            _sessions[e.Client].Dispose();
            _sessions.Remove(e.Client);
        }

        void ClientConnected(object? sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"Client connected: {e.Client}");

            var session = new ClientSession(this, e.Client);
            _sessions.Add(e.Client, session);
        }

        internal void SendResponse(ClientSession session, Response response)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (!await SendResponseAsync(session, response).ConfigureAwait(false))
                        throw new Exception($"Response {response} failed to send");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to send response: {response}\n{ex}");
                }
            });
        }

        internal Task<bool> SendResponseAsync(ClientSession session, Response response)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(response, SerializationHelper.SerializationOptions);

            return _server.SendAsync(session.Client.Guid, json);
        }
    }
}
