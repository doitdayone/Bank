using Bank.Transaction.Api.Application.Database;
using Bank.Transaction.Api.Application.External.ServiceBusSender;
using Bank.Transaction.Api.Application.Features.Process;
using Bank.Transaction.Api.Application.Handlers;
using Bank.Transaction.Api.External.ServiceBusReceive;
using Bank.Transaction.Api.External.ServiceBusSender;
using Bank.Transaction.Api.Persistence.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseService>(options =>
    options.UseSqlServer(builder.Configuration["TRANSACTIONSQLDBCONSTR"]));
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProcessHandler).Assembly));
builder.Services.AddHostedService<ServiceBusReceiveService>();
builder.Services.AddSingleton<IServiceBusSenderService, ServiceBusSenderService>();

var app = builder.Build();

app.Run();
