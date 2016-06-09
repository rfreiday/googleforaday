using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.SearchEngine;

namespace Google.Host.Controllers
{
    public abstract class BaseGoogleController : Controller
    {
        SearchIndexer _indexer;

        public SearchIndexer Indexer
        {
            get 
            {
                if (_indexer == null)
                    _indexer = new SearchIndexer();
                return _indexer;
            }
        }
    }
}
