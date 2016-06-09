using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.SearchEngine.Model;
using System.Threading.Tasks;

namespace Google.Host.Controllers
{
    public class IndexController : BaseGoogleController
    {
        public ActionResult Index()
        {
            return View(new SearchRequest());
        }

        [HttpPost]
        public ActionResult Index(SearchRequest indexPageRequest, string IndexPage, string ClearIndexes)
        {
            try
            {
                if (!string.IsNullOrEmpty(IndexPage))
                {
                    if (!string.IsNullOrEmpty(indexPageRequest.RequestValue))
                    {
                        Indexer.MaxLevels = 3;
                        var indexResult = Task.Run(() => Indexer.IndexPage(indexPageRequest.RequestValue)).Result;
                        if (indexResult != null)
                            return RedirectToAction("IndexResults", new { url = indexPageRequest.RequestValue });
                    }
                }
                else
                {
                    Indexer.ClearIndexes();
                }
            }
            catch
            {
            }
            return RedirectToAction("Index");
        }

        public ActionResult IndexResults(string url)
        {
            WebPage page = Indexer.GetPage(url);
            return View(page);
        }
    }
}
