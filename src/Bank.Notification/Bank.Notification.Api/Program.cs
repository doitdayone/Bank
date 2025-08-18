using Bank.Notification.Api.Application.Database;
using Bank.Notification.Api.Application.External.SendGridEmail;
using Bank.Notification.Api.Application.Features.Proccess;
using Bank.Notification.Api.Application.Handlers;
using Bank.Notification.Api.External.SendGridEmail;
using Bank.Notification.Api.External.ServiceBusReceive;
using Bank.Notification.Api.Persistence.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProcessHandler).Assembly));
builder.Services.AddHostedService<ServiceBusReceiveService>();
builder.Services.AddSingleton<ISendGridEmailService, SendGridEmailService>();

var app = builder.Build();

app.Run();

