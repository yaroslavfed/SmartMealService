using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using SmartMealService.Grpc.Mapping;
using SmartMealService.Shared.Abstractions;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;
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

        return response.MenuItems.Select(GrpcMenuItemMapper.ToModel).ToList();
    }

    public async Task<bool> SendOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        var response = await _client.SendOrderAsync(
            GrpcOrderMapper.ToGrpcOrder(order),
            cancellationToken: cancellationToken);

        if (!response.Success)
            throw new SmsApiException(response.ErrorMessage);

        return true;
    }
}
