using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Google.SearchEngine
{
    public class SearchDataManager
    {
        /// <summary>
        /// Gets a web_domain record based on the selected URL.  If not found, the record is inserted.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public web_domain GetDomain(string url)
        {
            using (DataClassesDataContext db = new DataClassesDataContext())
            {
                var domain = from webDomain in db.web_domains
                             where webDomain.domain_name.Equals(url)
                             select webDomain;
                if (domain != null)
                    return domain.FirstOrDefault();
                else
                {
                    var newDomain = new web_domain();
                    newDomain.domain_name = url;
                    db.web_domains.InsertOnSubmit(newDomain);
                    db.SubmitChanges();
                    return newDomain;
                }
            }
        }
    }
}
