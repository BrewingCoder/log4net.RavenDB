using log4net.Config;
using log4net.RavenDB;
using Microsoft.Extensions.Configuration;
using Moq;
using Raven.Client.Documents.Session;
using Raven.Client.Documents;
using FizzWare.NBuilder;
using log4net.Core;
using Nito.AsyncEx;

namespace log4net.RaveDB.Tests
{
    public class RavenDbAppenderTests
    {
        private Lazy<string> _serverUrl = new();
        private readonly Mock<IDocumentStore> _mockIDocumentStore;
        private readonly Mock<IDocumentSession> _mockIDocumentSession;

        public RavenDbAppenderTests()
        {
            _mockIDocumentStore = new Mock<IDocumentStore>();
            _mockIDocumentStore.SetupAllProperties();
            _mockIDocumentSession = new Mock<IDocumentSession>();
            _mockIDocumentSession.SetupAllProperties();
            _mockIDocumentStore.Setup(m => m.OpenSession()).Returns(_mockIDocumentSession.Object);
        }

        protected string ServerUrl
        {
            get
            {
                if (!_serverUrl.IsValueCreated) {
                    var configuration = new ConfigurationBuilder().AddUserSecrets<RavenDbAppenderTests>().Build();
                    _serverUrl = new Lazy<string>(configuration["RavenDBServer"]);
                }
                return _serverUrl.Value;
            }
        }

        

        [Fact]
        public void TestIntegrationWithLog4Net()
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            var logger = LogManager.GetLogger(typeof(RavenDbAppenderTests));
            var appenders = logger.Logger.Repository.GetAppenders();
            
            Assert.Single(appenders);
            Assert.Equal("RavenDBAppender", appenders[0].Name);
        }

        [Fact]
        public void TestLogThrowsExceptionOnNullEvent()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var myLog = new Log(null);
                Assert.IsType<string>(myLog.Level);
            });


        }

        [Fact]
        public void TestInstantiation()
        {
            var appender = new RavenDBAppender(_mockIDocumentStore.Object);
            Assert.Same(_mockIDocumentStore.Object,appender.DocStore);
        }

        [Fact]
        public void TestActivateWithProperParameters()
        {
            var appender = new RavenDBAppender(_mockIDocumentStore.Object)
            {
                Url = "http://localhost:8080", Database = "AppenderLogTest"
            };


            appender.ActivateOptions();
            _mockIDocumentStore.Verify(m=>m.Initialize(),Times.AtLeastOnce);
        }

        [Fact]
        public void TestActivateWithMissingUrl()
        {
            var appender = new RavenDBAppender(_mockIDocumentStore.Object)
            {
                Database = "AppenderLogTest"
            };
            Assert.Throws<InvalidOperationException>(() =>
            {
                appender.ActivateOptions();
            });
            
        }

        [Fact]
        public void TestActivateWithMissingDatabaseName()
        {
            var appender = new RavenDBAppender(_mockIDocumentStore.Object)
            {
                Url = "http://localhost:8080"
            };
            Assert.Throws<InvalidOperationException>(() =>
            {
                appender.ActivateOptions();
            });
        }

        private static LoggingEvent MakeLoggingEvent()
        {
            var data = Builder<LoggingEventData>.CreateNew().Build();
            var results = new LoggingEvent(data);
            return results;
        }
        
        [Fact]
        public void TestSendBuffer()
        {
            var lEvent = Builder<LoggingEvent>.CreateNew()
                .WithFactory(MakeLoggingEvent)
                .Build();
                
            var events = new LoggingEvent[1];
            events[0] = lEvent;

            var appender = new RavenDBAppender(_mockIDocumentStore.Object)
            {
                Url = "http://localhost:8080",
                Database = "AppenderLogTest"
            };


            AsyncContext.Run(() =>
            {
                appender.LogEvents(events);
            });

            _mockIDocumentSession.Verify(m=>m.Store(It.IsAny<Log>()),Times.Exactly(events.Length));
            _mockIDocumentSession.Verify(m=>m.SaveChanges(),Times.Exactly(events.Length));
            _mockIDocumentSession.Verify(m=>m.Dispose(),Times.Once);

        }

        [Fact]
        public void TestSendBufferWithZeroLenArray()
        {
            var events = Array.Empty<LoggingEvent>();
            var appender = new RavenDBAppender(_mockIDocumentStore.Object)
            {
                Url = "http://localhost:8080",
                Database = "AppenderLogTest"
            };

            AsyncContext.Run(() =>
            {
                appender.LogEvents(events);
            });

            _mockIDocumentSession.Verify(m => m.Store(It.IsAny<Log>()), Times.Never);
            _mockIDocumentSession.Verify(m => m.SaveChanges(), Times.Never);
            _mockIDocumentSession.Verify(m => m.Dispose(), Times.Never);
        }

        [Fact]
        public void TestSendBufferWithNullParam()
        {
            var appender = new RavenDBAppender(_mockIDocumentStore.Object)
            {
                Url = "http://localhost:8080",
                Database = "AppenderLogTest"
            };

            AsyncContext.Run(() =>
            {
                appender.LogEvents(null);
            });

            _mockIDocumentSession.Verify(m => m.Store(It.IsAny<Log>()), Times.Never);
            _mockIDocumentSession.Verify(m => m.SaveChanges(), Times.Never);
            _mockIDocumentSession.Verify(m => m.Dispose(), Times.Never);
        }
    }
}