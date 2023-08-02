using CsvHelper;
using CsvHelper.Configuration;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace App.Library.Language
{
    public class LanguageText : ILanguageText
    {
        private const string TokenStart = "<%";

        private const string TokenEnd = "%>";

        private readonly Dictionary<string, Dictionary<string, string>> allStrings;

        private readonly string preFix;

        private Dictionary<string, string> activeStrings;

        private string languageId;

        /// <summary />
        public LanguageText(string fileName, string languageId)
            : base()
        {
            this.languageId = languageId;
            this.allStrings = new Dictionary<string, Dictionary<string, string>>();
            this.preFix = Debugger.IsAttached ? "*" : string.Empty;

            this.ReadItemsFromFile(fileName);
            this.SetLanguageId(languageId);
        }

        /// <summary />
        //public string this[string key]
        //{
        //    get
        //    {
        //        if (this.activeStrings.TryGetValue(key, out var item))
        //        {
        //            return Debugger.IsAttached
        //                       ? $"{this.preFix}{this.ReplaceTokenStrings(item)}"
        //                       : this.ReplaceTokenStrings(item);
        //        }

        //        return string.Format(CultureInfo.CurrentCulture, "{0} not found", key);
        //    }
        //}

        /// <summary />
        public void SetLanguageId(string newLanguageId)
        {
            this.languageId = newLanguageId;
            this.activeStrings = this.allStrings[this.languageId];
        }

        /// <summary />
        private void ReadItemsFromFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(fileName);
            using (var reader = new StreamReader(stream))
            {
                using (var csvReader = new CsvReader(
                           reader,
                           new CsvConfiguration(CultureInfo.InvariantCulture)
                           {
                               Delimiter = ";"
                           }))
                {
                    var numberOfColumns = 0;
                    var columnMapping = new Dictionary<int, string>();
                    csvReader.Read();

                    while (csvReader.TryGetField(numberOfColumns, out string headerValue))
                    {
                        // Add Language Id's here
                        if (numberOfColumns > 0)
                        {
                            if (!this.allStrings.ContainsKey(headerValue))
                            {
                                this.allStrings.Add(headerValue, new Dictionary<string, string>());
                            }

                            columnMapping.Add(numberOfColumns, headerValue);
                        }

                        numberOfColumns++;
                    }

                    this.ReadLines(csvReader, numberOfColumns, columnMapping);
                }
            }
        }

        private void ReadLines(CsvReader csvReader, int numberOfColumns, Dictionary<int, string> columnMapping)
        {
            while (csvReader.Read())
            {
                try
                {
                    var fieldKey = csvReader.GetField(0);

                    for (var i = 1; i < numberOfColumns; i++)
                    {
                        var fieldValue = csvReader.GetField(i);

                        if (this.allStrings.TryGetValue(columnMapping[i], out var value))
                        {
                            if (value.ContainsKey(fieldKey))
                            {
                                this.allStrings[columnMapping[i]][fieldKey] = fieldValue;
                            }
                            else
                            {
                                this.allStrings[columnMapping[i]]
                                    .Add(fieldKey, fieldValue);
                            }
                        }
                        else
                        {
                            this.allStrings[columnMapping[i]]
                                .Add(fieldKey, fieldValue);
                        }
                    }
                }
                catch (Exception exp)
                {
                    var lastKey = this.allStrings.First()
                                      .Value.Keys.Last();

                    throw new Exception($"Record after {lastKey} is faulty", exp);
                }
            }
        }

        private string ReplaceTokenStrings(string parentKey)
        {
            var start = parentKey.IndexOf(TokenStart, StringComparison.OrdinalIgnoreCase);
            var end = parentKey.IndexOf(TokenEnd, StringComparison.OrdinalIgnoreCase);

            while (start > -1
                   && end > -1
                   && end > start)
            {
                var subKey = parentKey.Substring(start + TokenStart.Length, end - (start + TokenStart.Length));
                var value = ""; //this[subKey];

                parentKey = parentKey.Replace(TokenStart + subKey + TokenEnd, value);

                start = parentKey.IndexOf(TokenStart, StringComparison.OrdinalIgnoreCase);
                end = parentKey.IndexOf(TokenEnd, StringComparison.OrdinalIgnoreCase);
            }

            return parentKey;
        }
    }
}