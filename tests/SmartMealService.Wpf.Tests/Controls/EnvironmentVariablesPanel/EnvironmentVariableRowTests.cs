using FluentAssertions;
using SmartMealService.Wpf.Controls.EnvironmentVariablesPanel;

namespace SmartMealService.Wpf.Tests.Controls.EnvironmentVariablesPanel;

public class EnvironmentVariableRowTests
{
    [Fact]
    public void Value_ShouldRaisePropertyChanged_WhenChanged()
    {
        var row = new EnvironmentVariableRow("SMS_HTTP_BASE_URL", "http://localhost:5000", "Адрес SMS HTTP-сервера");
        var changedProperties = new List<string?>();
        row.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        row.Value = "http://localhost:5001";

        changedProperties.Should().Contain(nameof(EnvironmentVariableRow.Value));
    }

    [Fact]
    public void Value_ShouldSupportLongText()
    {
        var row = new EnvironmentVariableRow("SMS_LONG_VALUE", "", "");
        var longValue = new string('x', 10_000);

        row.Value = longValue;

        row.Value.Should().Be(longValue);
    }
}
