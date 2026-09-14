using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ADA_Contracts.Events;

// Her opretter vi en forbindelse til RabbitMQ-serveren og opretter en kanal til at lytte på "assessment-created" køen. 
//Vi bruger AsyncEventingBasicConsumer til at modtage beskeder asynkront.
var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "assessment-created",
    durable: true,
    exclusive: false,
    autoDelete: false);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (sender, eventArgs) =>
{
    var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

    var assessmentCreated =
        JsonSerializer.Deserialize<AssessmentCreated>(json);

    if (assessmentCreated is not null)
    {
        Console.WriteLine("=================================");
        Console.WriteLine("AssessmentCreated received!");
        Console.WriteLine("=================================");

        Console.WriteLine(
            $"Assessment ID: {assessmentCreated.AssessmentId}");

        Console.WriteLine(
            $"Assessment Name: {assessmentCreated.AssessmentName}");

        Console.WriteLine(
            $"Start Date: {assessmentCreated.StartDate}");

        Console.WriteLine(
            $"End Date: {assessmentCreated.EndDate}");

        Console.WriteLine();
    }

    await Task.CompletedTask;
};

await channel.BasicConsumeAsync(
    queue: "assessment-created",
    autoAck: true,
    consumer: consumer);

Console.WriteLine("Waiting for AssessmentCreated events...");
Console.WriteLine("Press [Enter] to exit.");

await Task.Run(() => Console.ReadLine());
