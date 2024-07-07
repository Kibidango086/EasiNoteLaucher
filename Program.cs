using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool IsWindow(IntPtr hWnd);

    private static readonly string registryKeyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Seewo\EasiNote5";
    private static readonly string registryValueName = "ActualExePath";
    private static readonly TimeSpan checkInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan maxNoWindowDuration = TimeSpan.FromSeconds(5);

    static async Task Main(string[] args)
    {
        string processPath = GetEasiNotePath();
        if (string.IsNullOrEmpty(processPath))
        {
            Console.WriteLine("EasiNote path not found");
            return;
        }

        Process.Start(processPath);

        TimeSpan noWindowDuration = TimeSpan.Zero;
        string processName = "EasiNote"; 

        while (true)
        {
            await Task.Delay(checkInterval);

            var easiNoteProcesses = Process.GetProcessesByName(processName);
            bool windowExists = easiNoteProcesses.Any(p => IsWindow(p.MainWindowHandle));

            if (!windowExists)
            {
                noWindowDuration += checkInterval;

                if (noWindowDuration >= maxNoWindowDuration)
                {
                    foreach (var process in easiNoteProcesses)
                    {
                        process.Kill();
                    }
                    break;
                }
            }
            else
            {
                noWindowDuration = TimeSpan.Zero;
            }
        }
    }

    private static string GetEasiNotePath()
    {
        try
        {
            return (string)Registry.GetValue(registryKeyPath, registryValueName, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading registry: {ex.Message}");
            return null;
        }
    }
}
