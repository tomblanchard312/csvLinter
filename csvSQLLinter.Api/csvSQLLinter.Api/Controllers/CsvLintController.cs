using csvSQLLinter.Api.Extensions;
using csvSQLLinter.Api;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.IO;
namespace csvSQLLinter.Api.Controllers
{    /// <summary>
     /// Handles CSV linting operations.
     /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CsvLintController : ControllerBase
    {
        private readonly CsvLinter _linter;
        private readonly ILogger<CsvLintController> _logger;

        // Dependency injection of CsvLinter and ILogger
        public CsvLintController(CsvLinter linter, ILogger<CsvLintController> logger)
        {
            _linter = linter;
            _logger = logger;
            // Configure the schemas and enumerations
            var schemas = new Dictionary<string, Dictionary<string, SqlServerType>>
        {
            {
                "EmployeeDetails", new Dictionary<string, SqlServerType>
                {
                    { "EmployeeId", SqlServerType.Int },
                    { "Name", SqlServerType.Nvarchar },
                    { "HireDate", SqlServerType.DateTime },
                    { "Salary", SqlServerType.Decimal },
                    { "Salutation", SqlServerType.Varchar },
                    { "Gender", SqlServerType.Varchar }
                }
            },
            {
                "Addresses", new Dictionary<string, SqlServerType>
                {
                    { "EmployeeId", SqlServerType.Int },
                    { "Address", SqlServerType.Varchar }
                }
            },
            {
                "Passport", new Dictionary<string, SqlServerType>
                {
                    { "EmployeeId", SqlServerType.Int },
                    { "PassportID", SqlServerType.Nvarchar },
                    { "PassportCountry", SqlServerType.Nvarchar },
                    { "PassportIssuingAuthority", SqlServerType.Nvarchar },
                    { "DateOfBirth", SqlServerType.DateTime },
                    { "Gender", SqlServerType.Varchar }
                }
            }
        };

            var validSalutations = new Dictionary<string, string[]>
        {
            { "Salutation", new[] { "Mr.", "Ms.", "Mrs.", "Dr.", "Mx." } }
        };

            var validGenders = new Dictionary<string, string[]>
        {
            { "Gender", new[] { "male", "female", "x", "u" } }
        };

            var enumerations = new Dictionary<string, Dictionary<string, string[]>>
        {
            { "EmployeeDetails", new Dictionary<string, string[]> { { "Gender", validGenders["Gender"] } } },
            { "Passport", new Dictionary<string, string[]> { { "Gender", validGenders["Gender"] } } }
        };

            // Initialize the linter with the configured schemas and enumerations
            _linter = new CsvLinter(schemas, enumerations);
        }
      
        /// <summary>
        /// Lint a CSV file against a predefined schema.
        /// </summary>
        /// <param name="file">The CSV file to lint.</param>
        /// <param name="schemaType">The type of schema to validate against. Example: 'EmployeeDetails'.</param>
        /// <returns>A list of linting issues or a success message.</returns>
        /// <response code="200">Returns the list of issues found or a success message if no issues were found.</response>
        /// <response code="400">If the file is null, empty, or schema type is not specified.</response>
        /// <response code="500">If there is an internal server error.</response>
        [HttpPost("lint")]
        public ActionResult<List<string>> LintCsv(IFormFile file, [FromQuery] string schemaType)
        {
            if (file == null)
            {
                return BadRequest("No file uploaded.");
            }

            if (string.IsNullOrEmpty(schemaType))
            {
                return BadRequest("Schema type must be specified.");
            }

            using (var stream = file.OpenReadStream())
            {
                var issues = _linter.LintCsv(stream, schemaType);
                if (issues.Count == 0)
                {
                    return Ok("No issues found.");
                }
                else
                {
                    return Ok(issues);
                }
            }
        }
    }
}