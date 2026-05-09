using System;
using System.Collections.Generic;
using System.Text;

namespace Babbelite.Server.Core
{
    public abstract class TranslationService
    {
        public abstract Task<string> Translate(string text, string sourceLanguage, string targetLanguage);
    }
}
