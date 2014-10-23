namespace SineSignal.Ottoman.Serialization
{
	internal enum JsonParserToken
	{
		// JsonLexer tokens
		None = System.Char.MaxValue + 1,
		Number,
		True,
		False,
		Null,
		CharSeq,
		// Single char
		Char,
		
		// Parser Rules
		Text,
		Object,
		ObjectPrime,
		Pair,
		PairRest,
		Array,
		ArrayPrime,
		Value,
		ValueRest,
		String,
		
		// End of input
		End,
		
		// The empty rule
		Epsilon
    }
}
