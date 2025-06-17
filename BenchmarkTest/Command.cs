using AdFetcherTools;
using CommandLine;
using log4net;
using log4net.Appender;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BenchmarkTest
{
    public class Command
    {
        public const int DefaultMaxWaitTimeBtwRequestInMsec = 2000;
        public const int DefaultClickUrlColumn = 9;

        public const int ReturnValue_CommandLineParseError  = -1;
        public const int ReturnValue_InputFileNotExist      = -2;
        public const int ReturnValue_ErrorOccurred          = -3;
        public const int ReturnValue_InvalidOutputFile      = -4;

        static readonly ILog log = LogManager.GetLogger(typeof(Command));

        public static void Main2(string[] args)
        {
            string line = null;
            using var queryListReader = new StreamReader(args[0]);
            var clickUrlList = new List<Tuple<string, string>>();
            Options options = new Options();
            for (int queryCount = 0; (line = queryListReader.ReadLine()) != null; queryCount++)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var clickUrl = line.Split(new char[] { '\t' })[1];
                    if (!string.IsNullOrWhiteSpace(clickUrl))
                    {
                        clickUrlList.Add(new Tuple<string, string>(clickUrl, line));
                    }
                }
            }
        }

        /// <summary>
        /// The console tool Main method.
        /// </summary>
        /// <param name="args">
        /// You can see how to set the command line arguments by executing this tool without setting 
        /// any arguments. In the source code, please see <see cref="Options"/> class to know what
        /// are the required parameters, .etc
        /// </param>
        public static void Main(string[] args)
        {
            // For showing Japanese texts on console
            Console.OutputEncoding = Encoding.Unicode;

            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

            var result = Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed<Options>(errors =>
                {
                    Environment.ExitCode = ReturnValue_CommandLineParseError;
                    return;
                })
                .WithParsed<Options>(options =>
                {
                    if (!File.Exists(options.YahooAdListFilePath))
                    {
                        Environment.ExitCode = ReturnValue_InputFileNotExist;
                        Console.Error.WriteLine($"Input file {options.YahooAdListFilePath} does not exist.");
                        return;
                    }

                    if (File.Exists(options.OutputSitelinkDataFilePath) || Directory.Exists(options.OutputSitelinkDataFilePath))
                    {
                        Environment.ExitCode = ReturnValue_InvalidOutputFile;
                        Console.Error.WriteLine($"Output file {options.OutputSitelinkDataFilePath} already exists or is a directory.");
                        return;
                    }

                    if (!string.IsNullOrEmpty(options.LogFilePath))
                    {
                        var log4netHierarchy = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();

                        foreach (var appender in log4netHierarchy.Root.Appenders)
                        {
                            if (appender is FileAppender)
                            {
                                var fileAppender = (FileAppender)appender;
                                fileAppender.File = options.LogFilePath;
                                fileAppender.ActivateOptions();
                                break;
                            }
                        }
                    }
                    
                    using (var queryListReader = new StreamReader(options.YahooAdListFilePath))
                    using (var outputSitelinkDataWriter = new StreamWriter(options.OutputSitelinkDataFilePath, false, Encoding.UTF8))
                    {
                        try
                        {
                            log.Info("The application started");

                            Console.WriteLine("Recommend to change the font used on this console to any font that has Japanese (or Chinese/Korean) glyphs");

                            int clickUrlColumn = ((options.ClickUrlColumn != null) ? options.ClickUrlColumn.Value : DefaultClickUrlColumn);
                            int maxWaitMsec = ((options.MaxWaitTimeInMsec != null) ? options.MaxWaitTimeInMsec.Value : DefaultMaxWaitTimeBtwRequestInMsec);
                            Execute(queryListReader, outputSitelinkDataWriter, clickUrlColumn, maxWaitMsec);
                        }
                        catch
                        {
                            Environment.ExitCode = ReturnValue_ErrorOccurred;
                            return;
                        }
                    }
                });
        }

        public static void Execute(
            StreamReader queryListStreamReader,
            StreamWriter outputSitelinkDataWriter,
            int clickUrlColumn,
            int maxWaitTimeInMsec)
        {
            string line = null;
            var clickUrlList = new List<Tuple<string,string>>();
            for (int queryCount = 0; (line = queryListStreamReader.ReadLine()) != null; queryCount++)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var clickUrl = line.Split(new char[] {'\t'})[clickUrlColumn];
                    if (!string.IsNullOrWhiteSpace(clickUrl))
                    {
                        clickUrlList.Add(new Tuple<string,string>(clickUrl,line));
                    }
                }
            }

            var random = new Random();
            int count = 1;

            var cookieManager = new CookieManager();

            var noRedirectHandler = new HttpClientHandler();
            noRedirectHandler.AllowAutoRedirect = false;

            var httpClientForAdLp = new HttpClient(noRedirectHandler);

            const int HttpClientTimeoutSeconds = 180;

            httpClientForAdLp.DefaultRequestHeaders.Add("User-Agent", AdRequest.AdRequestUserAgent);
            httpClientForAdLp.Timeout = TimeSpan.FromSeconds(HttpClientTimeoutSeconds);

            var adLandingPage = new AdLandingPage(httpClientForAdLp);

            int sleepMsec = maxWaitTimeInMsec;

            log.Info("Starting to process clickUrls...");
            foreach (var clickUrl in clickUrlList)
            {
                // Wait for random milliseconds (max {maxWaitTimeInMsec} msec)
                sleepMsec = random.Next(maxWaitTimeInMsec);
                log.Info($"Sleeping for {sleepMsec} milliseconds...");
                Thread.Sleep(sleepMsec);
                try
                {
                    adLandingPage.AddOrUpdateCookies(cookieManager);
                    var lpUrl = adLandingPage.GetDestinationUrl(clickUrl.Item1);
                    outputSitelinkDataWriter.WriteLine(clickUrl.Item2 + "\t" + lpUrl);
                    outputSitelinkDataWriter.Flush();
                }
                catch (HttpRequestException hre)
                {
                    log.Error($"HTTP Request exception. clickUrl {clickUrl} {RenderExceptionMessage(hre)}");
                    outputSitelinkDataWriter.WriteLine(clickUrl.Item2 + "\t");
                    outputSitelinkDataWriter.Flush();
                    // Go to next query
                }
                catch (TaskCanceledException tce)
                {
                    log.Error($"Task was canceled. clickUrl {clickUrl} {RenderExceptionMessage(tce)}");
                    outputSitelinkDataWriter.WriteLine(clickUrl.Item2 + "\t");
                    outputSitelinkDataWriter.Flush();
                    // Go to next query
                }
                catch (Exception e)
                {
                    log.Fatal($"Error occurred. Aborting the application. clickUrl {clickUrl} {RenderExceptionMessage(e)}");
                    throw e;
                }
            }

            log.Info($"Processed {count} queries. Normally exiting the application.");
        }

        //private static string FormatQueryToDisplayOnConsole(string query)
        //{
        //    if (string.IsNullOrEmpty(query))
        //    {
        //        return string.Empty;
        //    }

        //    if (query.ToCharArray().Any(c => c > 0x7F))
        //    {
        //        return $"{{{query}}} Encoded: {HttpUtility.UrlEncode(query)}"; 
        //    }
        //    else
        //    {
        //        return query;
        //    }
        //}

        private static string RenderExceptionMessage(Exception e)
        {
            return $"\nException: {e.GetType().FullName} {e.Message}\n{e.InnerException}\n{e.StackTrace}";
        }
    }

    class Options
    {
        [Value(0, MetaName = "YahooAdListFilePath", Required = true)]
        public string YahooAdListFilePath { get; set; }

        [Value(1, MetaName = "OutputSitelinkDataFilePath", Required = true)]
        public string OutputSitelinkDataFilePath { get; set; }

        [Option('c', MetaValue = "ClickUrl Column (0 origin) in the YahooAdListFilePath (9 by default)")]
        public int? ClickUrlColumn { get; set; }

        [Option('l', MetaValue = "Log file path")]
        public string LogFilePath { get; set; }

        [Option('w', MetaValue = "Max wait time (msec) between requests sent to Yahoo")]
        public int? MaxWaitTimeInMsec { get; set; }
    }
}
