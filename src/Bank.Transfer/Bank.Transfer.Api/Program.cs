using Bank.Transfer.Api.Application.Database;
using Bank.Transfer.Api.Application.External.ServiceBusSender;
using Bank.Transfer.Api.Application.Features.Process;
using Bank.Transfer.Api.Application.Handlers;
using Bank.Transfer.Api.External.ServiceBusReceive;
using Bank.Transfer.Api.External.ServiceBusSender;
using Bank.Transfer.Api.Persistence.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DatabaseService>(options =>
    options.UseSqlServer(builder.Configuration["TRANSFERSQLDBCONSTR"]));
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProcessHandler).Assembly));
builder.Services.AddHostedService<ServiceBusReceiveService>();
builder.Services.AddSingleton<IServiceBusSenderService, ServiceBusSenderService>();

var app = builder.Build();

app.Run();

