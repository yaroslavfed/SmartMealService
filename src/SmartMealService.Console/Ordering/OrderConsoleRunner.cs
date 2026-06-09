using SmartMealService.Console.ConsoleIO;
using SmartMealService.Console.Exceptions;
using SmartMealService.Console.Input;
using SmartMealService.Console.Persistence;
using SmartMealService.Shared.Abstractions;
using SmartMealService.Shared.Exceptions;
using SmartMealService.Shared.Models;

namespace SmartMealService.Console.Ordering;

public class OrderConsoleRunner(
    ISmsClient smsClient,
    IMenuRepository menuRepository,
    IConsoleIO console)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        List<MenuItem> menuItems;
        try
        {
            await menuRepository.InitializeAsync(cancellationToken);
            menuItems = await smsClient.GetMenuAsync(cancellationToken);
            await menuRepository.SaveMenuAsync(menuItems, cancellationToken);
        }
        catch (SmsApiException ex)
        {
            console.WriteLine(ex.Message);
            return;
        }
        catch (Exception ex)
        {
            WriteInfrastructureError(ex);
            return;
        }

        WriteMenu(menuItems);

        var order = ReadOrder(menuItems);
        await SendOrderAsync(order, cancellationToken);
    }

    private void WriteMenu(IEnumerable<MenuItem> menuItems)
    {
        foreach (var item in menuItems)
            console.WriteLine($"{item.Name} - {item.Id} ({item.Article}) - {item.Price}");
    }

    private Order ReadOrder(IReadOnlyCollection<MenuItem> menuItems)
    {
        var order = new Order();

        while (true)
        {
            console.WriteLine("Введите блюда в формате Код1:Количество1;Код2:Количество2");
            var input = console.ReadLine();

            try
            {
                order.Items.AddRange(OrderInputParser.Parse(input, menuItems));
                return order;
            }
            catch (OrderInputException ex)
            {
                console.WriteLine(ex.Message);
            }
        }
    }

    private async Task SendOrderAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            await smsClient.SendOrderAsync(order, cancellationToken);
            console.WriteLine("УСПЕХ");
        }
        catch (SmsApiException ex)
        {
            console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            WriteInfrastructureError(ex);
        }
    }

    private void WriteInfrastructureError(Exception exception)
    {
        console.WriteLine($"Ошибка выполнения: {exception.Message}");
    }
}
