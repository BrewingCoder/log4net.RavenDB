using System;
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

        private IDocumentStore _docStore;
        public string Url { get; set; }
        public string Database { get; set; }

        public RavenDBAppender(){}

        public RavenDBAppender(IDocumentStore documentStore)
        {
            _docStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public override void ActivateOptions()
        {
            if (string.IsNullOrEmpty(Url))
            {
                var exception = new InvalidOperationException("Database connection parameter 'Url' is not specified.");
                ErrorHandler.Error("Database connection parameter 'Url' is not specified.",exception,ErrorCode.GenericFailure);
                return;
            }

            if (string.IsNullOrEmpty(Database))
            {
                var exception = new InvalidOperationException("Database name parameter 'Database' is not specified.");
                ErrorHandler.Error("Database name parameter 'Database' is not specified.", exception, ErrorCode.GenericFailure);
                return;
            }

            EnsureDocumentStore();
        }

        private void EnsureDocumentStore()
        {
            if (_documentStore == null) _documentStore = new Lazy<IDocumentStore>(() => new DocumentStore
            {
                Urls = new[] { Url },
                Database = Database
            });
            if (_docStore == null)
            {
                _docStore = _documentStore.Value;
                _docStore.Initialize();
            }
        }


        protected override async void SendBuffer(LoggingEvent[] events)
        {
            if (events == null || !events.Any()) return;

            var logsEvents = events.Where(e => e != null).Select(e => new Log(e));

            EnsureDocumentStore();
            var session = _docStore.OpenSession();
            await Task.Run(() =>
            {
                Parallel.ForEach(logsEvents, (entry) =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    session.Store(entry);
                    session.SaveChanges();
                });
            });
            session.Dispose();
        }
    }
}
