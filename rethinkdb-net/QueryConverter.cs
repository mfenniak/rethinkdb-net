using System;

namespace RethinkDb
{
    public class QueryConverter : IQueryConverter
    {
        private IDatumConverterFactory delegatedDatumConverterFactory;
        private IExpressionConverterFactory delegatedExpressionConverterFactory;

        public QueryConverter(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            this.delegatedDatumConverterFactory = datumConverterFactory;
            this.delegatedExpressionConverterFactory = expressionConverterFactory;
        }

        #region IExpressionConverterFactory implementation

        public IExpressionConverterZeroParameter<TReturn> CreateExpressionConverter<TReturn>(IDatumConverterFactory datumConverterFactory)
        {
            return delegatedExpressionConverterFactory.CreateExpressionConverter<TReturn>(datumConverterFactory);
        }

        public IExpressionConverterOneParameter<TParameter1, TReturn> CreateExpressionConverter<TParameter1, TReturn>(IDatumConverterFactory datumConverterFactory)
        {
            return delegatedExpressionConverterFactory.CreateExpressionConverter<TParameter1, TReturn>(datumConverterFactory);
        }

        public IExpressionConverterTwoParameter<TParameter1, TParameter2, TReturn> CreateExpressionConverter<TParameter1, TParameter2, TReturn>(IDatumConverterFactory datumConverterFactory)
        {
            return delegatedExpressionConverterFactory.CreateExpressionConverter<TParameter1, TParameter2, TReturn>(datumConverterFactory);
        }

        #endregion
        #region IDatumConverterFactory implementation

        public bool TryGet(Type datumType, IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter datumConverter)
        {
            return delegatedDatumConverterFactory.TryGet(datumType, rootDatumConverterFactory, out datumConverter);
        }

        public bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            return delegatedDatumConverterFactory.TryGet<T>(rootDatumConverterFactory, out datumConverter);
        }

        #endregion
    }
}
