using MCBank.WebApi.Infrastructure;
using MCBank.WebApi.Infrastructure.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();