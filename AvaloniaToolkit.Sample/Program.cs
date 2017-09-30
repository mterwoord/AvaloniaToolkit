using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Logging.Serilog;
using Serilog;

namespace AvaloniaToolkit.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            SerilogLogger.Initialize(new LoggerConfiguration()
                                         .MinimumLevel.Warning()
                                         .WriteTo.Console(outputTemplate: "{Area}: {Message}\r\n")
                                         .CreateLogger());
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .Start<MainWindow>();

            Console.ReadLine();

        }
    }
}
