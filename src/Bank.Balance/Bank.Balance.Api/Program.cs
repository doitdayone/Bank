using Bank.Balance.Api.Application.Database;
using Bank.Balance.Api.Application.Features.Process;
using Bank.Balance.Api.Application.Handlers;
using Bank.Balance.Api.External.ServiceBusReceive;
using Bank.Balance.Api.Persistence.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseService>(options =>
    options.UseSqlServer(builder.Configuration["BALANCESQLDBCONSTR"]));
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProcessHandler).Assembly));
builder.Services.AddHostedService<ServiceBusReceiveService>();

var app = builder.Build();

app.Run();

