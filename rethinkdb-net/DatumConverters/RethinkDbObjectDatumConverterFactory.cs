using System;
using System.Collections.Generic;
using System.Linq;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public class RethinkDbObjectDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly RethinkDbObjectDatumConverterFactory Instance = new RethinkDbObjectDatumConverterFactory();

        private RethinkDbObjectDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            if (typeof(T) == typeof(RethinkDbObject))
            {
                datumConverter = (IDatumConverter<T>)new RethinkDbObjectDatumConverter(rootDatumConverterFactory);
                return true;
            }

            datumConverter = null;
            return false;
        }

        private class RethinkDbObjectDatumConverter : AbstractReferenceTypeDatumConverter<RethinkDbObject>
        {
            private IDatumConverter<Dictionary<string, object>> dictionaryDatumConverter;

            public RethinkDbObjectDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
            {
                this.dictionaryDatumConverter = rootDatumConverterFactory.Get<Dictionary<string, object>>();
            }

            public override RethinkDbObject ConvertDatum(Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                return new RethinkDbObject(dictionaryDatumConverter.ConvertDatum(datum));
            }

            public override Datum ConvertObject(RethinkDbObject value)
            {
                if (value == null)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                return dictionaryDatumConverter.ConvertObject(value.InnerDictionary);
            }
        }
    }
}
