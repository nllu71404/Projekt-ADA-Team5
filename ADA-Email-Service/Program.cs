using ADA_EmailConsumer;
using ADA_EmailConsumer.Consumers;
using ADA_EmailConsumer.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<AssessmentCreatedConsumer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();