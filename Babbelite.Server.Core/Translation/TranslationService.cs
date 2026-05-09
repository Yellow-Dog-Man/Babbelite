using Babbelite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Server.Core
{
    public abstract class TranslationService
    {
        public async Task<TranslatedText> Translate(TranslateText text)
        {
            var translated = await Translate(text.Text, text.SourceLanguage, text.TargetLangoage);

            return new TranslatedText()
            {
                IsSuccess = true,
                SourceLanguage = text.SourceLanguage,
                TargetLangoage = text.TargetLangoage,
                Text = translated
            };
        }

        protected abstract Task<string> Translate(string text, string sourceLanguage, string targetLanguage);
    }
}
