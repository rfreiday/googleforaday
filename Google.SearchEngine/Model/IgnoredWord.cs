using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Google.SearchEngine.Model
{
    public enum IgnoredWordType
    {
        Conjunctions,
        Pronouns,
        Articles,
        AuxiliaryVerbs,
        Prepositions
    }

    public class IgnoredWord
    {
        public IgnoredWordType Type { get; set; }

        public string Word { get; set; }

        public bool AlternateMeanings { get; set; }
    }
}
