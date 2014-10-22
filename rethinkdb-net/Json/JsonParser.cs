using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace RethinkDb.Json
{
    public class JsonParser
    {
        private readonly TextReader reader;
        private IToken currentToken;
        private Stack<State> state = new Stack<State>();

        private enum State
        {
            ArrayValue,
            ArraySeparator,
        }

        public JsonParser(TextReader reader)
        {
            this.reader = reader;
        }

        public IToken Token
        {
            get
            {
                if (currentToken == null)
                    throw new InvalidOperationException("Invoke Read() first");
                return currentToken;
            }
        }

        public bool Read()
        {
            if (state.Count == 0)
                return ReadNextValue();
            var s = state.Peek();
            switch (s)
            {
                case State.ArrayValue:
                    ReadNextValue()
                    
                case State.ArraySeparator:
                    return ReadForArraySeparator();
                default:
                    throw new Exception("huh?");
            }
        }

        private bool ReadNextValue()
        {
            int c = reader.Read();
            while (true)
            {
                switch (c)
                {
                    case -1:
                        return false;
                    case (int)' ':
                    case (int)'\t':
                    case (int)'\n':
                    case (int)'\r':
                        continue;
                    case (int)'n':
                        {
                            int n1 = reader.Read();
                            int n2 = reader.Read();
                            int n3 = reader.Read();
                            if (n1 == (int)'u' && n2 == (int)'l' && n3 == (int)'l')
                            {
                                currentToken = NullToken.Instance;
                                return true;
                            }
                            else
                                return false;
                        }
                    case (int)'t':
                        {
                            int n1 = reader.Read();
                            int n2 = reader.Read();
                            int n3 = reader.Read();
                            if (n1 == (int)'r' && n2 == (int)'u' && n3 == (int)'e')
                            {
                                currentToken = TrueToken.Instance;
                                return true;
                            }
                            else
                                return false;
                        }
                    case (int)'f':
                        {
                            int n1 = reader.Read();
                            int n2 = reader.Read();
                            int n3 = reader.Read();
                            int n4 = reader.Read();
                            if (n1 == (int)'a' && n2 == (int)'l' && n3 == (int)'s' && n4 == (int)'e')
                            {
                                currentToken = FalseToken.Instance;
                                return true;
                            }
                            else
                                return false;
                        }
                    case (int)'0':
                    case (int)'1':
                    case (int)'2':
                    case (int)'3':
                    case (int)'4':
                    case (int)'5':
                    case (int)'6':
                    case (int)'7':
                    case (int)'8':
                    case (int)'9':
                    case (int)'-':
                    case (int)'.':
                        return ParseNumber((char)c, out currentToken);
                    case (int)'"':
                        return ParseString(out currentToken);
                    case (int)'[':
                        currentToken = StartArrayToken.Instance;
                        state.Push(State.ArrayValue);
                        return true;
                    //return ParseArray(out currentToken);
                    //case (int)'{':
                    //currentToken = ParseObject(reader, ref depthCount);
                    //return true;
                    default:
                        return false;
                }
            }
        }
    

        private bool ParseString(out IToken token)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                int c = reader.Read();
                switch (c)
                {
                    case -1:
                        throw new Exception("eof?");
                    case '"':
                        token = new StringToken(sb.ToString());
                        return true;
                    case '\\':
                        {
                            int c2 = reader.Read();
                            switch (c2)
                            {
                                case -1:
                                    throw new Exception("eof");
                                case (int)'"':
                                    sb.Append('"');
                                    break;
                                case (int)'\\':
                                    sb.Append('\\');
                                    break;
                                case (int)'/':
                                    sb.Append('/');
                                    break;
                                case (int)'b':
                                    sb.Append('\b');
                                    break;
                                case (int)'f':
                                    sb.Append('\f');
                                    break;
                                case (int)'n':
                                    sb.Append('\n');
                                    break;
                                case (int)'r':
                                    sb.Append('\r');
                                    break;
                                case (int)'t':
                                    sb.Append('\t');
                                    break;
                                case (int)'u':
                                    {
                                        StringBuilder sb2 = new StringBuilder();
                                        sb2.Append(ReadHex(reader));
                                        sb2.Append(ReadHex(reader));
                                        sb2.Append(ReadHex(reader));
                                        sb2.Append(ReadHex(reader));
                                        sb.Append((char)Int32.Parse(sb2.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier));
                                    }
                                    break;
                            }
                        }
                        break;
                    default:
                        sb.Append((char)c);
                        break;
                }
            }
        }

        private static char ReadHex(TextReader reader)
        {
            int c = reader.Read();
            if (c == -1)
                throw new Exception("eof");
            if ((c >= 'a' && c <= 'f') || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F'))
                return (char)c;
            else
                throw new Exception(String.Format("Invalid hex character in \\u escaped character: {0}", (char)c));
        }

        private bool ParseNumber(char first, out IToken token)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(first);
            while (true)
            {
                int c = reader.Peek();
                switch (c)
                {
                    case (int)'-':
                    case (int)'+':
                    case (int)'0':
                    case (int)'1':
                    case (int)'2':
                    case (int)'3':
                    case (int)'4':
                    case (int)'5':
                    case (int)'6':
                    case (int)'7':
                    case (int)'8':
                    case (int)'9':
                        reader.Read();
                        sb.Append((char)c);
                        break;
                    case (int)'.':
                    case (int)'e':
                    case (int)'E':
                        reader.Read();
                        sb.Append((char)c);
                        break;
                    case -1:
                    default:
                        token = new NumberToken(sb.ToString());
                        return true;
                }
            }
        }

        private bool ReadForArraySeparator()
        {
            bool foundSeparator = false;
            while (true)
            {
                int p = reader.Peek();
                switch (p)
                {
                    case -1:
                        throw new Exception("eof?");
                    case (int)' ':
                    case (int)'\t':
                    case (int)'\n':
                    case (int)'\r':
                        reader.Read();
                        continue;
                    case (int)',':
                        if (foundSeparator)
                            throw new Exception("multiple array separators");
                        foundSeparator = true;
                        continue;
                    case (int)']':
                        currentToken = EndArrayToken.Instance;
                        state.Pop();
                        return true;
                    default:
                        return ReadNextValue();
                }
            }
        }

        /*
        private static JsonObject ParseObject(TextReader reader, ref int depthCount)
        {
            JsonObject retval = new JsonObject();

            while (true)
            {
                int c = reader.Read();
                switch (c)
                {
                    case -1:
                        throw new JsonParseEofException();
                    case (int)' ':
                    case (int)'\t':
                    case (int)'\n':
                    case (int)'\r':
                        continue;
                    case (int)'"':
                        {
                            string key = ParseString(reader);
                            bool readForColon = true;
                            while (readForColon)
                            {
                                int c2 = reader.Read();
                                switch (c2)
                                {
                                    case -1:
                                        throw new JsonParseEofException();
                                    case (int)' ':
                                    case (int)'\t':
                                    case (int)'\n':
                                    case (int)'\r':
                                        continue;
                                    case (int)':':
                                        readForColon = false;
                                        break;
                                    default:
                                        throw new JsonParseException("Expected :, not {0}", (char)c2);
                                }
                            }

                            retval[key] = Parse(reader, ref depthCount);

                            bool readForComma = true;
                            while (readForComma)
                            {
                                int c2 = reader.Read();
                                switch (c2)
                                {
                                    case -1:
                                        throw new JsonParseEofException();
                                    case (int)' ':
                                    case (int)'\t':
                                    case (int)'\n':
                                    case (int)'\r':
                                        continue;
                                    case (int)',':
                                        readForComma = false;
                                        break;
                                    case (int)'}':
                                        return retval;
                                    default:
                                        throw new JsonParseException("Expected , or }}, not {0}", (char)c2);
                                }
                            }
                        }
                        break;
                    case (int)'}':
                        return retval;
                    default:
                        throw new JsonParseException("Expected \" or }}, not {0}", (char)c);
                }
            }
        }
        */

    }
}
