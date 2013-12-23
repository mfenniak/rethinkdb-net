using System;
using System.ComponentModel;
using RethinkDb.Spec;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IQuery
    {
        Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory);
    }
}

