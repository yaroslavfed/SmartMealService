using System.Windows;
using System.Windows.Controls;

namespace SmartMealService.Wpf.Views;

public partial class EnvironmentVariablesPanel : UserControl
{
    public EnvironmentVariablesPanel()
    {
        InitializeComponent();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this).WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this).Close();
    }
}
