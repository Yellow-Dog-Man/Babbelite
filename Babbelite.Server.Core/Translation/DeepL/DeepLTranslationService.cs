using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DeepL;
using LibreTranslate.Net.Enhanced.Models;

namespace Babbelite.Server.Core
{
    // NOTE: I haven't tested this yet (even the dev plan requires a credit card to be put in >///> )
    // If anybody wants to give this a test, feel free to make PR's to remove this notice and/or 
    // remove this notice.
    public class DeepLTranslationService : TranslationService
    {
        DeepLClient _client;

        Dictionary<string, HashSet<string>> _languagesByCode;
        private List<SupportedLanguages> _languages;

        HashSet<string> _sourceLanguages;
        HashSet<string> _targetLanguages;

        public DeepLTranslationService(DeepLConfig config) : base(config)
        {
            Console.WriteLine($"Connecting to DeepL");

            _client = new DeepLClient(config.AuthKey);
        }

        public override async Task Initialize()
        {
            var sourceLanguages = await _client.GetSourceLanguagesAsync();
            var targetLanguages = await _client.GetTargetLanguagesAsync();

            _sourceLanguages = sourceLanguages.Select(l => l.Code.ToLowerInvariant()).ToHashSet();
            _targetLanguages = targetLanguages.Select(l => l.Code.ToLowerInvariant()).ToHashSet();
        }

        public override async ValueTask<bool> SupportsTranslation(string sourceLanguage, string targetLanguage)
        {
            sourceLanguage = sourceLanguage.ToLowerInvariant();
            targetLanguage = targetLanguage.ToLowerInvariant();

            if (!_sourceLanguages.Contains(sourceLanguage))
                return false;

            if (!_targetLanguages.Contains(targetLanguage))
                return false;

            return true;
        }

        protected override async Task<string> Translate(string text, string sourceLanguage, string targetLanguage)
        {
            try
            {
                var result = await _client.TranslateTextAsync(text, sourceLanguage, targetLanguage);

                return result.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception translating from {sourceLanguage} to {targetLanguage}\n{ex}");
                throw;
            }
        }
    }
}