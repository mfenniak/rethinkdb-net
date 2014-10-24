// Copyright 2010 Dale Ragan (@dragan)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
