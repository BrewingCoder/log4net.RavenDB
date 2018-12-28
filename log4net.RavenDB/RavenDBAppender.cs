using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using Raven.Client.Documents;

namespace log4net.RavenDB
{
    public class RavenDBAppender : BufferingAppenderSkeleton
    {
        private Lazy<IDocumentStore> _documentStore;

        public IDocumentStore DocStore { get; private set; }
        public string Url { get; set; }
        public string Database { get; set; }
        private bool IsInitialized { get; set; }

        [ExcludeFromCodeCoverage]
        public RavenDBAppender(){}

        public RavenDBAppender(IDocumentStore documentStore)
        {
            DocStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public override void ActivateOptions()
        {
            if (string.IsNullOrEmpty(Url))
            {
                var exception = new InvalidOperationException("Database connection parameter 'Url' is not specified.");
                ErrorHandler.Error("Database connection parameter 'Url' is not specified.",exception,ErrorCode.GenericFailure);
                throw (exception);
            }

            if (string.IsNullOrEmpty(Database))
            {
                var exception = new InvalidOperationException("Database name parameter 'Database' is not specified.");
                ErrorHandler.Error("Database name parameter 'Database' is not specified.", exception, ErrorCode.GenericFailure);
                throw exception;
            }
            EnsureDocumentStore();
        }

        private void EnsureDocumentStore()
        {
            if (DocStore == null)
            {
                if (_documentStore == null) _documentStore = new Lazy<IDocumentStore>(() => new DocumentStore
                {
                    Urls = new[] { Url },
                    Database = Database
                });
                DocStore = _documentStore.Value;
            }

            if (!IsInitialized)
            {
                DocStore.Initialize();
                IsInitialized = true;
            }
        }

        public void LogEvents(LoggingEvent[] events)
        {
            SendBuffer(events);
        }

        protected override async void SendBuffer(LoggingEvent[] events)
        {
            if (events == null || !events.Any()) return;
                using (var session = DocStore.OpenSession())
                {
                    var logsEvents = events.Where(e => e != null).Select(e => new Log(e)).ToList();
                    foreach (var entry in logsEvents)
                    {
                        session.Store(entry);
                        session.SaveChanges();
                    }
                };
        }
    }
}
