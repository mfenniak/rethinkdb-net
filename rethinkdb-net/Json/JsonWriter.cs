using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SineSignal.Ottoman.Serialization
{
	public sealed class JsonWriter
	{
		private static NumberFormatInfo numberFormatInfo;
		
		//private StringBuilder internalStringBuilder;
		private TextWriter writer;
		private Stack<Scope> scopes;
		
		static JsonWriter ()
        {
            numberFormatInfo = NumberFormatInfo.InvariantInfo;
        }
		
        public JsonWriter(TextWriter writer)
		{
            this.writer = writer;
			//internalStringBuilder = new StringBuilder();
			//writer = new StringWriter(internalStringBuilder);
			scopes = new Stack<Scope>();
		}
		
		public void BeginObject()
		{
			BeginObjectScope();
		}
		
		public void BeginArray()
		{
			BeginArrayScope();
		}
		
		public void WriteMember(string name)
		{
			PutComma(ScopeType.Object);
			PutStringValue(name);
			PutNameSeparator();
		}
		
		public void WriteBoolean(bool boolean)
		{
			PutValue(boolean ? "true" : "false");
		}
		
		public void WriteString(string text)
		{
			if (text != null)
			{
				PutStringValue(text);
			}
			else
			{
				PutValue("null");
			}
		}

        public void WriteNull()
        {
            PutValue("null");
        }
		
		public void WriteNumber(int number)
		{
			PutValue(Convert.ToString(number, numberFormatInfo));
		}
		
		public void WriteNumber(long number)
		{
			PutValue(Convert.ToString(number, numberFormatInfo));
		}
		
		// This makes us not CLS Compliant
		public void WriteNumber(ulong number)
		{
			PutValue(Convert.ToString(number, numberFormatInfo));
        }
		
		public void WriteNumber(float number)
		{
			PutValue(Convert.ToString(number, numberFormatInfo));
		}
		
		public void WriteNumber(double number)
		{
			PutValue(Convert.ToString(number, numberFormatInfo));
		}
		
		public void WriteNumber(decimal number)
		{
			PutValue(Convert.ToString(number, numberFormatInfo));
		}
		
		public void EndObject()
		{
			EndScope();
		}
		
		public void EndArray()
		{
			EndScope();
		}
		
		private void PutValue(string text)
		{
			PutComma(ScopeType.Array);
			writer.Write(text);
		}
		
		private void PutStringValue(string text)
		{
			PutValue(String.Format("\"{0}\"", EscapeString(text)));
		}
		
		private string EscapeString(string text)
		{
			var stringBuilder = new StringBuilder();
			for (int index = 0; index < text.Length; index++)
			{
				char character = text[index];
				
				// Special Character, need to escape
				switch (character)
				{
					case '\n':
						stringBuilder.Append("\\n");
						continue;
					
					case '\r':
						stringBuilder.Append("\\r");
						continue;
					
					case '\t':
						stringBuilder.Append("\\t");
						continue;
	                
					case '"':
					case '\\':
						stringBuilder.Append('\\');
						stringBuilder.Append(character);
						continue;
					
					case '\f':
						stringBuilder.Append("\\f");
						continue;
					
					case '\b':
						stringBuilder.Append("\\b");
						continue;
				}
				
				// Unicode Character
				if (character < ' ' || character > 0x7F)
				{
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:X4}", (int)character);
					continue;
				}
				
				// ASCII Character
				stringBuilder.Append(character);
			}
			
			return stringBuilder.ToString();
		}
		
		private void BeginObjectScope()
		{
			BeginScope(ScopeType.Object);
		}
		
		private void BeginArrayScope()
		{
			BeginScope(ScopeType.Array);
		}
		
		private void BeginScope(ScopeType scopeType)
		{
			PutComma(ScopeType.Array);
			
			var scope = new Scope(scopeType);
			scopes.Push(scope);
			
			if (scopeType == ScopeType.Object)
			{
				writer.Write('{');
			}
			else if (scope.Type == ScopeType.Array)
			{
				writer.Write('[');
			}
		}
		
		private void EndScope()
		{
			Scope scope = scopes.Pop();
			if (scope.Type == ScopeType.Object)
			{
				writer.Write('}');
			}
			else if (scope.Type == ScopeType.Array)
			{
				writer.Write(']');
			}
		}
		
		private void PutNameSeparator()
		{
			writer.Write(':');
		}
		
		private void PutComma(ScopeType scopeType)
		{
			if (scopes.Count != 0)
			{
				Scope currentScope = scopes.Peek();
				if (currentScope.Type == scopeType && 
					currentScope.ObjectCount != 0)
				{
					writer.Write(',');
				}
				currentScope.ObjectCount++;
			}
		}
		
		private enum ScopeType
		{
			Object = 0,
			Array = 1
		}
		
		private sealed class Scope
		{
			public ScopeType Type { get; private set; }
			public int ObjectCount { get; set; }
			
			public Scope(ScopeType scopeType)
			{
				Type = scopeType;
			}
		}
	}
}
