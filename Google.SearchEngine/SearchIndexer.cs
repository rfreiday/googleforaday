using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;
using HtmlAgilityPack;

using Google.SearchEngine.Model;
using Google.Common;

namespace Google.SearchEngine
{
    internal enum CacheObjectType
    {
        KeyWords,
        IndexedPages,
        IgnoredWords,
    }

    public class SearchIndexer
    {
        #region Private Variables

        private Dictionary<string, IgnoredWord> _ignoredWords;
        private Dictionary<string, WebKeyword> _keywords;
        private Dictionary<string, WebPage> _indexedPages;

        #endregion

        #region Public Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchIndexer()
        {
            LoadCachedData();
            LoadIgnoredWords();
        }

        public WebKeyword SearchKeyword(string searchValue)
        {
            WebKeyword keyword = Keywords.Where(o => o.Key.Equals(searchValue, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value;
            keyword.LastSearch = DateTime.Now;
            keyword.SearchCount++;
            return keyword;
        }

        /// <summary>
        /// This method will start the "web crawler" process to index a given page
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Task<WebPage> IndexPage(string url)
        {
            return LoadWebPage(url, 0);
        }

        public WebPage GetPage(string url)
        {
            Uri uri = GetUriFromUrl(url);
            return _indexedPages.Where(o => o.Key.Equals(url, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value;
        }

        public WebKeyword GetKeyword(string searchValue)
        {
            return Keywords.Where(o => o.Key.Equals(searchValue, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value;
        }

        public void ClearIndexes()
        {
            foreach (CacheObjectType cacheObjectType in Enum.GetValues(typeof(CacheObjectType)))
                GlobalCachingProvider.Instance.AddItem(cacheObjectType, null);
        }

        #endregion

        #region Public Properties

        public Dictionary<string, WebKeyword> Keywords
        {
            get { return _keywords; }
        }

        public Dictionary<string, IgnoredWord> IgnoredWords
        {
            get { return _ignoredWords; }
        }

        public Dictionary<string, WebPage> IndexedPages
        {
            get { return _indexedPages; }
        }

        public int MaxLevels { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method can be called recursively up to x levels.  It loads the page then calls the IndexKeywords to index the words
        /// </summary>
        /// <param name="url">The URL of the page to load</param>
        /// <param name="level">The number of levels in the current call stack.  This should be set to 0 initially.</param>
        /// <returns></returns>
        private Task<WebPage> LoadWebPage(string url, int level)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            level++;
            if (level > MaxLevels)
                return null;

            if (url.EndsWith("pdf", StringComparison.OrdinalIgnoreCase))
                return null;

            WebPage webPage = GetPage(url);
            if (webPage != null)
                return Task.FromResult<WebPage>(webPage);

            Uri uri = GetUriFromUrl(url);
            var html = new HtmlDocument();
            try
            {
                string htmlFile;
                if (uri.IsFile)
                    html.Load(url);
                else
                {
                    htmlFile = new WebClient().DownloadString(uri.AbsoluteUri);
                    html.LoadHtml(htmlFile);
                }
            }
            catch (Exception)
            {
                return null;
            }

            var root = html.DocumentNode;
            var nodes = root.Descendants();

            nodes
                .Where(n => n.Name == "script" || n.Name == "style")
                .ToList()
                .ForEach(n => n.Remove());

            var titleNode = nodes.Where(o => o.Name.Equals("Title", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var bodyNode = nodes.Where(o => o.Name.Equals("Body", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (bodyNode != null)
            {
                string title = uri.AbsoluteUri;
                if (titleNode != null)
                    title = titleNode.InnerText.Replace("\t", string.Empty).Replace("\r\n", string.Empty);

                webPage = new WebPage(uri.AbsoluteUri, title);
                IndexKeywords(webPage, bodyNode.OuterHtml);

                _indexedPages.Add(url, webPage);

                var anchors = root.Descendants("a");
                foreach (HtmlNode node in anchors)
                {
                    HtmlAttribute href = node.Attributes.Where(o => o.Name.Equals("href", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (href != null)
                    {
                        string hrefUrl = href.Value;
                        if (!hrefUrl.ToLower().Contains("mailto"))
                        {
                            if (!hrefUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) &
                                !hrefUrl.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                            {
                                Uri uriToUse = new Uri(uri, hrefUrl);
                                if (GetPage(uriToUse.AbsoluteUri) == null)
                                {
                                    var linkedPage = LoadWebPage(uriToUse.AbsoluteUri, level);
                                    if (linkedPage != null)
                                        webPage.Links.Add(linkedPage.Result);
                                }
                            }
                        }
                    }
                }
            }

            GlobalCachingProvider.Instance.AddItem(CacheObjectType.IndexedPages, _indexedPages);
            return Task.FromResult<WebPage>(webPage);
        }

        /// <summary>
        /// This method produces a list of keywords which can be searched on.
        /// </summary>
        /// <param name="webPage"></param>
        //private void IndexKeywords(WebPage webPage, HtmlNode parentNode)
        private void IndexKeywords(WebPage webPage, string htmlText)
        {
            try
            {
                // First, strip out all "nodes", leaving us with just a collection of values (between the nodes)
                htmlText = htmlText.RemoveSections('<', '>').RemoveSections('[', ']');

                // Next, delimit the values into seperate words
                char[] delimiterChars = { ' ', ',', '.', ':', '-', '\t' };
                string[] words = htmlText.Split(delimiterChars);
                foreach (string s in words)
                {
                    string keywordKey = s.Trim().ToLower().Replace("(", string.Empty).Replace(")", string.Empty);
                    //if (keywordKey.ContainsAlphaOnly())
                        if (!string.IsNullOrEmpty(keywordKey))
                            if (IgnoredWords.Keys.Where(o => o.Equals(s, StringComparison.OrdinalIgnoreCase)).Count() == 0)
                            {
                                WebKeyword webKeyword = Keywords.Where(o => o.Key.Equals(keywordKey, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value;
                                if (webKeyword == null)
                                {
                                    webKeyword = new WebKeyword(keywordKey);
                                    Keywords.Add(keywordKey, webKeyword);
                                }
                                webKeyword.AddWebPageReference(webPage);
                            }
                }

                GlobalCachingProvider.Instance.AddItem(CacheObjectType.KeyWords, _keywords);
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// This method loads a list of "ignored words" which will be omitted from the indexing engine.
        /// </summary>
        private void LoadIgnoredWords()
        {
            if (_ignoredWords.Count > 0)
                return;
            try
            {
                // Attempt to find / load the file containing the reserved words
                _ignoredWords = new Dictionary<string, IgnoredWord>();
                const string RESERVED_WORDS_FILE = "ReservedWordsFile";
                string reservedWordsFile = ConfigurationManager.AppSettings.Get(RESERVED_WORDS_FILE);
                if (String.IsNullOrEmpty(reservedWordsFile))
                    throw new Exception(string.Format("Missing Application Setting [{0}]", RESERVED_WORDS_FILE));
                if (!File.Exists(reservedWordsFile))
                    throw new FileNotFoundException(string.Format("Could not find reserved words definitions at [{0}]", reservedWordsFile));

                // Load the document
                XDocument doc = XDocument.Load(reservedWordsFile);
                XElement rootElement = doc.Elements().FirstOrDefault();

                // Load the list of word "types"
                IEnumerable<XElement> reservedWordTypeList =
                    from reservedWordType in rootElement.Elements()
                    select reservedWordType;

                // Enumerate through the types
                foreach (XElement reservedWordType in reservedWordTypeList)
                {
                    // If the word type is valid...
                    IgnoredWordType wordType;
                    if (Enum.TryParse<IgnoredWordType>(reservedWordType.Name.ToString(), out wordType))
                    {
                        // Enumerate through the list of reserved words and add them to the list.
                        IEnumerable<XElement> wordList =
                            from word in reservedWordType.Elements()
                            select word;

                        foreach (XElement word in wordList)
                            if (!IgnoredWords.ContainsKey(word.Value.ToString()))
                            {
                                IgnoredWord reservedWord = new IgnoredWord();
                                reservedWord.Type = wordType;
                                reservedWord.Word = word.Value.ToString();
                                _ignoredWords.Add(reservedWord.Word, reservedWord);
                            }
                    }
                }

                GlobalCachingProvider.Instance.AddItem(CacheObjectType.IgnoredWords, _ignoredWords);
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading list of reserved words.  " + ex.Message);
            }
        }

        private Uri GetUriFromUrl(string urlString)
        {
            try
            {
                if (!urlString.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    urlString = string.Format("http://{0}", urlString);
                return new Uri(urlString);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void LoadCachedData()
        {
            _keywords = GlobalCachingProvider.Instance.GetItem<Dictionary<string, WebKeyword>>(CacheObjectType.KeyWords, true);
            _ignoredWords = GlobalCachingProvider.Instance.GetItem<Dictionary<string, IgnoredWord>>(CacheObjectType.IgnoredWords, true);
            _indexedPages = GlobalCachingProvider.Instance.GetItem<Dictionary<string, WebPage>>(CacheObjectType.IndexedPages, true);
        }

        #endregion
    }
}
