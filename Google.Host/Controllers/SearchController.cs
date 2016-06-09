using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.SearchEngine.Model;
using System.Threading.Tasks;

namespace Google.Host.Controllers
{
    public class SearchController : BaseGoogleController
    {
        public virtual ActionResult Index()
        {
            return View(new SearchRequest());
        }

        public ActionResult SearchResults(string searchValue)
        {
            if (!string.IsNullOrEmpty(searchValue))
            {
                try
                {
                    WebKeyword keyword = Indexer.Keywords.Where(o => o.Key.Equals(searchValue, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value;
                    if (keyword != null)
                        return View(keyword);
                    else
                        return RedirectToAction("KeywordNotFound");
                }
                catch
                {
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult KeywordNotFound()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(SearchRequest indexPageRequest)
        {
            return RedirectToAction("SearchResults", new { searchValue = indexPageRequest.RequestValue });
        }      
    }
}
