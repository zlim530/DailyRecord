using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace BenchmarkTest;

public class NewProgram
{
    public static async Task Main2(string[] args)
    {
        string inputFile = args[0];
        string outputFile = args[1];
        int maxWaitTimeMs = args.Length > 2 ? int.Parse(args[2]) : 2000;
        int index = args.Length > 3 ? int.Parse(args[3]) : 9;
        var urls = await File.ReadAllLinesAsync(inputFile);
        using var outputWriter = new StreamWriter(outputFile, false, Encoding.UTF8);
    }

    public static async Task Main(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<NewProgram>();

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: <inputFile> <outputFile> [maxWaitTimeMs=2000]");
            return;
        }

        string inputFile = args[0];
        string outputFile = args[1];
        int maxWaitTimeMs = args.Length > 2 ? int.Parse(args[2]) : 2000;
        int index = args.Length > 3 ? int.Parse(args[3]) : 9;

        if (!File.Exists(inputFile))
        {
            logger.LogError("Input file does not exist: {InputFile}", inputFile);
            return;
        }

        var urls = await File.ReadAllLinesAsync(inputFile);
        using var outputWriter = new StreamWriter(outputFile, false, Encoding.UTF8);

        using var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(180)
        };

        var tasks = urls.Select(async (line, index) =>
        {
            try
            {
                var columns = line.Split('\t');
                if (columns.Length <= 9)
                {
                    logger.LogWarning("Invalid line format at {Index}", index);
                    return;
                }

                string clickUrl = columns[index];
                if (string.IsNullOrWhiteSpace(clickUrl))
                {
                    logger.LogWarning("Empty click URL at {Index}", index);
                    return;
                }

                await Task.Delay(Random.Shared.Next(maxWaitTimeMs));

                var response = await httpClient.GetAsync(clickUrl);
                string landingPageUrl = response.Headers.Location?.ToString() ?? clickUrl;

                string outputLine = string.Join('\t', columns) + "\t" + landingPageUrl;
                await outputWriter.WriteLineAsync(outputLine);
                logger.LogInformation("Processed URL at {Index}: {LandingPageUrl}", index, landingPageUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing line at {Index}", index);
            }
        });

        await Task.WhenAll(tasks);
        logger.LogInformation("Processing complete. Results written to {OutputFile}", outputFile);
    }
}
