using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;

namespace log4net.RavenDB
{
    public class Log : IEntity
    {
        public string Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public object MessageObject { get; set; }
        public object Exception { get; set; }
        public string LoggerName { get; set; }
        public string Domain { get; set; }
        public string Identity { get; set; }
        public string Level { get; set; }
        public string ClassName { get; set; }
        public string FileName { get; set; }
        public string LineNumber { get; set; }
        public string FullInfo { get; set; }
        public string MethodName { get; set; }
        public string Fix { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public string UserName { get; set; }
        public string ThreadName { get; set; }

        public Log(){}

        public Log(string id)
        {
            if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            Id = id;
        }

        public Log(LoggingEvent logEvent)
        {
            if(logEvent==null) throw new ArgumentNullException(nameof(logEvent));
            LoggerName = logEvent.LoggerName;
            Domain = logEvent.Domain;
            Identity = logEvent.Identity;
            ThreadName = logEvent.ThreadName;
            UserName = logEvent.UserName;
            MessageObject = logEvent.MessageObject;
            TimeStamp = logEvent.TimeStamp;
            Exception = logEvent.ExceptionObject;
            Message = logEvent.RenderedMessage;
            Fix = logEvent.Fix.ToString();

            if (logEvent.Level != null)
            {
                Level = logEvent.Level.ToString();
            }

            if (logEvent.LocationInformation != null)
            {
                ClassName = logEvent.LocationInformation.ClassName;
                FileName = logEvent.LocationInformation.FileName;
                LineNumber = logEvent.LocationInformation.LineNumber;
                FullInfo = logEvent.LocationInformation.FullInfo;
                MethodName = logEvent.LocationInformation.MethodName;
            }
            Properties = logEvent.Properties.GetKeys().ToDictionary(key => key, key => logEvent.Properties[key].ToString());
        }
        

    }
}
