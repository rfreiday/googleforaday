using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Google.SearchEngine.Model
{
    public class WebKeyword
    {
        public WebKeyword(string word)
        {
            Word = word;
            PageReferences = new Dictionary<WebPage, int>();
        }

        public void AddWebPageReference(WebPage webPage)
        {
            if (!PageReferences.Keys.Contains(webPage))
                PageReferences.Add(webPage, 0);
            PageReferences[webPage]++;
        }

        public int Count
        {
            get 
            {
                int count = 0;
                foreach (WebPage page in PageReferences.Keys)
                    count += PageReferences[page];
                return count;
            }
        }

        public string Word { get; private set;}

        public List<WebPage> RankedPages
        {
            get
            {
                var sortedDict = from entry in PageReferences orderby entry.Value descending select entry.Key;
                return sortedDict.ToList();
            }
        }

        public Dictionary<WebPage, int> PageReferences { get; private set;}

        public DateTime? LastSearch { get; set; }

        public int SearchCount { get; set; }
    }
}