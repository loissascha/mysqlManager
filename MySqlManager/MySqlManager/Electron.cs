using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
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
    
    public static int GetAvailablePort(int startingPort)
    {
        var portArray = new List<int>();
        for (var i = startingPort; i < startingPort + 100; i++)
        {
            portArray.Add(i);
        }

        IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] endPoints = ipGlobalProperties.GetActiveTcpListeners();

        foreach (var endPoint in endPoints)
        {
            if (portArray.Contains(endPoint.Port))
                portArray.Remove(endPoint.Port);
        }

        return portArray.First();
    }
}