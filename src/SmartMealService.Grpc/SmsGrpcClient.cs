using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using SmartMealService.Shared.Abstractions;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;
using GrpcOrder = Sms.Test.Order;
using GrpcOrderItem = Sms.Test.OrderItem;
using SmsTestService = Sms.Test.SmsTestService;

namespace SmartMealService.Grpc;

public class SmsGrpcClient : ISmsClient
{
    private readonly SmsTestService.SmsTestServiceClient _client;

    public SmsGrpcClient(string address)
        : this(new SmsTestService.SmsTestServiceClient(GrpcChannel.ForAddress(address)))
    {
    }

    internal SmsGrpcClient(SmsTestService.SmsTestServiceClient client)
    {
        _client = client;
    }

    public async Task<List<MenuItem>> GetMenuAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetMenuAsync(new BoolValue { Value = true }, cancellationToken: cancellationToken);

        if (!response.Success)
            throw new SmsApiException(response.ErrorMessage);

        return response.MenuItems.Select(item => new MenuItem
        {
            Id = item.Id,
            Article = item.Article,
            Name = item.Name,
            Price = (decimal)item.Price,
            IsWeighted = item.IsWeighted,
            FullPath = item.FullPath,
            Barcodes = [..item.Barcodes]
        }).ToList();
    }

    public async Task<bool> SendOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        var request = new GrpcOrder
        {
            Id = order.Id,
            OrderItems =
            {
                order.Items.Select(i => new GrpcOrderItem
                {
                    Id = i.Id,
                    Quantity = i.Quantity
                })
            }
        };

        var response = await _client.SendOrderAsync(request, cancellationToken: cancellationToken);

        if (!response.Success)
            throw new SmsApiException(response.ErrorMessage);

        return true;
    }
}
