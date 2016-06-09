using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Google.SearchEngine.Model
{
    public class WebPage
    {
        private List<WebPage> _links = new List<WebPage>();

        public WebPage(string url, string title)
        {
            this.Url = url;
            this.Title = title;
            this.IndexDate = DateTime.Now;
        }

        public string Url { get; private set; }
        
        public string Title { get; private set; }

        public DateTime IndexDate { get; set; }

        public List<WebPage> Links
        {
            get { return _links;  }
        }  
    }
}