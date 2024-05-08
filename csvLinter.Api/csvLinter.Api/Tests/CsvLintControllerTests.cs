using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using csvLinter.Api.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

namespace csvLinter.Api.Tests
{
    public class CsvLintControllerTests
    {
        [Fact]
        public void LintCsv_ValidCsv_ReturnsOkResult()
        {
            // Arrange
            var csvContent = "1,John,Doe,john@example.com,1990-01-01";
            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
            var formFile = new FormFile(csvStream, 0, csvStream.Length, "csvFile", "example.csv");

            var controller = new CsvLintController();

            // Act
            var result = controller.LintCsv(formFile, "EmployeeDetails");

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void LintCsv_NoFileUploaded_ReturnsBadRequestResult()
        {
            // Arrange
            var controller = new CsvLintController();

            // Act
            var result = controller.LintCsv(null, "EmployeeDetails");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded.", (result as BadRequestObjectResult).Value.ToString());
        }
    }
}