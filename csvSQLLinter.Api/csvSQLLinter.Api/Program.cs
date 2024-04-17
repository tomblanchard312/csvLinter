using csvSQLLinter.Api;

using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// Configure schemas and enumerations here
var schemas = new Dictionary<string, Dictionary<string, csvSQLLinter.Api.Extensions.SqlServerType>>
{
    {"EmployeeDetails", new Dictionary<string, csvSQLLinter.Api.Extensions.SqlServerType>
    {
        {"EmployeeId", csvSQLLinter.Api.Extensions.SqlServerType.Int},
        {"Name", csvSQLLinter.Api.Extensions.SqlServerType.Nvarchar},
        {"HireDate", csvSQLLinter.Api.Extensions.SqlServerType.DateTime},
        {"Salary", csvSQLLinter.Api.Extensions.SqlServerType.Decimal},
        {"Salutation", csvSQLLinter.Api.Extensions.SqlServerType.Varchar},
        {"Gender", csvSQLLinter.Api.Extensions.SqlServerType.Varchar}
    }},
    {"Addresses", new Dictionary<string, csvSQLLinter.Api.Extensions.SqlServerType>
    {
        {"EmployeeId", csvSQLLinter.Api.Extensions.SqlServerType.Int},
        {"Address", csvSQLLinter.Api.Extensions.SqlServerType.Varchar}
    }},
    {"Passport", new Dictionary<string, csvSQLLinter.Api.Extensions.SqlServerType>
    {
        {"EmployeeId", csvSQLLinter.Api.Extensions.SqlServerType.Int},
        {"PassportID", csvSQLLinter.Api.Extensions.SqlServerType.Nvarchar},
        {"PassportCountry", csvSQLLinter.Api.Extensions.SqlServerType.Nvarchar},
        {"PassportIssuingAuthority", csvSQLLinter.Api.Extensions.SqlServerType.Nvarchar},
        {"DateOfBirth", csvSQLLinter.Api.Extensions.SqlServerType.DateTime},
        {"Gender", csvSQLLinter.Api.Extensions.SqlServerType.Varchar}
    }}
};

var enumerations = new Dictionary<string, Dictionary<string, string[]>>
{
    {"EmployeeDetails", new Dictionary<string, string[]>
    {
        {"Gender", new[] {"male", "female", "x", "u"}}
    }},
    {"Passport", new Dictionary<string, string[]>
    {
        {"Gender", new[] {"male", "female", "x", "u"}}
    }}
};
// Add services to the container.
builder.Services.AddControllers();

// Add the CsvLinter to the DI container
builder.Services.AddSingleton<CsvLinter>(provider => new CsvLinter(schemas, enumerations));

// Configure Swagger to include generated XML comments and custom API information
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "CSV Linter API",
        Description = "An API for linting CSV files against predefined schemas.",
        TermsOfService = new Uri("/docs/terms.txt", UriKind.Relative),
        Contact = new OpenApiContact
        {
            Name = "Example Support",
            Email = "support@example.com",
            Url = new Uri("https://support.example.com")
        },
        License = new OpenApiLicense
        {
            Name = "Apache 2.0 License",
            Url = new Uri("/docs/LICENSE.txt", UriKind.Relative)
        }
    });

    // Including XML comments
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
// Configure static files to be served from the 'docs' directory
builder.Services.AddDirectoryBrowser();

var app = builder.Build();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
    Path.Combine(builder.Environment.ContentRootPath, "Docs")),
    RequestPath = "/docs"
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CSV Linter API V1");
    });
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
