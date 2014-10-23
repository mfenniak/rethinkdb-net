using System;

namespace SineSignal.Ottoman.Serialization
{
	public class JsonException : Exception
	{
		public JsonException() : base()
		{
		}
		
		internal JsonException(int c) : 
			base(String.Format("Invalid character '{0}' in input string", (char)c))
		{
		}
		
		internal JsonException(JsonParserToken token, Exception innerException) : 
			base (String.Format("Invalid token '{0}' in input string", token), innerException)
		{
		}
		
		public JsonException(string message) : base (message)
		{
		}
		
		public JsonException(string message, Exception innerException) : 
			base (message, innerException)
		{
		}
	}
}
