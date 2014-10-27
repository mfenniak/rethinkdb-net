using System;

namespace RethinkDb
{
    // Just to avoid having to pass around multiple factory interfaces, this is a composed factory interface.
    public interface IQueryConverter : IDatumConverterFactory, IExpressionConverterFactory
    {
    }
}
