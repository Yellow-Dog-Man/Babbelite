using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LibreTranslate.Net.Enhanced;
using LibreTranslate.Net.Enhanced.Models;

namespace Babbelite.Server.Core
{
    public class LibreTranslateTranslationService : TranslationService
    {
        LibreTranslate.Net.Enhanced.LibreTranslate _translator;

        List<SupportedLanguages> _languages;
        Dictionary<string, HashSet<string>> _languagesByCode;

        public LibreTranslateTranslationService(LibreTranslateConfig config) : base(config)
        {
            Console.WriteLine($"Connecting to LibreTranslate at {config.HostURL}");

            _translator = new LibreTranslate.Net.Enhanced.LibreTranslate(config.HostURL, config.ApiKey);
        }

        async Task InitializeLanguages()
        {
            // Build the language map
            _languages = (await _translator.GetSupportedLanguagesAsync()).ToList();
            _languagesByCode = new Dictionary<string, HashSet<string>>();
                
            foreach(var lang in _languages)
                _languagesByCode.Add(lang.Code.ToLowerInvariant(), 
                    lang.Targets.Select(l => l.ToLowerInvariant()).ToHashSet());
        }
        
        public override async ValueTask<bool> SupportsTranslation(string sourceLanguage, string targetLanguage)
        {
            if (_languagesByCode == null)
                await InitializeLanguages();

            sourceLanguage = sourceLanguage.ToLowerInvariant();
            targetLanguage = targetLanguage.ToLowerInvariant();

            if (!_languagesByCode.TryGetValue(sourceLanguage, out var suportedLanguages))
                return false;

            return suportedLanguages.Contains((targetLanguage));
        }

        protected override async Task<string> Translate(string text, string sourceLanguage, string targetLanguage)
        {
            try
            {
                return await _translator.TranslateAsync(new LibreTranslate.Net.Enhanced.Models.Translate()
                {
                    Source = sourceLanguage,
                    Target = targetLanguage,
                    Text = text
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception translating from {sourceLanguage} to {targetLanguage}\n{ex}");
                throw;
            }
        }
    }
}
