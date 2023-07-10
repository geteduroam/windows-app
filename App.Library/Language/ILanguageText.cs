using System.Collections.Generic;

namespace App.Library.Language
{
    public interface ILanguageText
    {
        /// <summary />
        string this[string key] { get; }

        /// <summary />
        Dictionary<string, string> GetActiveStrings();
    }
}