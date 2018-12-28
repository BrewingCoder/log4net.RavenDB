using System;
using FizzWare.NBuilder;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net.Core;
using Moq;
using Nito.AsyncEx;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace log4net.RavenDB.Tests
{
    [TestClass]
    public class TestRavenDBAppender
    {
        private Mock<IDocumentStore> _mockIDocumentStore;
        private Mock<IDocumentSession> _mockIDocumentSession;

        [TestMethod]
        public void TestIntegrationWithLog4Net()
        {

            XmlConfigurator.Configure();
            var logger = LogManager.GetLogger(typeof(TestRavenDBAppender));
            var appenders = logger.Logger.Repository.GetAppenders();

            Assert.AreEqual(1, appenders.Length);
            Assert.AreEqual("RavenDBAppender", appenders[0].Name);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestLogThrowsExceptionOnNullEvent()
        {
            var myLog = new Log(null);
        }

        [TestInitialize]
        public void Initialize()
        {
            _mockIDocumentStore = new Mock<IDocumentStore>();
            _mockIDocumentStore.SetupAllProperties();

            _mockIDocumentSession = new Mock<IDocumentSession>();
            _mockIDocumentSession.SetupAllProperties();

            _mockIDocumentStore.Setup(m => m.OpenSession()).Returns(_mockIDocumentSession.Object);
        }

        [TestMethod]
        public void TestInstantiation()
        {
            var appender = new RavenDBAppender(_mockIDocumentStore.Object);
            Assert.AreSame(_mockIDocumentStore.Object,appender.DocStore);
        }

        [TestMethod]
        public void TestActivateWithProperParameters()
        {
            var appender = new RavenDBAppender(_mockIDocumentStore.Object)
            {
                Url = "http://localhost:8080", Database = "AppenderLogTest"
            };


            appender.ActivateOptions();
            _mockIDocumentStore.Verify(m=>m.Initialize(),Times.AtLeastOnce);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Database connection parameter 'Url' is not specified.")]
        public void TestActivateWithMissingUrl()
        {
            var appender = new RavenDBAppender(_mockIDocumentStore.Object)
            {
                Database = "AppenderLogTest"
            };
            appender.ActivateOptions();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Database name parameter 'Database' is not specified.")]
        public void TestActivateWithMissingDatabaseName()
        {
            var appender = new RavenDBAppender(_mockIDocumentStore.Object)
            {
                Url = "http://localhost:8080"
            };
            appender.ActivateOptions();
        }

        private static LoggingEvent MakeLoggingEvent()
        {
            var data = Builder<LoggingEventData>.CreateNew().Build();
            var results = new LoggingEvent(data);
            return results;
        }

        [TestMethod]
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

        [TestMethod]
        public void TestSendBufferWithZeroLenArray()
        {
            var events = new LoggingEvent[0];
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

        [TestMethod]
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

