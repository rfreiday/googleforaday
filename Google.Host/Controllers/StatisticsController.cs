using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.SearchEngine;
using Google.SearchEngine.Model;

namespace Google.Host.Controllers
{
    public class StatisticsController : BaseGoogleController
    {
        public virtual ActionResult Index()
        {
            return View();
        }
    }
}
