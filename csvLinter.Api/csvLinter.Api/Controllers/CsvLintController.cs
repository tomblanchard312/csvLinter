
using csvLinter.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace csvLinter.Api.Controllers
{
    /// <summary>
    /// Handles CSV linting operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CsvLintController : ControllerBase
    {
        [HttpPost]
        public IActionResult LintCsv(IFormFile csvFile, string schemaType)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var stream = csvFile.OpenReadStream())
            {
                var errors = ValidateCsv(stream, schemaType);
                if (errors.Any())
                {
                    return BadRequest(errors);
                }
            }

            return Ok("CSV validated successfully.");
        }
        private Dictionary<string, int> GetHeaderIndices(StreamReader reader)
        {
            var headers = reader.ReadLine().Split(',').Select(h => h.Trim().ToLower()).ToArray();
            var headerIndices = new Dictionary<string, int>();

            for (int i = 0; i < headers.Length; i++)
            {
                headerIndices[headers[i]] = i;
            }

            return headerIndices;
        }
        private List<string> ValidateCsv(Stream csvStream, string schemaType)
        {
            var errors = new List<string>();
            var schema = CsvValidationHelper.GetSchema(schemaType);

            using (var reader = new StreamReader(csvStream))
            {
                var headers = reader.ReadLine().Split(',').Select(h => h.Trim().ToLower()).ToArray();
                var headerIndexMap = headers.Select((header, index) => new { header, index })
                                            .ToDictionary(h => h.header, h => h.index);

                string line;
                int lineNumber = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    var values = line.Split(',');
                    foreach (var schemaEntry in schema)
                    {
                        var normalizedKey = schemaEntry.Key.ToLower().Trim();
                        if (headerIndexMap.TryGetValue(normalizedKey, out int index))
                        {
                            string errorMessage = "";
                            if (index >= values.Length || !CsvValidationHelper.ValidateField(schemaType, schemaEntry.Key, values[index], schemaEntry.Value, out errorMessage))
                            {
                                errors.Add($"Line {lineNumber}: {errorMessage}");
                            }
                        }
                        else
                        {
                            errors.Add($"Line {lineNumber}: Missing '{schemaEntry.Key}' in CSV.");
                        }
                    }
                }
            }
            return errors;
        }
    }
}
