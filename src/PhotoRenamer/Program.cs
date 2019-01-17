namespace PhotoRenamer
{
    using CommandLine;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;

    class Program
    {
        readonly ILogger _logger;
        readonly ServiceProvider serviceProvider;

        // Referred to the following sample code.
        // https://github.com/aspnet/Logging/blob/master/samples/SampleApp/Program.cs
        public Program(CommandLineOptions options = null)
        {
            // A Web App based program would configure logging via the WebHostBuilder.
            // Create a logger factory with filters that can be applied across all logger providers.
            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddConsole();

                    if (options != null && options.Verbose)
                    {
                        builder.AddFilter("PhotoRenamer", LogLevel.Trace);
                    }
                    else
                    {
                        builder.AddFilter("PhotoRenamer", LogLevel.Information);
                    }
                })
                .AddSingleton<IPhotoRenameService, PhotoRenameService>();

            // providers may be added to a LoggerFactory before any loggers are created
            serviceProvider = serviceCollection.BuildServiceProvider();

            // getting the logger using the class's name is conventional
            _logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithNotParsed(errors => new Program().ProcessErrors(errors))
                .WithParsed(options => new Program(options).RunService(options));
        }

        public void ProcessErrors(IEnumerable<Error> errors)
        {
            foreach (var item in errors)
            {
                _logger.LogError(item.ToString());
            }
        }

        public void RunService(CommandLineOptions options)
        {
            _logger.LogTrace("Start service");
            var service = serviceProvider.GetRequiredService<IPhotoRenameService>();
            service.Run(options);

            if (options.Verbose)
            {
                _logger.LogTrace("Press any key to close the window...");
                Console.ReadLine();
            }
        }
    }
}
