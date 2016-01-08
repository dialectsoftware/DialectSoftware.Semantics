using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ******************************************************************************************************************
/// * Copyright (c) 2011 Dialect Software LLC                                                                        *
/// * This software is distributed under the terms of the Apache License http://www.apache.org/licenses/LICENSE-2.0  *
/// *                                                                                                                *
/// ******************************************************************************************************************

namespace DialectSoftware.Semantics
{
    public class WordDigest<T> : IEqualityComparer<Word>, IEqualityComparer<String>
    {
        public string[] Universe
        {
            get;
            private set;
        }

        public StringComparison ComparisonMode
        {
            get;
            private set;
        }

        public WordDigest()
            : this(StringComparison.InvariantCultureIgnoreCase)
        { 
        }

        public WordDigest(StringComparison comparisonMode)
        {
            Universe = new string[] { };
            ComparisonMode = comparisonMode;
        }

        public void Parse(T key, String text)
        {
           var words = new WordReader(text).Distinct(this).Select(w => w.ToString()).ToArray();
            if (Universe.Length == 0)
            {
                Universe = words;
            }
            else
            {
                Universe = Universe.Union(words, this).ToArray();
            }
        
        }

        public void Parse(T key, String text, Predicate<Word> filter)
        {
            IEnumerable<Word> reader = new WordReader(text).ToArray();
            var words = reader.Where(w => !filter(w.Text)).Distinct(this).Select(w => w.ToString()).ToArray();
            if (Universe.Length == 0)
            {
                Universe = words;
            }
            else
            {
                Universe = Universe.Union(words, this).ToArray();
            }

        }

        public void Parse(T key, String text, string[] noiseFilter)
        {
            IEnumerable<Word> reader = new WordReader(text).ToArray();
            var words = reader.Where(w=>!noiseFilter.Contains(w.Text,this)).Distinct(this).Select(w => w.ToString()).ToArray();
            if (Universe.Length == 0)
            {
                Universe = words;
            }
            else
            {
                Universe = Universe.Union(words, this).ToArray();
            }

        }

        public bool Equals(Word x, Word y)
        {
            return x.Text.Equals(y.Text, ComparisonMode);
        }

        public int GetHashCode(Word obj)
        {
            if (ComparisonMode.ToString().Contains("IgnoreCase"))
                return obj.Text.ToLower().GetHashCode();
            else
                return obj.GetHashCode();
        }

        public bool Equals(string x, string y)
        {
            return x.Equals(y, ComparisonMode);
        }

        public int GetHashCode(string obj)
        {
            if (ComparisonMode.ToString().Contains("IgnoreCase"))
                return obj.ToLower().GetHashCode();
            else
                return obj.GetHashCode();
        }
    }
}
