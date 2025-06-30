
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class ServiceBusQueueService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly string _queueName;

    public ServiceBusQueueService(IConfiguration configuration)
    {
        var connectionString = configuration["ServiceBus:ConnectionString"];
        _queueName = configuration["ServiceBus:QueueName"];

        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(_queueName);
    }

    public async Task SendMessageAsync(string message)
    {
        ServiceBusMessage busMessage = new ServiceBusMessage(message);
        await _sender.SendMessageAsync(busMessage);
    }

    public async Task<string> ReceiveMessageAsync()
    {
        var receiver = _client.CreateReceiver(_queueName);
        var message = await receiver.ReceiveMessageAsync();

        if (message != null)
        {
            await receiver.CompleteMessageAsync(message);
            return message.Body.ToString();
        }

        return null;
    }
}
