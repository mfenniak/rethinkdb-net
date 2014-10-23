using System;
using System.IO;
using System.Text;

namespace SineSignal.Ottoman.Serialization
{
	// This is loosely based off of LitJson's Lexer.  I modified the naming convention and also updated 
	// it a little also to take advantage of newer language features.  It will do for now, until I have 
	// more time to revisit and finish the task below.
	// TODO: Refactor using the State pattern(http://www.dofactory.com/Patterns/PatternState.aspx).  It will make this a lot more maintainable and testable.
	internal sealed class JsonParser
	{
		private static int[] stateReturnTable;
		private static Func<StateContext, bool>[] stateHandlerTable;
		
		private StateContext Context { get; set; }
		private int InputBuffer { get; set; }
		private int InputCharacter { get; set; }
		private TextReader Reader { get; set; }
		private int State { get; set; }
		private StringBuilder StringBuffer { get; set; }
		private int UnicodeCharacter { get; set; }
		
		public bool AllowComments { get; set; }
		public bool AllowSingleQuotedStrings { get; set; }
		
		public bool EndOfInput { get; private set; }
		public int Token { get; private set; }
		public string StringValue { get; private set; }
		
		static JsonParser()
		{
			PopulateStateTables();
		}
		
		public JsonParser(TextReader reader)
		{
			InputBuffer = 0;
			StringBuffer = new StringBuilder(128);
			State = 1;
			
			AllowComments = false;
			AllowSingleQuotedStrings = false;
			EndOfInput = false;
			
			this.Reader = reader;
			
			Context = new StateContext();
			Context.Parser = this;
		}
		
		public bool NextToken()
		{
			Func<StateContext, bool> stateHandler;
			Context.Return = false;
			
			while (true)
			{
				stateHandler = stateHandlerTable[State - 1];
				
				if (!stateHandler(Context))
					throw new JsonException(InputCharacter);
				
				if (EndOfInput)
					return false;
				
				if (Context.Return)
				{
					StringValue = StringBuffer.ToString();
					StringBuffer.Remove(0, StringBuffer.Length);
					Token = stateReturnTable[State - 1];
					
					if (Token == (int)JsonParserToken.Char)
						Token = InputCharacter;
					
					State = Context.NextState;
					
					return true;
				}
				
				State = Context.NextState;
			}
		}
		
		private bool GetCharacter()
		{
			if ((InputCharacter = NextCharacter()) != -1)
				return true;
			
			EndOfInput = true;
			return false;
		}
		
		private int NextCharacter()
		{
			if (InputBuffer != 0)
			{
				int tmp = InputBuffer;
				InputBuffer = 0;
				
				return tmp;
			}
			
			return Reader.Read();
		}
		
		private void PreviousCharacter()
		{
			InputBuffer = InputCharacter;
		}
		
		private static void PopulateStateTables()
		{
			stateHandlerTable = new Func<StateContext, bool>[28] {
				State1,
				State2,
				State3,
				State4,
				State5,
				State6,
				State7,
				State8,
				State9,
				State10,
				State11,
				State12,
				State13,
				State14,
				State15,
				State16,
				State17,
				State18,
				State19,
				State20,
				State21,
				State22,
				State23,
				State24,
				State25,
				State26,
				State27,
				State28
			};
			
			stateReturnTable = new int[28] {
				(int)JsonParserToken.Char,
				0,
				(int)JsonParserToken.Number,
				(int)JsonParserToken.Number,
				0,
				(int)JsonParserToken.Number,
				0,
				(int)JsonParserToken.Number,
				0,
				0,
				(int)JsonParserToken.True,
				0,
				0,
				0,
				(int)JsonParserToken.False,
				0,
				0,
				(int)JsonParserToken.Null,
				(int)JsonParserToken.CharSeq,
				(int)JsonParserToken.Char,
				0,
				0,
				(int)JsonParserToken.CharSeq,
				(int)JsonParserToken.Char,
				0,
				0,
				0,
				0
			};
		}
		
		private static bool State1(StateContext context)
		{
			while (context.Parser.GetCharacter())
			{
				if (context.Parser.InputCharacter == ' ' ||
					context.Parser.InputCharacter >= '\t' && context.Parser.InputCharacter <= '\r')
					continue;
				
				if (context.Parser.InputCharacter >= '1' && context.Parser.InputCharacter <= '9')
				{
					context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
					context.NextState = 3;
					return true;
				}
				
				switch (context.Parser.InputCharacter)
				{
					case '"':
						context.NextState = 19;
						context.Return = true;
						return true;
					
					case ',':
					case ':':
					case '[':
					case ']':
					case '{':
					case '}':
						context.NextState = 1;
						context.Return = true;
						return true;
					
					case '-':
						context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
						context.NextState = 2;
						return true;
					
					case '0':
						context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
						context.NextState = 4;
						return true;
					
					case 'f':
						context.NextState = 12;
						return true;
					
					case 'n':
						context.NextState = 16;
						return true;
					
					case 't':
						context.NextState = 9;
						return true;
					
					case '\'':
						if (!context.Parser.AllowSingleQuotedStrings)
							return false;
						
						context.Parser.InputCharacter = '"';
						context.NextState = 23;
						context.Return = true;
						return true;
					
					case '/':
						if (!context.Parser.AllowComments)
							return false;
						
						context.NextState = 25;
						return true;
					
					default:
						return false;
				}
			}
			
			return true;
		}
		
		private static bool State2(StateContext context)
		{
			context.Parser.GetCharacter();
			
			if (context.Parser.InputCharacter >= '1' && context.Parser.InputCharacter<= '9')
			{
				context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
				context.NextState = 3;
				return true;
			}

			switch (context.Parser.InputCharacter)
			{
				case '0':
					context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
					context.NextState = 4;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State3(StateContext context)
		{
			while (context.Parser.GetCharacter())
			{
				if (context.Parser.InputCharacter >= '0' && context.Parser.InputCharacter <= '9')
				{
					context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
					continue;
				}
				
				if (context.Parser.InputCharacter == ' ' ||
					context.Parser.InputCharacter >= '\t' && context.Parser.InputCharacter <= '\r')
				{
					context.Return = true;
					context.NextState = 1;
					return true;
				}
				
				switch (context.Parser.InputCharacter)
				{
					case ',':
					case ']':
					case '}':
						context.Parser.PreviousCharacter();
						context.Return = true;
						context.NextState = 1;
						return true;
					
					case '.':
						context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
						context.NextState = 5;
						return true;
					
					case 'e':
					case 'E':
						context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
						context.NextState = 7;
						return true;
					
					default:
						return false;
				}
			}
			
			return true;
		}
		
		private static bool State4(StateContext context)
		{
			context.Parser.GetCharacter();
			
			if (context.Parser.InputCharacter == ' ' ||
				context.Parser.InputCharacter >= '\t' && context.Parser.InputCharacter <= '\r')
			{
				context.Return = true;
				context.NextState = 1;
				return true;
			}
			
			switch (context.Parser.InputCharacter)
			{
				case ',':
				case ']':
				case '}':
					context.Parser.PreviousCharacter();
					context.Return = true;
					context.NextState = 1;
					return true;
				
				case '.':
					context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
					context.NextState = 5;
					return true;

				case 'e':
				case 'E':
					context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
					context.NextState = 7;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State5(StateContext context)
		{
			context.Parser.GetCharacter();
			
			if (context.Parser.InputCharacter >= '0' && context.Parser.InputCharacter <= '9')
			{
				context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
				context.NextState = 6;
				return true;
			}
			
			return false;
		}
		
		private static bool State6(StateContext context)
		{
			while (context.Parser.GetCharacter())
			{
				if (context.Parser.InputCharacter >= '0' && context.Parser.InputCharacter <= '9')
				{
					context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
					continue;
				}
				
				if (context.Parser.InputCharacter == ' ' ||
					context.Parser.InputCharacter >= '\t' && context.Parser.InputCharacter <= '\r')
				{
					context.Return = true;
					context.NextState = 1;
					return true;
				}
				
				switch (context.Parser.InputCharacter)
				{
					case ',':
					case ']':
					case '}':
						context.Parser.PreviousCharacter();
						context.Return = true;
						context.NextState = 1;
						return true;
					
					case 'e':
					case 'E':
						context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
						context.NextState = 7;
						return true;
					
					default:
						return false;
				}
			}
			
			return true;
		}
		
		private static bool State7(StateContext context)
		{
			context.Parser.GetCharacter();
			
			if (context.Parser.InputCharacter >= '0' && context.Parser.InputCharacter<= '9')
			{
				context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
				context.NextState = 8;
				return true;
            }
			
			switch (context.Parser.InputCharacter)
			{
				case '+':
				case '-':
					context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
					context.NextState = 8;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State8(StateContext context)
		{
			while (context.Parser.GetCharacter())
			{
				if (context.Parser.InputCharacter >= '0' && context.Parser.InputCharacter<= '9')
				{
					context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
					continue;
				}
				
				if (context.Parser.InputCharacter == ' ' ||
					context.Parser.InputCharacter >= '\t' && context.Parser.InputCharacter<= '\r')
				{
					context.Return = true;
					context.NextState = 1;
					return true;
				}
				
				switch (context.Parser.InputCharacter)
				{
					case ',':
					case ']':
					case '}':
						context.Parser.PreviousCharacter();
						context.Return = true;
						context.NextState = 1;
						return true;
					
					default:
						return false;
				}
			}

			return true;
		}
		
		private static bool State9(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'r':
					context.NextState = 10;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State10(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'u':
					context.NextState = 11;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State11(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'e':
					context.Return = true;
					context.NextState = 1;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State12(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'a':
					context.NextState = 13;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State13(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'l':
					context.NextState = 14;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State14(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 's':
					context.NextState = 15;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State15(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'e':
					context.Return = true;
					context.NextState = 1;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State16(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'u':
					context.NextState = 17;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State17(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'l':
					context.NextState = 18;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State18(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'l':
					context.Return = true;
					context.NextState = 1;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State19(StateContext context)
		{
			while (context.Parser.GetCharacter())
			{
				switch (context.Parser.InputCharacter)
				{
					case '"':
						context.Parser.PreviousCharacter();
						context.Return = true;
						context.NextState = 20;
						return true;
					
					case '\\':
						context.StateStack = 19;
						context.NextState = 21;
						return true;
					
					default:
						context.Parser.StringBuffer.Append((char)context.Parser.InputCharacter);
						continue;
				}
			}
			
			return true;
		}
		
		private static bool State20(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case '"':
					context.Return = true;
					context.NextState = 1;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State21(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case 'u':
					context.NextState = 22;
					return true;
				
				case '"':
				case '\'':
				case '/':
				case '\\':
				case 'b':
				case 'f':
				case 'n':
				case 'r':
				case 't':
					context.Parser.StringBuffer.Append(ProcessEscChar(context.Parser.InputCharacter));
					context.NextState = context.StateStack;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State22(StateContext context)
		{
			int counter = 0;
			int mult = 4096;
			
			context.Parser.UnicodeCharacter = 0;
			
			while (context.Parser.GetCharacter())
			{
				if (context.Parser.InputCharacter >= '0' && context.Parser.InputCharacter <= '9' ||
					context.Parser.InputCharacter >= 'A' && context.Parser.InputCharacter <= 'F' ||
					context.Parser.InputCharacter >= 'a' && context.Parser.InputCharacter <= 'f')
				{
					
					context.Parser.UnicodeCharacter += HexValue(context.Parser.InputCharacter) * mult;
					
					counter++;
					mult /= 16;
					
					if (counter == 4)
					{
						context.Parser.StringBuffer.Append(Convert.ToChar(context.Parser.UnicodeCharacter));
						context.NextState = context.StateStack;
						return true;
					}
					
					continue;
				}
				
				return false;
			}
			
			return true;
		}
		
		private static bool State23(StateContext context)
		{
			while (context.Parser.GetCharacter())
			{
				switch (context.Parser.InputCharacter)
				{
					case '\'':
						context.Parser.PreviousCharacter();
						context.Return = true;
						context.NextState = 24;
						return true;
					
					case '\\':
						context.StateStack = 23;
						context.NextState = 21;
						return true;
					
					default:
						context.Parser.StringBuffer.Append((char) context.Parser.InputCharacter);
						continue;
				}
			}
			
			return true;
		}
		
		private static bool State24(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case '\'':
					context.Parser.InputCharacter = '"';
					context.Return = true;
					context.NextState = 1;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State25(StateContext context)
		{
			context.Parser.GetCharacter();
			
			switch (context.Parser.InputCharacter)
			{
				case '*':
					context.NextState = 27;
					return true;
				
				case '/':
					context.NextState = 26;
					return true;
				
				default:
					return false;
			}
		}
		
		private static bool State26(StateContext context)
		{
			while (context.Parser.GetCharacter())
			{
				if (context.Parser.InputCharacter == '\n')
				{
					context.NextState = 1;
					return true;
				}
			}
			
			return true;
		}
		
		private static bool State27(StateContext context)
		{
			while (context.Parser.GetCharacter())
			{
				if (context.Parser.InputCharacter == '*')
				{
					context.NextState = 28;
					return true;
				}
			}
			
			return true;
		}
		
		private static bool State28(StateContext context)
		{
			while (context.Parser.GetCharacter())
			{
				if (context.Parser.InputCharacter == '*')
					continue;
				
				if (context.Parser.InputCharacter == '/')
				{
					context.NextState = 1;
					return true;
				}
				
				context.NextState = 27;
				return true;
			}
			
			return true;
		}
		
		private static char ProcessEscChar(int esc_char)
		{
			switch (esc_char)
			{
				case '"':
				case '\'':
				case '\\':
				case '/':
					return Convert.ToChar(esc_char);
				
				case 'n':
					return '\n';
				
				case 't':
					return '\t';
				
				case 'r':
					return '\r';
				
				case 'b':
					return '\b';
				
				case 'f':
					return '\f';
				
				default:
					// Unreachable
					return '?';
			}
		}
		
		private static int HexValue(int digit)
		{
			switch (digit)
			{
				case 'a':
				case 'A':
					return 10;
				
				case 'b':
				case 'B':
					return 11;
				
				case 'c':
				case 'C':
					return 12;
				
				case 'd':
				case 'D':
					return 13;
				
				case 'e':
				case 'E':
					return 14;
				
				case 'f':
				case 'F':
					return 15;
				
				default:
					return digit - '0';
			}
		}
	}
	
	internal class StateContext
    {
		public bool Return;
		public int NextState;
		public JsonParser Parser;
		public int StateStack;
    }
}
