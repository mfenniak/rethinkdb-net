using System.Collections.Generic;

namespace RethinkDb.DatumConverters
{
    public class AggregateDatumConverterFactory : AbstractDatumConverterFactory
    {
        private readonly IList<IDatumConverterFactory> datumConverterFactories;

        public AggregateDatumConverterFactory(params IDatumConverterFactory[] datumConverterFactories)
        {
            this.datumConverterFactories = new List<IDatumConverterFactory>(datumConverterFactories);
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
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

