using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net.Config;

namespace log4net.RavenDB.Tests
{
    [TestClass]
    public class TestRavenDBAppender
    {
        private static readonly log4net.ILog log =log4net.LogManager.GetLogger(typeof(TestRavenDBAppender));


        [TestMethod]
        public void TestMethod1()
        {
           
            XmlConfigurator.Configure();

            log.Info("Test");
        }
    }
}
