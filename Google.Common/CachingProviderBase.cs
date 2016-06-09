using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Google.Common
{
    public abstract class CachingProviderBase
    {
        public CachingProviderBase()
        {
            DeleteLog();
        }

        protected MemoryCache cache = new MemoryCache("CachingProvider");

        static readonly object padlock = new object();

        protected virtual void AddItem(string key, object value)
        {
            lock (padlock)
            {
                cache.Add(key, value, DateTimeOffset.MaxValue);
            }
        }

        protected virtual void RemoveItem(string key)
        {
            lock (padlock)
            {
                cache.Remove(key);
            }
        }

        protected virtual void Clear()
        {
            lock (padlock)
            {
                cache.Dispose();
                cache = new MemoryCache("CachingProvider");
            }
        }

        protected virtual T GetItem<T>(string key, bool createIfNull)
        {
            lock (padlock)
            {
                var res = cache[key];
                if (res != null)
                    if (res is T)
                        return (T)res;

                if (createIfNull)
                    return (T)Activator.CreateInstance(typeof(T));
                else
                    return default(T);
            }
        }

        #region Error Logs

        string LogPath = System.Environment.GetEnvironmentVariable("TEMP");

        protected void DeleteLog()
        {
            System.IO.File.Delete(string.Format("{0}\\CachingProvider_Errors.txt", LogPath));
        }

        protected void WriteToLog(string text)
        {
            using (System.IO.TextWriter tw = System.IO.File.AppendText(string.Format("{0}\\CachingProvider_Errors.txt", LogPath)))
            {
                tw.WriteLine(text);
                tw.Close();
            }
        }

        #endregion
    } 
}
