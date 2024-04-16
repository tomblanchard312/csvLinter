using CsvHelper;
using CsvHelper.Configuration;

using csvSQLLinter.Api.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace csvSQLLinter.Api
{
    public class CsvLinter
    {
        private Dictionary<string, SqlServerType> _schema;

        public CsvLinter(Dictionary<string, SqlServerType> schema)
        {
            _schema = schema;
        }

        public List<string> LintCsv(Stream csvStream)
        {
            var issues = new List<string>();
            using (var reader = new StreamReader(csvStream))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                csv.Read();
                csv.ReadHeader();
                var headerRecord = csv.Context.Reader.HeaderRecord;  // Corrected access to header records

                foreach (var column in _schema.Keys)
                {
                    if (!headerRecord.Contains(column))
                    {
                        issues.Add($"Missing expected column: {column}");
                    }
                    else
                    {
                        var columnIndex = Array.IndexOf(headerRecord, column);
                        while (csv.Read())
                        {
                            var value = csv.GetField(columnIndex);
                            var type = _schema[column].ToSystemType();
                            if (!IsValidType(value, type))
                            {
                                issues.Add($"Column '{column}' expected to be {_schema[column]} but found '{value}'.");
                            }
                        }
                    }
                }
            }
            return issues;
        }

        private bool IsValidType(string value, Type type)
        {
            try
            {
                if (type == typeof(int))
                    int.Parse(value);
                else if (type == typeof(DateTime))
                    DateTime.Parse(value);
                else if (type == typeof(float))
                    float.Parse(value);
                else if (type == typeof(decimal))
                    decimal.Parse(value);
                else if (type == typeof(string))
                    return true; // String is always valid
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
