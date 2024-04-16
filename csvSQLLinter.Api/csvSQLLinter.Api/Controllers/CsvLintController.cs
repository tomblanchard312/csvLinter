using csvSQLLinter.Api.Extensions;

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
            // Hardcoded schema for demonstration
            var schema = new Dictionary<string, SqlServerType>
        {
            { "EmployeeId", SqlServerType.Int },
            { "Name", SqlServerType.Nvarchar },
            { "HireDate", SqlServerType.DateTime },
            { "Salary", SqlServerType.Decimal }
        };

            _linter = new CsvLinter(schema);
        }

        [HttpPost("lint")]
        public ActionResult<List<string>> LintCsv(IFormFile file)
        {
            if (file == null) return BadRequest("No file uploaded.");

            using (var stream = file.OpenReadStream())
            {
                var issues = _linter.LintCsv(stream);
                if (issues.Count == 0)
                    return Ok("No issues found.");
                else
                    return Ok(issues);
            }
        }
    }
}
