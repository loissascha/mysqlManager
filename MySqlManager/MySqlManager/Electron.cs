using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MySqlManager;

public class Electron
{
    private int _processId;
    private int _mainProcessId;
    
    public void Start(string address, int mainProcessId)
    {
        _mainProcessId = mainProcessId;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var process = new Process();
            process.StartInfo.FileName = "electron-linux/blazorelectronapp";
            process.StartInfo.Arguments = "--url=" + address;
            process.Start();
            _processId = process.Id;
            Console.WriteLine("ProcessID for UI: " + _processId);
            RunTimer();
        }
        // else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        // {
        //     var process = new Process();
        //     process.StartInfo.FileName = "electron-windows/blazorelectronapp.exe";
        //     process.StartInfo.Arguments = "--url=" + address;
        //     process.Start();
        //     _processId = process.Id;
        //     Console.WriteLine("ProcessID for UI: " + _processId);
        //     RunTimer();
        //     //Console.WriteLine("No UI for Windows available yet! Open in Browser: " + address);
        // }
        else
        {
            Console.WriteLine("No UI for this platform available! Open in Browser: " + address);
        }
    }

    private async void RunTimer()
    {
        while (true)
        {
            var stopTimer = false;
            await Task.Run(async () =>
            {
                await Task.Delay(500);
                try
                {
                    var process = Process.GetProcessById(_processId);
                }
                catch (ArgumentException)
                {
                    try
                    {
                        Console.WriteLine("Stopping main processs..");
                        var mainProcess = Process.GetProcessById(_mainProcessId);
                        mainProcess.Kill();
                        stopTimer = true;
                    }
                    catch (ArgumentException)
                    {
                        stopTimer = true;
                    }
                }
            });
            if (stopTimer) return;
        }
    }
}