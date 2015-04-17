using System;
using System.Collections.Generic;
using System.Linq;

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

            if (typeof(T) == typeof(Dictionary<string, object>.KeyCollection))
            {
                datumConverter = (IDatumConverter<T>)new NamedValueDictionaryKeysDatumConverter(rootDatumConverterFactory);
                return true;
            }

            if (typeof(T) == typeof(Dictionary<string, object>.ValueCollection))
            {
                datumConverter = (IDatumConverter<T>)new NamedValueDictionaryValuesDatumConverter(new NamedValueDictionaryDatumConverter(rootDatumConverterFactory));
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

    public class NamedValueDictionaryKeysDatumConverter : AbstractReferenceTypeDatumConverter<Dictionary<string, object>.KeyCollection>
    {
        private IDatumConverter<string[]> arrayDatumConverter;

        public NamedValueDictionaryKeysDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
        {
            arrayDatumConverter = rootDatumConverterFactory.Get<string[]>();
        }

        #region IDatumConverter<T> Members

        public override Dictionary<string, object>.KeyCollection ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Spec.Datum.DatumType.R_NULL)
                return null;

            string[] keys = arrayDatumConverter.ConvertDatum(datum);
            return keys.ToDictionary(k => k, k => (object)null).Keys;
        }

        public override Spec.Datum ConvertObject(Dictionary<string, object>.KeyCollection keyCollection)
        {
            if (keyCollection == null)
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
            throw new NotSupportedException("Converting a KeyCollection to a Datum not currently supported");
        }

        #endregion
    }

    // There's no RethinkDB command to get the "values" of an object; so, this datum converter will function on the actual
    // object (both keys and values) and extract the values collection client-side.
    public class NamedValueDictionaryValuesDatumConverter : AbstractReferenceTypeDatumConverter<Dictionary<string, object>.ValueCollection>
    {
        private NamedValueDictionaryDatumConverter dictionaryConverter;

        public NamedValueDictionaryValuesDatumConverter(NamedValueDictionaryDatumConverter dictionaryConverter)
        {
            this.dictionaryConverter = dictionaryConverter;
        }

        #region IDatumConverter<T> Members

        public override Dictionary<string, object>.ValueCollection ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Spec.Datum.DatumType.R_NULL)
                return null;
            return dictionaryConverter.ConvertDatum(datum).Values;
        }

        public override Spec.Datum ConvertObject(Dictionary<string, object>.ValueCollection valueCollection)
        {
            if (valueCollection == null)
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
            throw new NotSupportedException("Converting a ValueCollection to a Datum not currently supported");
        }

        #endregion
    }
}
