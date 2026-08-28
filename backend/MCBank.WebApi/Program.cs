using MCBank.WebApi.Application;
using MCBank.WebApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FileManager>();
builder.Services.AddSingleton<BankService>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();