using System;
using Castle.Core.Logging;
using FizzWare.NBuilder;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace log4net.RavenDB.Tests
{
    public class LogItem
    {
        public LoggerLevel Level { get; set; }
        public string Message { get; set; }
    }
 

    /// <summary>
    /// live test for benefit of proving appender can handle mass logging throughput
    /// </summary>
    [TestClass]
    public class TestRavenDBAppenderLive
    {
        

        readonly RandomGenerator _gen = new RandomGenerator(new Random());
        
        [TestMethod]
        public void AddTenItems()
        {
            XmlConfigurator.Configure();
            var log = LogManager.GetLogger(typeof(TestRavenDBAppenderLive));


            var messages = Builder<LogItem>
                .CreateListOfSize(10)
                .All()
                .With(c=>c.Message = Faker.Lorem.Sentence())
                .With(c=>c.Level = _gen.Enumeration<LoggerLevel>())
                .Build();

            foreach (var message in messages)
            {
                switch (message.Level)
                {
                    case LoggerLevel.Debug:
                        log.Debug(message.Message);
                        break;
                    case LoggerLevel.Error:
                        log.Error(message.Message);
                        break;
                    case LoggerLevel.Fatal:
                        log.Fatal(message.Message);
                        break;
                    case LoggerLevel.Info:
                        log.Info(message.Message);
                        break;
                    case LoggerLevel.Off:
                        break;
                    case LoggerLevel.Warn:
                        log.Info(message.Message);
                        break;
                }
            }
        }
    }
}
