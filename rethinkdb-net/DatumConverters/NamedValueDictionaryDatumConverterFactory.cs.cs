using System;
using System.Collections.Generic;

namespace RethinkDb.DatumConverters
{
    public class NamedValueDictionaryDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly NamedValueDictionaryDatumConverterFactory Instance = new NamedValueDictionaryDatumConverterFactory();

        private NamedValueDictionaryDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;
            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");

            if (typeof(T) == typeof(Dictionary<string, object>))
            {
                datumConverter = (IDatumConverter<T>)new NamedValueDictionaryDatumConverter(rootDatumConverterFactory);
                return true;
            }

            return false;
        }
    }

    public class NamedValueDictionaryDatumConverter : AbstractReferenceTypeDatumConverter<Dictionary<string, object>>
    {
        private IDatumConverterFactory rootDatumConverterFactory;
        private Dictionary<Type, IDatumConverter> datumConverterCache = new Dictionary<Type, IDatumConverter>();

        public NamedValueDictionaryDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
        {
            this.rootDatumConverterFactory = rootDatumConverterFactory;
        }

        #region IDatumConverter<T> Members

        private IDatumConverter GetConverter(Type type)
        {
            IDatumConverter valueConverter = null;

            if (datumConverterCache.TryGetValue(type, out valueConverter))
                return valueConverter;

            datumConverterCache[type] = valueConverter = rootDatumConverterFactory.Get(type);
            return valueConverter;
        }

        public override Dictionary<string, object> ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Spec.Datum.DatumType.R_NULL)
                return null;

            if (datum.type != RethinkDb.Spec.Datum.DatumType.R_OBJECT)
                throw new NotSupportedException("Attempted to convert Datum to named-value dictionary, but Datum was unsupported type " + datum.type);

            Dictionary<string, object> retval = new Dictionary<string, object>();

            foreach (var kvp in datum.r_object)
            {
                Type valueType = rootDatumConverterFactory.GetBestNativeTypeForDatum(kvp.val);
                var valueConverter = GetConverter(valueType);
                retval [kvp.key] = valueConverter.ConvertDatum(kvp.val);
            }

            return retval;
        }

        public override Spec.Datum ConvertObject(Dictionary<string, object> dictionary)
        {
            if (dictionary == null)
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };

            var retval = new Spec.Datum() { type = Spec.Datum.DatumType.R_OBJECT };

            foreach (var kvp in dictionary)
            {
                var valueConverter = GetConverter(kvp.Value.GetType());
                var convertedValue = valueConverter.ConvertObject(kvp.Value);
                retval.r_object.Add(new RethinkDb.Spec.Datum.AssocPair()
                {
                    key = kvp.Key,
                    val = convertedValue,
                });
            }

            return retval;
        }

        #endregion
    }
}
