using System.Runtime.InteropServices;

namespace SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableChangeNotifier;

public sealed class WindowsEnvironmentVariableChangeNotifier : IEnvironmentVariableChangeNotifier
{
    private static readonly IntPtr BroadcastWindow = new(0xffff);
    private const int SettingChangeMessage = 0x001A;
    private const int AbortIfHung = 0x0002;

    public void NotifyChanged()
    {
        SendMessageTimeout(
            BroadcastWindow,
            SettingChangeMessage,
            UIntPtr.Zero,
            "Environment",
            AbortIfHung,
            5000,
            out _);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        int msg,
        UIntPtr wParam,
        string lParam,
        int fuFlags,
        int uTimeout,
        out UIntPtr lpdwResult);
}
