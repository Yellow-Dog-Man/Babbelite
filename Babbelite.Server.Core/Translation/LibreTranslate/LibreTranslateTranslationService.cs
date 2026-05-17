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

        private List<SupportedLanguages> _languages;
        private HashSet<string> _languageCodes;

        public LibreTranslateTranslationService(LibreTranslateConfig config) : base(config)
        {
            Console.WriteLine($"Connecting to LibreTranslate at {config.HostURL}");

            _translator = new LibreTranslate.Net.Enhanced.LibreTranslate(config.HostURL, config.ApiKey);
        }
        
        public override async ValueTask<bool> SupportsLanguage(string language)
        {
            if (_languages == null)
            {
                _languages = (await _translator.GetSupportedLanguagesAsync()).ToList();
                _languageCodes = _languages.Select(l => l.Code.ToLowerInvariant()).ToHashSet();
            }

            return _languageCodes.Contains(language.ToLowerInvariant());
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
