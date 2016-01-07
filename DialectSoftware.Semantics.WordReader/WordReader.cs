using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace DialectSoftware.Semantics
{
    //TODO:Finish
    internal delegate void Query(Predicate<Word> word, ICollection<Word> accumulator);
    public delegate bool Coalescence(ref Char cChar, String text, out bool include);

    public class WordReader : StringReader, IEnumerable<Word>, IEnumerator<Word> 
    {
        int currentIndex;

        public WordReader(string text)
            : base(text)
        {
            Text = text;
        }

        //TODO:Finish
        internal event Query OnQuery;

        public string Text
        {
            get;
            private set;
        }

        public int CurrentIndex
        {
            get { return currentIndex; }
            set { currentIndex = value; }
        }

        public override int Read()
        {
            Interlocked.Increment(ref currentIndex);
            return base.Read();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            int i = base.Read(buffer, index, count);
            if (i > 0)
            {
                Interlocked.Increment(ref currentIndex);
            }
            return i;
        }

        public override string ReadLine()
        {
            return ReadTo('\n').ReadTo('\r');
        }

        public override string ReadToEnd()
        {
            currentIndex = ToString().Length;
            return base.ReadToEnd();
        }

        public IEnumerable<Word> ReadToEnd(Predicate<char> predicate)
        {
            int i = -1;
            while (!IsEOF)
            {
                i++;
                var word = ReadTo(predicate);
                word.Ordinal = i;
                yield return word;
            }
        }

        public Word ReadNextWord()
        {
            return ReadTo(' ', '\r');
        }

        public Word ReadNextWord(Predicate<char> predicate)
        {
            return ReadTo(predicate);
        }

        public void MoveToNextWord()
        {
            do
            {
                int i = Read();
                if (i > 0)
                {
                    if (char.IsWhiteSpace(Convert.ToChar(i)))
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            while (true);

            do
            {
                int i = Peek();
                if(i > 0)
                {
                    if (char.IsLetterOrDigit(Convert.ToChar(i)))
                    {
                        break;
                    }
                    else 
                    {
                        Read();
                    }

                }
                else
                {
                    break;
                }
            }
            while (true);
            
        }

        public Word ReadTo()
        {
            return ReadTo(new Predicate<char>(delegate(char c)
            {
                return false;
            }));

        }

        public Word ReadTo(params Char[] boundaryTokens)
        {
            return ReadTo(new Predicate<char>(delegate(char c)
            {
                return boundaryTokens.Contains(c);
            }));

        }

        public Word ReadTo(Predicate<char> predicate)
        {
            string text = String.Empty;
            int startIndex = currentIndex;
            do
            {
                char[] cChar = new char[1];
                if (Read(cChar, 0, 1) == 1)
                {
                    if (predicate(cChar.Single()))
                    {
                        if (String.IsNullOrEmpty(text))
                        {
                            //text = String.Concat(text, cChar.Single());
                            text = String.Concat(text, Char.MinValue);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        text = String.Concat(text, cChar.Single());
                    }
                }
                else
                {
                    break;
                }

            } while (true);
            Word word = new Word(text, startIndex, this);
            return word;
        }

        public String Coalesce(Coalescence coalesce)
        {
            string text = String.Empty;
            do
            {
                char[] cChar = new char[1];
                if (Read(cChar, 0, 1) == 1)
                {
                    bool include;
                    Char c = cChar.Single();
                    if (coalesce(ref c, text, out include))
                    {
                        if (include)
                        {
                            text = String.Concat(text, c);
                        }

                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

            } while (true);
            return text;
        }

        public void Coalesce(Predicate<char> predicate, Action<char> action)
        {
            do
            {
                char[] cChar = new char[1];
                if (Read(cChar, 0, 1) == 1)
                {
                    if (predicate(cChar.Single()))
                    {
                        action(cChar.Single());
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

            } while (true);
        }

        public bool IsEOF
        {
            get 
            {
                return CurrentIndex >= ToString().Length;
            }
        }

        //TODO:Finish
        public IEnumerable<Word> Find(Predicate<Word> word)
        {
            ICollection<Word> accumulator = new List<Word>();
            Find(word, accumulator);
            return accumulator;
        }

        protected void Find(Predicate<Word> word, ICollection<Word> accumulator)
        {
            if (OnQuery != null)
            {
                OnQuery(word, accumulator);
            }
        }

        public override string ToString()
        {
            return Text;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            String value = obj.ToString();
            return ToString().Equals(value);
        }

        public static bool operator ==(WordReader a, WordReader b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(WordReader a, WordReader b)
        {
            return !(a == b);
        }

        public static implicit operator String(WordReader word)
        {
            if (word.IsEOF)
                return null;
            else
                return word.ToString();
        }

        #region IEnumerator<Word>

        public Word Current
        {
            get;
            private set;
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (!IsEOF)
                Current = ReadNextWord();
            else
                Current = null;
            return !IsEOF;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<Word>
        public IEnumerator<Word> GetEnumerator()
        {
            return this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this;
        }
        #endregion

        public static WordReader Open(string path)
        {
           WordReader reader = null;
           using(var stream = File.OpenText(path))
           {
               reader = new WordReader(stream.ReadToEnd());
               stream.Close();
           }
           return reader;
        }

        
    }
}
