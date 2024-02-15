using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Optix.Movies.Infrastructure.Configuration;
using Optix.Movies.Infrastructure.Database;
using Optix.Movies.Infrastructure.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<MoviesQuery>();

builder.Services.Configure<PageDefaults>(builder.Configuration);
var dbConnection = builder.Configuration.GetConnectionString("DbConnection");
builder.Services.AddDbContext<MoviesContext>(options =>
{
    options.UseSqlServer(dbConnection);
});

builder.Services.AddLogging();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.Load("Optix.Movies.Application")));

builder.Services.AddControllers().AddNewtonsoftJson(options =>
        options.SerializerSettings.Converters.Add(new StringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGenNewtonsoftSupport();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
