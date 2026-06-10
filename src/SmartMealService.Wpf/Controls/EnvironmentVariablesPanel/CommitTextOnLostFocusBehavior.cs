using System.Windows;
using System.Windows.Controls;

namespace SmartMealService.Wpf.Controls.EnvironmentVariablesPanel;

public static class CommitTextOnLostFocusBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(CommitTextOnLostFocusBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
    {
        if (element is not TextBox textBox)
            return;

        if ((bool)args.NewValue)
            textBox.LostFocus += TextBox_LostFocus;
        else
            textBox.LostFocus -= TextBox_LostFocus;
    }

    private static void TextBox_LostFocus(object sender, RoutedEventArgs args)
    {
        if (sender is TextBox textBox)
            textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }
}
