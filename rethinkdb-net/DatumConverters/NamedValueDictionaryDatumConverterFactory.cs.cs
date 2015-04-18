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

            if (typeof(T).IsGenericType &&
                typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                typeof(T).GetGenericArguments() [0] == typeof(string))
            {
                var specificType = typeof(NamedValueDictionaryDatumConverter<>).MakeGenericType(typeof(T).GetGenericArguments()[1]);
                var dc = Activator.CreateInstance(specificType, new object[] { rootDatumConverterFactory });
                datumConverter = (IDatumConverter<T>)dc;
                return true;
            }

            if (typeof(T).IsGenericType &&
                typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>.KeyCollection) &&
                typeof(T).GetGenericArguments() [0] == typeof(string))
            {
                var specificType = typeof(NamedValueDictionaryKeysDatumConverter<>).MakeGenericType(typeof(T).GetGenericArguments()[1]);
                var dc = Activator.CreateInstance(specificType, new object[] { rootDatumConverterFactory });
                datumConverter = (IDatumConverter<T>)dc;
                return true;
            }

            if (typeof(T).IsGenericType &&
                typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>.ValueCollection) &&
                typeof(T).GetGenericArguments()[0] == typeof(string))
            {
                var dictionaryConverterType = typeof(NamedValueDictionaryDatumConverter<>).MakeGenericType(typeof(T).GetGenericArguments()[1]);
                var dictionaryConverter = Activator.CreateInstance(dictionaryConverterType, new object[] { rootDatumConverterFactory });

                var specificType = typeof(NamedValueDictionaryValuesDatumConverter<>).MakeGenericType(typeof(T).GetGenericArguments()[1]);
                var dc = Activator.CreateInstance(specificType, new object[] { dictionaryConverter });
                datumConverter = (IDatumConverter<T>)dc;
                return true;
            }

            return false;
        }
    }

    public class NamedValueDictionaryDatumConverter<TValue> : AbstractReferenceTypeDatumConverter<Dictionary<string, TValue>>
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

        public override Dictionary<string, TValue> ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Spec.Datum.DatumType.R_NULL)
                return null;

            if (datum.type != RethinkDb.Spec.Datum.DatumType.R_OBJECT)
                throw new NotSupportedException("Attempted to convert Datum to named-value dictionary, but Datum was unsupported type " + datum.type);

            Dictionary<string, TValue> retval = new Dictionary<string, TValue>();

            IDatumConverter valueConverter = null;
            if (typeof(TValue) != typeof(object))
                valueConverter = rootDatumConverterFactory.Get<TValue>();

            foreach (var kvp in datum.r_object)
            {
                IDatumConverter thisValueConverter = valueConverter;
                if (thisValueConverter == null)
                {
                    Type valueType = rootDatumConverterFactory.GetBestNativeTypeForDatum(kvp.val);
                    thisValueConverter = GetConverter(valueType);
                }
                retval[kvp.key] = (TValue)thisValueConverter.ConvertDatum(kvp.val);
            }

            return retval;
        }

        public override Spec.Datum ConvertObject(Dictionary<string, TValue> dictionary)
        {
            if (dictionary == null)
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };

            var retval = new Spec.Datum() { type = Spec.Datum.DatumType.R_OBJECT };

            foreach (var kvp in dictionary)
            {
                IDatumConverter valueConverter;
                if ((object)kvp.Value == null)
                {
                    // can't call kvp.Value.GetType() if it's null!
                    valueConverter = GetConverter(typeof(object));
                }
                else
                    valueConverter = GetConverter(kvp.Value.GetType());
                
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

    public class NamedValueDictionaryKeysDatumConverter<TValue> : AbstractReferenceTypeDatumConverter<Dictionary<string, TValue>.KeyCollection>
    {
        private IDatumConverter<string[]> arrayDatumConverter;

        public NamedValueDictionaryKeysDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
        {
            arrayDatumConverter = rootDatumConverterFactory.Get<string[]>();
        }

        #region IDatumConverter<T> Members

        public override Dictionary<string, TValue>.KeyCollection ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Spec.Datum.DatumType.R_NULL)
                return null;

            string[] keys = arrayDatumConverter.ConvertDatum(datum);
            return keys.ToDictionary(k => k, k => default(TValue)).Keys;
        }

        public override Spec.Datum ConvertObject(Dictionary<string, TValue>.KeyCollection keyCollection)
        {
            if (keyCollection == null)
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
            throw new NotSupportedException("Converting a KeyCollection to a Datum not currently supported");
        }

        #endregion
    }

    // There's no RethinkDB command to get the "values" of an object; so, this datum converter will function on the actual
    // object (both keys and values) and extract the values collection client-side.
    public class NamedValueDictionaryValuesDatumConverter<TValue> : AbstractReferenceTypeDatumConverter<Dictionary<string, TValue>.ValueCollection>
    {
        private NamedValueDictionaryDatumConverter<TValue> dictionaryConverter;

        public NamedValueDictionaryValuesDatumConverter(NamedValueDictionaryDatumConverter<TValue> dictionaryConverter)
        {
            this.dictionaryConverter = dictionaryConverter;
        }

        #region IDatumConverter<T> Members

        public override Dictionary<string, TValue>.ValueCollection ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Spec.Datum.DatumType.R_NULL)
                return null;
            return dictionaryConverter.ConvertDatum(datum).Values;
        }

        public override Spec.Datum ConvertObject(Dictionary<string, TValue>.ValueCollection valueCollection)
        {
            if (valueCollection == null)
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
            throw new NotSupportedException("Converting a ValueCollection to a Datum not currently supported");
        }

        #endregion
    }
}
