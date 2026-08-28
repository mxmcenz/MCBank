using MCBank.WebApi.Application;
using MCBank.WebApi.Core.Interfaces;
using MCBank.WebApi.Infrastructure;
using MCBank.WebApi.Infrastructure.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStorage, JsonStorage>();
builder.Services.AddSingleton<IBankService, BankService>();
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