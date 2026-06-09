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
        await menuRepository.InitializeAsync(cancellationToken);

        List<MenuItem> menuItems;
        try
        {
            menuItems = await smsClient.GetMenuAsync(cancellationToken);
        }
        catch (SmsApiException ex)
        {
            console.WriteLine(ex.Message);
            return;
        }

        await menuRepository.SaveMenuAsync(menuItems, cancellationToken);
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
            console.WriteLine("\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u0431\u043b\u044e\u0434\u0430 \u0432 \u0444\u043e\u0440\u043c\u0430\u0442\u0435 \u041a\u043e\u04341:\u041a\u043e\u043b\u0438\u0447\u0435\u0441\u0442\u0432\u043e1;\u041a\u043e\u04342:\u041a\u043e\u043b\u0438\u0447\u0435\u0441\u0442\u0432\u043e2");
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
            console.WriteLine("\u0423\u0421\u041f\u0415\u0425");
        }
        catch (SmsApiException ex)
        {
            console.WriteLine(ex.Message);
        }
    }
}
