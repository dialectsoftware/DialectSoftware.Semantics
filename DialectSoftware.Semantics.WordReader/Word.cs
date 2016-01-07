using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DialectSoftware.Semantics
{
    public class Word : WordReader
    {
        public WordReader Parent
        {
            get;
            private set;
        }

        public int? Ordinal
        {
            get;
            internal set;
        }

        public int StartIndex { get; set; }

        public int Length
        {
            get
            {
                return ToString().Length;
            }
        }

        public Word(String text):this(text,0)
        { 
        
        }

        internal Word(String text, int start)
            : base(text)
        {
            StartIndex = start;
        }

        internal Word(String text, WordReader parent)
            : this(text, 0, parent)
        {
  
        }

        internal Word(String text, int start, WordReader parent)
            :this(text, start)
        {
            Parent = parent;
            Parent.OnQuery += new Semantics.Query(Query);
        }

        //TODO:Finish
        internal void Query(Predicate<Word> word, ICollection<Word> accumulator)
        {
            if (word(this))
                accumulator.Add(this);
            this.Find(word, accumulator);
        }

        public static implicit operator Word(String text)
        {
            return new Word(text);
        }

    }
}
