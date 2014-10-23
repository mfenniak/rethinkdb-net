using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SineSignal.Ottoman.Serialization
{
	// This is loosely based off of LitJson's JsonReader.  I modified the naming convention and also updated 
	// it a little also to take advantage of newer language features.  It will do for now, until I have more 
	// time to revisit and finish the task below.
	// TODO: Refactor when we refactor JsonParser
	public sealed class JsonReader
	{
		private static IDictionary<int, IDictionary<int, int[]>> parseTable;
		
		private Stack<int> AutomationStack { get; set; }
		private int CurrentInput { get; set; }
		private int CurrentSymbol { get; set; }
		private JsonParser Parser { get; set; }
		private bool ParserInString { get; set; }
		private bool ParserReturn { get; set; }
		private bool ReadStarted { get; set; }
		private TextReader Reader { get; set; }
		private bool ReaderIsOwned { get; set; }
		
		public bool EndOfInput { get; private set; }
		public bool EndOfJson { get; private set; }
		public JsonToken CurrentToken { get; private set; }
		public object CurrentTokenValue { get; private set; }
		
		public bool AllowComments
		{
			get { return Parser.AllowComments; }
			set { Parser.AllowComments = value; }
		}
		
		public bool AllowSingleQuotedStrings
		{
			get { return Parser.AllowSingleQuotedStrings; }
			set { Parser.AllowSingleQuotedStrings = value; }
		}
		
		static JsonReader()
		{
			PopulateParseTable();
		}
		
		public JsonReader(string json) : this(new StringReader(json), true)
		{
		}
		
		public JsonReader(TextReader reader) : this(reader, false)
		{
		}
		
		public JsonReader(TextReader reader, bool owned)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
			
			ParserInString = false;
			ParserReturn = false;
			
			ReadStarted = false;
			AutomationStack = new Stack<int>();
			AutomationStack.Push((int)JsonParserToken.End);
			AutomationStack.Push((int)JsonParserToken.Text);
			
			Parser = new JsonParser(reader);
			
			EndOfInput = false;
			EndOfJson  = false;
			
			this.Reader = reader;
			ReaderIsOwned = owned;
		}
		
		public bool Read()
		{
			if (EndOfInput)
				return false;
			
			if (EndOfJson)
			{
				EndOfJson = false;
				AutomationStack.Clear();
				AutomationStack.Push((int)JsonParserToken.End);
				AutomationStack.Push((int)JsonParserToken.Text);
			}
			
			ParserInString = false;
			ParserReturn = false;
			
			CurrentToken = JsonToken.None;
			CurrentTokenValue = null;
			
			if (!ReadStarted)
			{
				ReadStarted = true;
				
				if (!ReadToken())
					return false;
			}
			
			int[] entry_symbols;
			
			while (true)
			{
				if (ParserReturn)
				{
					if (AutomationStack.Peek() == (int)JsonParserToken.End)
						EndOfJson = true;
					
					return true;
				}
				
				CurrentSymbol = AutomationStack.Pop();
				
				ProcessSymbol();
				
				if (CurrentSymbol == CurrentInput)
				{
					if (!ReadToken())
					{
						if (AutomationStack.Peek() != (int)JsonParserToken.End)
							throw new JsonException("Input doesn't evaluate to proper JSON text");
						
						if (ParserReturn)
							return true;
						
						return false;
					}
					
					continue;
				}
				
				try
				{
					entry_symbols = parseTable[CurrentSymbol][CurrentInput];
				}
				catch(KeyNotFoundException e)
				{
					throw new JsonException((JsonParserToken)CurrentInput, e);
				}

				if (entry_symbols[0] == (int)JsonParserToken.Epsilon)
					continue;
				
				for (int index = entry_symbols.Length - 1; index >= 0; index--)
				{
					AutomationStack.Push(entry_symbols[index]);
				}
			}
		}
		
		public void Close()
		{
			if (EndOfInput)
				return;
			
			EndOfInput = true;
			EndOfJson  = true;
			
			if (ReaderIsOwned)
				Reader.Close();
			
			Reader = null;
		}
		
		private bool ReadToken()
		{
			if (EndOfInput)
				return false;
			
			Parser.NextToken();
			
			if (Parser.EndOfInput)
			{
				Close();
				
				return false;
			}
			
			CurrentInput = Parser.Token;
			
			return true;
		}
		
		private void ProcessSymbol()
		{
			if (CurrentSymbol == '[')
			{
				CurrentToken = JsonToken.ArrayStart;
				ParserReturn = true;
			}
			else if (CurrentSymbol == ']')
			{
				CurrentToken = JsonToken.ArrayEnd;
				ParserReturn = true;
			}
			else if (CurrentSymbol == '{')
			{
				CurrentToken = JsonToken.ObjectStart;
				ParserReturn = true;
			}
			else if (CurrentSymbol == '}')
			{
				CurrentToken = JsonToken.ObjectEnd;
				ParserReturn = true;
			}
			else if (CurrentSymbol == '"')
			{
				if (ParserInString)
				{
					ParserInString = false;
					ParserReturn = true;
				}
				else
				{
					if (CurrentToken == JsonToken.None)
						CurrentToken = JsonToken.String;
					
					ParserInString = true;
				}
			}
			else if (CurrentSymbol == (int)JsonParserToken.CharSeq)
			{
				CurrentTokenValue = Parser.StringValue;
			}
			else if (CurrentSymbol == (int)JsonParserToken.False)
			{
				CurrentToken = JsonToken.Boolean;
				CurrentTokenValue = false;
				ParserReturn = true;
			}
			else if (CurrentSymbol == (int)JsonParserToken.Null)
			{
				CurrentToken = JsonToken.Null;
				ParserReturn = true;
			}
			else if (CurrentSymbol == (int)JsonParserToken.Number)
			{
				ProcessNumber(Parser.StringValue);
				ParserReturn = true;
			}
			else if (CurrentSymbol == (int)JsonParserToken.Pair)
			{
				CurrentToken = JsonToken.MemberName;
			}
			else if (CurrentSymbol == (int)JsonParserToken.True)
			{
				CurrentToken = JsonToken.Boolean;
				CurrentTokenValue = true;
				ParserReturn = true;
			}
		}
		
		private void ProcessNumber(string number)
		{
			if (number.IndexOf('.') != -1 || 
				number.IndexOf('e') != -1 || 
				number.IndexOf('E') != -1)
			{	
				double n_double;
				if (Double.TryParse(number, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out n_double))
				{
					CurrentToken = JsonToken.Double;
					CurrentTokenValue = n_double;
					return;
				}
			}
			
			int n_int32;
			if (Int32.TryParse(number, out n_int32))
			{
				CurrentToken = JsonToken.Int;
				CurrentTokenValue = n_int32;
				return;
			}
			
			long n_int64;
			if (Int64.TryParse(number, out n_int64))
			{
				CurrentToken = JsonToken.Long;
				CurrentTokenValue = n_int64;
				return;
			}
			
			// Shouldn't happen, but just in case, return something
			CurrentToken = JsonToken.Int;
			CurrentTokenValue = 0;
		}
		
		private static void PopulateParseTable()
		{
			parseTable = new Dictionary<int, IDictionary<int, int[]>>();
			
			TableAddRow(JsonParserToken.Array);
			TableAddColumn(JsonParserToken.Array, 
						   '[', '[', (int)JsonParserToken.ArrayPrime);
			
			TableAddRow(JsonParserToken.ArrayPrime);
			TableAddColumn(JsonParserToken.ArrayPrime, 
						   '"', (int)JsonParserToken.Value, (int)JsonParserToken.ValueRest, ']');
			TableAddColumn(JsonParserToken.ArrayPrime, 
						   '[', (int)JsonParserToken.Value, (int)JsonParserToken.ValueRest, ']');
			TableAddColumn(JsonParserToken.ArrayPrime, 
						   ']', ']');
			TableAddColumn(JsonParserToken.ArrayPrime, 
						   '{', (int)JsonParserToken.Value, (int)JsonParserToken.ValueRest, ']');
			TableAddColumn(JsonParserToken.ArrayPrime, 
						   (int)JsonParserToken.Number, (int)JsonParserToken.Value, (int)JsonParserToken.ValueRest, ']');
			TableAddColumn(JsonParserToken.ArrayPrime, 
						   (int)JsonParserToken.True, (int)JsonParserToken.Value, (int)JsonParserToken.ValueRest, ']');
			TableAddColumn(JsonParserToken.ArrayPrime, 
						   (int)JsonParserToken.False, (int)JsonParserToken.Value, (int)JsonParserToken.ValueRest, ']');
			TableAddColumn(JsonParserToken.ArrayPrime, 
						   (int)JsonParserToken.Null, (int)JsonParserToken.Value, (int)JsonParserToken.ValueRest, ']');
			
			TableAddRow(JsonParserToken.Object);
			TableAddColumn(JsonParserToken.Object, 
						   '{', '{', (int)JsonParserToken.ObjectPrime);
			
			TableAddRow(JsonParserToken.ObjectPrime);
			TableAddColumn(JsonParserToken.ObjectPrime, 
						   '"', (int)JsonParserToken.Pair, (int)JsonParserToken.PairRest, '}');
			TableAddColumn(JsonParserToken.ObjectPrime, 
						   '}', '}');
			
			TableAddRow(JsonParserToken.Pair);
			TableAddColumn(JsonParserToken.Pair, 
						   '"', (int)JsonParserToken.String, ':', (int)JsonParserToken.Value);
			
			TableAddRow(JsonParserToken.PairRest);
			TableAddColumn(JsonParserToken.PairRest, 
						   ',', ',', (int)JsonParserToken.Pair, (int)JsonParserToken.PairRest);
			TableAddColumn(JsonParserToken.PairRest, 
						   '}', (int)JsonParserToken.Epsilon);
			
			TableAddRow(JsonParserToken.String);
			TableAddColumn(JsonParserToken.String, 
						   '"', '"', (int)JsonParserToken.CharSeq, '"');
			
			TableAddRow(JsonParserToken.Text);
			TableAddColumn(JsonParserToken.Text, 
						   '[', (int)JsonParserToken.Array);
			TableAddColumn(JsonParserToken.Text, 
						   '{', (int)JsonParserToken.Object);
			
			TableAddRow(JsonParserToken.Value);
			TableAddColumn(JsonParserToken.Value, 
						   '"', (int)JsonParserToken.String);
			TableAddColumn(JsonParserToken.Value, 
						   '[', (int)JsonParserToken.Array);
			TableAddColumn(JsonParserToken.Value, 
						   '{', (int)JsonParserToken.Object);
			TableAddColumn(JsonParserToken.Value, 
						   (int)JsonParserToken.Number, (int)JsonParserToken.Number);
			TableAddColumn(JsonParserToken.Value, 
						   (int)JsonParserToken.True, (int)JsonParserToken.True);
			TableAddColumn(JsonParserToken.Value, 
						   (int)JsonParserToken.False, (int)JsonParserToken.False);
			TableAddColumn(JsonParserToken.Value, 
						   (int)JsonParserToken.Null, (int)JsonParserToken.Null);
			
			TableAddRow(JsonParserToken.ValueRest);
			TableAddColumn(JsonParserToken.ValueRest, 
						   ',', ',', (int)JsonParserToken.Value, (int)JsonParserToken.ValueRest);
			TableAddColumn(JsonParserToken.ValueRest, ']', (int)JsonParserToken.Epsilon);
		}
		
		private static void TableAddColumn(JsonParserToken row, int column, params int[] symbols)
		{
			parseTable[(int)row].Add(column, symbols);
		}
		
		private static void TableAddRow(JsonParserToken rule)
		{
			parseTable.Add((int)rule, new Dictionary<int, int[]>());
		}
	}
	
	public enum JsonToken
	{
		None,
		ObjectStart,
		MemberName,
		ObjectEnd,
		ArrayStart,
		ArrayEnd,
		Int,
		Long,
		Double,
		String,
		Boolean,
		Null
	}
}
