using System;
using System.Collections.Generic;
using System.Text;
using LibreTranslate.Net.Enhanced;

namespace Babbelite.Server.Core
{
    public class LibreTranslateTranslationService : TranslationService
    {
        LibreTranslate.Net.Enhanced.LibreTranslate _translator;

        public LibreTranslateTranslationService(LibreTranslateConfig config)
        {
            Console.WriteLine($"Connecting to LibreTranslate at {config.HostURL}");

            _translator = new LibreTranslate.Net.Enhanced.LibreTranslate(config.HostURL, config.ApiKey);
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
