using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Google.Common
{
    public class GlobalCachingProvider : CachingProviderBase, IGlobalCachingProvider
    {
        #region Singleton

        protected GlobalCachingProvider()
        {
        }

        public static GlobalCachingProvider Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }
            internal static readonly GlobalCachingProvider instance = new GlobalCachingProvider();
        }

        #endregion

        #region ICachingProvider

        public virtual new void AddItem(Enum key, object value)
        {
            AddItem(key.ToString(), value);
        }

        public virtual new void AddItem(string key, object value)
        {
            base.AddItem(key, value);
        }

        public virtual T GetItem<T>(Enum key, bool createIfNull)
        {
            return GetItem<T>(key.ToString(), createIfNull);
        }

        public virtual T GetItem<T>(string key, bool createIfNull)
        {
            return base.GetItem<T>(key, createIfNull);
        }

        public void Clear()
        {
            base.Clear();
        }
        #endregion
    }
}
