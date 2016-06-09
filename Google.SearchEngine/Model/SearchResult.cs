using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Google.SearchEngine.Model
{
    public class SearchResult
    {
        private Dictionary<WebPage, int> _matchingPages = new Dictionary<WebPage, int>();

        public SearchResult(string keyword)
        {
            this.Keyword = keyword;
            
        }

        public string Keyword { get; private set; }

        public Dictionary<WebPage, int> MatchingPages
        {
            get { return _matchingPages; }
        }
    }
}