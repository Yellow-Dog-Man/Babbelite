using Babbelite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Server.Core
{
    public abstract class TranslationService
    {
        public int Priority { get; private set; }
        
        private HashSet<string> _preferredLanguages;

        public TranslationService(TranslationConfig config)
        {
            Priority = config.Priority;
            _preferredLanguages = config.PreferredLanguages;
        }

        public virtual Task Initialize() => Task.CompletedTask;

        public bool IsPreferredForLanguage(string language) =>
            _preferredLanguages?.Contains(language.ToLowerInvariant()) ?? false;
        
        public abstract ValueTask<bool> SupportsTranslation(string sourceLanguage, string targetLanguage);
        
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
