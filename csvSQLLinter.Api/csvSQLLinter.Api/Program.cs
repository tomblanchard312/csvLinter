using csvLinter.Api;

using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();

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
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyAllowAnyOrigin",
                      builder =>
                      {
                          builder.AllowAnyOrigin()   // Allow any origin
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
});
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
app.UseCors("MyAllowAnyOrigin");
app.Run();
