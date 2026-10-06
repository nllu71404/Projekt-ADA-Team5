using ADA_EmailConsumer;
using ADA_EmailConsumer.Consumers;
using ADA_EmailConsumer.Services;
using Resend;

var builder = Host.CreateApplicationBuilder(args);

var resendApiKey = builder.Configuration["RESEND_API_KEY"];

Console.WriteLine(
    $"RESEND_API_KEY found: {!string.IsNullOrEmpty(resendApiKey)}");

builder.Services.AddHttpClient<ResendClient>();

builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = resendApiKey!;
});

builder.Services.AddTransient<IResend, ResendClient>();

builder.Services.AddScoped<IEmailService, TestEmailService>(); //Kan ændres til "EmailService", når vi vil sende rigtige emails 
builder.Services.AddScoped<AssessmentCreatedConsumer>();
builder.Services.AddScoped<EmailScheduledConsumer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();