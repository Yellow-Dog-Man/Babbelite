using Babbelite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Server.Core
{
    public abstract class TranslationService
    {
        private HashSet<string> _preferredLanguages;

        public TranslationService(TranslationConfig config)
        {
            _preferredLanguages = config.PreferredLanguages;
        }

        public bool IsPreferredForLanguage(string language) =>
            _preferredLanguages?.Contains(language.ToLowerInvariant()) ?? false;
        
        public abstract ValueTask<bool> SupportsLanguage(string language);
        
        public async Task<TranslatedText> Translate(TranslateText text)
        {
            var translated = await Translate(text.Text, text.SourceLanguage, text.TargetLanguage);

            return new TranslatedText()
            {
                IsSuccess = true,
                SourceLanguage = text.SourceLanguage,
                TargetLangoage = text.TargetLanguage,
                Text = translated
            };
        }

        protected abstract Task<string> Translate(string text, string sourceLanguage, string targetLanguage);
    }
}
