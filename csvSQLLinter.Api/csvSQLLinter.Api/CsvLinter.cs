using CsvHelper;
using CsvHelper.Configuration;

using csvSQLLinter.Api.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace csvSQLLinter.Api
{
    using CsvHelper;
    using CsvHelper.Configuration;

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using CsvHelper;
    using CsvHelper.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    public class CsvLinter
    {
        private Dictionary<string, Dictionary<string, SqlServerType>> _schemas;
        private Dictionary<string, Dictionary<string, string[]>> _enumerations;

        public CsvLinter(Dictionary<string, Dictionary<string, SqlServerType>> schemas,
                         Dictionary<string, Dictionary<string, string[]>> enumerations)
        {
            _schemas = schemas;
            _enumerations = enumerations;
        }

        public List<string> LintCsv(Stream csvStream, string schemaType)
        {
            var issues = new List<string>();
            if (!_schemas.ContainsKey(schemaType))
            {
                issues.Add("Invalid schema type specified.");
                return issues;
            }

            var schema = _schemas[schemaType];
            var enumeration = _enumerations.ContainsKey(schemaType) ? _enumerations[schemaType] : new Dictionary<string, string[]>();

            using (var reader = new StreamReader(csvStream))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                csv.Read();
                csv.ReadHeader();
                var headerRecord = csv.Context.Reader.HeaderRecord;

                // Validate number of columns matches schema
                if (headerRecord.Length != schema.Count)
                {
                    issues.Add($"The number of columns in the CSV ({headerRecord.Length}) does not match the expected number ({schema.Count}).");
                    return issues;  // Return early since further validation is not meaningful
                }

                while (csv.Read())
                {
                    foreach (var column in schema.Keys)
                    {
                        var columnIndex = Array.IndexOf(headerRecord, column);
                        if (columnIndex == -1)
                        {
                            issues.Add($"Missing expected column: {column}");
                            continue;
                        }

                        var value = csv.GetField(columnIndex);
                        if (schema[column] == SqlServerType.Varchar && enumeration.ContainsKey(column) && !enumeration[column].Contains(value))
                        {
                            issues.Add($"Column '{column}' has invalid value '{value}'. Expected values: {string.Join(", ", enumeration[column])}.");
                        }
                        else if (!IsValidType(value, schema[column].ToSystemType()))
                        {
                            issues.Add($"Column '{column}' expected to be {schema[column]} but found '{value}'.");
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
