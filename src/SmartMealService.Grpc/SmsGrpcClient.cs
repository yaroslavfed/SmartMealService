using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;
using GrpcOrder = Sms.Test.Order;
using GrpcOrderItem = Sms.Test.OrderItem;
using SmsTestService = Sms.Test.SmsTestService;

namespace SmartMealService.Grpc;

public class SmsGrpcClient : ISmsGrpcClient
{
    private readonly SmsTestService.SmsTestServiceClient _client;

    public SmsGrpcClient(string address)
        : this(new SmsTestService.SmsTestServiceClient(
                   GrpcChannel.ForAddress(address))) { }

    public SmsGrpcClient(SmsTestService.SmsTestServiceClient client)
    {
        _client = client;
    }

    public async Task<List<MenuItem>> GetMenuAsync()
    {
        var response = await _client.GetMenuAsync(new BoolValue { Value = true });

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

    public async Task<bool> SendOrderAsync(Order order)
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

        var response = await _client.SendOrderAsync(request);

        if (!response.Success)
            throw new SmsApiException(response.ErrorMessage);

        return true;
    }
}