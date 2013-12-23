using System;

namespace RethinkDb
{
    public interface IExpressionConverterFactory
    {
        IExpressionConverterZeroParameter<TReturn> CreateExpressionConverter<TReturn>(IDatumConverterFactory datumConverterFactory);
        IExpressionConverterOneParameter<TParameter1, TReturn> CreateExpressionConverter<TParameter1, TReturn>(IDatumConverterFactory datumConverterFactory);
        IExpressionConverterTwoParameter<TParameter1, TParameter2, TReturn> CreateExpressionConverter<TParameter1, TParameter2, TReturn>(IDatumConverterFactory datumConverterFactory);
    }
}
