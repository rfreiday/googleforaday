using System;
using System.Linq;
using System.Data.Linq;
using System.Configuration;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Google.SearchEngine;
using Google.SearchEngine.Model;

namespace Google.SearchEngine.Tests
{
    [TestClass]
    public class SearchIndexerTest
    {
        [TestMethod]
        public void IndexPageTest()
        {
            SearchIndexer indexer = new SearchIndexer();
            Assert.IsTrue(indexer.IgnoredWords.Count > 0, "The system could not load the list of reserved words.");
            foreach (IgnoredWordType wordType in Enum.GetValues(typeof(IgnoredWordType)))
                Assert.IsTrue(
                    indexer.IgnoredWords.Values.Where(o => o.Type.Equals(wordType)).Count() > 0,
                    string.Format("The system has no instances of '{0}' reserved word types", wordType.ToString()));
        }

        [TestMethod]
        public void LoadWebSiteTest()
        {
            SearchIndexer indexer = new SearchIndexer();
            indexer.MaxLevels = 3;
            var page = indexer.IndexPage("www.freiday.com");
            Assert.IsTrue(page.Result.Title.Contains("Freiday"));
            Assert.IsTrue(page.Result.Links.Count > 0);
        }

        [TestMethod]
        public void TestDataConnection()
        {
            DataClassesDataContext db = new DataClassesDataContext();
            Table<web_domain> domains = db.web_domains;
            foreach (web_domain domain in domains)
            {

            }
            Assert.IsTrue(domains.Count() > 0);
        }
    }
}
