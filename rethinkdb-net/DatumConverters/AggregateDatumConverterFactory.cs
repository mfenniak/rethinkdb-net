using System.Collections.Generic;

namespace RethinkDb
{
    public class AggregateDatumConverterFactory : IDatumConverterFactory
    {
        private readonly IList<IDatumConverterFactory> datumConverterFactories;

        public AggregateDatumConverterFactory(params IDatumConverterFactory[] datumConverterFactories)
        {
            this.datumConverterFactories = new List<IDatumConverterFactory>(datumConverterFactories);
        }

        public bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;
            foreach (var factory in datumConverterFactories)
            {
                if (factory.TryGet<T>(rootDatumConverterFactory, out datumConverter))
                    return true;
            }
            return false;
        }
    }
}

