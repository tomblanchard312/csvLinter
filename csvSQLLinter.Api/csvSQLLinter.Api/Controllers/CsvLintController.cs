using csvSQLLinter.Api.Extensions;
using csvSQLLinter.Api;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.IO;
namespace csvSQLLinter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CsvLintController : ControllerBase
    {
        private readonly CsvLinter _linter;

        public CsvLintController()
        {
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