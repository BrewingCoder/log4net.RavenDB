using System;
using System.Diagnostics;
using FizzWare.NBuilder;
using log4net;
using log4net.Config;

namespace Log4net.RavenDB.LoadTest
{
    public enum LogLevel
    {
        Debug = 0,
        Error,
        Fatal,
        Info,
        Warn
    }
    public class LogItem
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }
    }

    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private static readonly RandomGenerator Gen = new RandomGenerator(new Random());
        

        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            Console.WriteLine("Building 10K random log events in memory");
            var messages = Builder<LogItem>
                .CreateListOfSize(10000)
                .All()
                .With(c => c.Message = Faker.Lorem.Sentence())
                .With(c => c.Level = Gen.Enumeration<LogLevel>())
                .Build();
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Beginning Log4Net Logging of items.");
            stopwatch.Start();
            foreach (var message in messages)
            {
                switch (message.Level)
                {
                    case LogLevel.Debug:
                        Log.Debug(message.Message);
                        break;
                    case LogLevel.Error:
                        Log.Error(message.Message);
                        break;
                    case LogLevel.Fatal:
                        Log.Fatal(message.Message);
                        break;
                    case LogLevel.Info:
                        Log.Info(message.Message);
                        break;
                    case LogLevel.Warn:
                        Log.Warn(message.Message);
                        break;
                }
            }
            stopwatch.Stop();
            Console.WriteLine($"Wrote 10,000 log events in {stopwatch.Elapsed}");
            Console.WriteLine("Press any key...");
            Console.ReadKey();

        }
    }
}
