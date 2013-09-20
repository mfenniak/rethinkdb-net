using System;
using System.ComponentModel;
using RethinkDb.Spec;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IGroupByReduction<TReductionType>
    {
        Term GenerateReductionObject(IDatumConverterFactory datumConverterFactory);
    }
}

