using CsvHelper;
using CsvHelper.Configuration;

using csvSQLLinter.Api.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace csvSQLLinter.Api
{
    public class CsvLinter
    {
        private readonly Dictionary<string, Dictionary<string, SqlServerType>> _schemas;
        private readonly Dictionary<string, Dictionary<string, string[]>> _enumerations;

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
                if (!csv.Read() || !csv.ReadHeader())
                {
                    issues.Add("Failed to read CSV header.");
                    return issues;
                }

                var headerRecord = csv.Context.Reader.HeaderRecord;
                var columnIndices = ValidateHeader(schema, headerRecord, issues);
                if (issues.Any()) return issues; // Stop processing if header validation fails

                while (csv.Read())
                {
                    ValidateRow(schema, enumeration, csv, headerRecord, columnIndices, issues);
                }
            }

            return issues;
        }

        private Dictionary<string, int> ValidateHeader(Dictionary<string, SqlServerType> schema, string[] headerRecord, List<string> issues)
        {
            var columnIndices = new Dictionary<string, int>();
            foreach (var column in schema.Keys)
            {
                var columnIndex = Array.IndexOf(headerRecord, column);
                if (columnIndex == -1)
                {
                    issues.Add($"Missing expected column: {column}");
                }
                else
                {
                    columnIndices[column] = columnIndex;
                }
            }

            if (headerRecord.Length != schema.Count)
            {
                issues.Add($"The number of columns in the CSV ({headerRecord.Length}) does not match the expected number ({schema.Count}).");
            }

            return columnIndices;
        }

        private void ValidateRow(Dictionary<string, SqlServerType> schema, Dictionary<string, string[]> enumeration, CsvReader csv, string[] headerRecord, Dictionary<string, int> columnIndices, List<string> issues)
        {
            foreach (var column in schema.Keys)
            {
                if (!columnIndices.TryGetValue(column, out int columnIndex)) continue;

                var value = csv.GetField(columnIndex);
                if (!IsValidType(value, schema[column].ToSystemType()))
                {
                    issues.Add($"Column '{column}' expected to be {schema[column]} but found '{value}'.");
                }
                else if (schema[column] == SqlServerType.Varchar && enumeration.ContainsKey(column) && !enumeration[column].Contains(value))
                {
                    issues.Add($"Column '{column}' has invalid value '{value}'. Expected values: {string.Join(", ", enumeration[column])}.");
                }
            }
        }

        private bool IsValidType(string value, Type type)
        {
            try
            {
                if (type == typeof(int) && int.TryParse(value, out _))
                    return true;
                if (type == typeof(DateTime) && DateTime.TryParse(value, out _))
                    return true;
                if (type == typeof(float) && float.TryParse(value, out _))
                    return true;
                if (type == typeof(decimal) && decimal.TryParse(value, out _))
                    return true;
                if (type == typeof(string))
                    return true; // String is always valid
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
