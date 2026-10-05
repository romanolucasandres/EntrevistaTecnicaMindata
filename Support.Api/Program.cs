using System.Text.Json.Serialization;
using Support.Application;
using Support.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services
 .AddControllers()
 .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();
app.UseExceptionHandler(); // cualquier error inesperado → 500 con ProblemDetails, sin stack trace
if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
public partial class Program; //tests de integración
