using System;
using System.Collections.Generic;
using System.Linq;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    // A special datum converter to support RethinkDB's $reql_type$ = GROUPED_DATA return values.
    public class GroupingDictionaryDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly GroupingDictionaryDatumConverterFactory Instance = new GroupingDictionaryDatumConverterFactory();

        private GroupingDictionaryDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;
            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IGroupingDictionary<,>))
            {
                Type converterType = typeof(GroupingDictionaryDatumConverter<,>).MakeGenericType(typeof(T).GetGenericArguments());
                datumConverter = (IDatumConverter<T>)Activator.CreateInstance(converterType, rootDatumConverterFactory);
                return true;
            }
            else
                return false;
        }
    }

    public class GroupingDictionaryDatumConverter<TKey, TValue> : AbstractReferenceTypeDatumConverter<IGroupingDictionary<TKey, TValue>>
    {
        private readonly IDatumConverter<TKey> keyTypeConverter;
        private readonly IDatumConverter<TValue> valueTypeConverter;

        public GroupingDictionaryDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
        {
            this.keyTypeConverter = rootDatumConverterFactory.Get<TKey>();
            this.valueTypeConverter = rootDatumConverterFactory.Get<TValue>();
        }

        #region IDatumConverter<T> Members

        public override IGroupingDictionary<TKey, TValue> ConvertDatum(Datum datum)
        {
            if (datum.type == Datum.DatumType.R_NULL)
            {
                return null;
            }
            else if (datum.type == Datum.DatumType.R_OBJECT)
            {
                var keys = datum.r_object.ToDictionary(kvp => kvp.key, kvp => kvp.val);

                Datum typeDatum;
                if (!keys.TryGetValue("$reql_type$", out typeDatum))
                    throw new NotSupportedException("Object without $reql_type$ key cannot be converted to a dictionary");
                if (typeDatum.type != Datum.DatumType.R_STR || typeDatum.r_str != "GROUPED_DATA")
                    throw new NotSupportedException("Object without $reql_type$ = GROUPED_DATA cannot be converted to a dictionary");

                Datum dataDatum;
                if (!keys.TryGetValue("data", out dataDatum))
                    throw new NotSupportedException("Object without data key cannot be converted to a dictionary");
                if (dataDatum.type != Datum.DatumType.R_ARRAY)
                    throw new NotSupportedException("Object's data key must be an array type");

                var retval = new GroupingDictionary<TKey, TValue>(dataDatum.r_array.Count);
                foreach (var item in dataDatum.r_array)
                {
                    if (item.type != Datum.DatumType.R_ARRAY || item.r_array.Count != 2)
                        throw new NotSupportedException("GROUPED_DATA data is expected to contain array elements of two items, a key and a value");
                    var key = keyTypeConverter.ConvertDatum(item.r_array[0]);
                    var value = valueTypeConverter.ConvertDatum(item.r_array[1]);
                    retval[key] = value;
                }

                return retval;
            }
            else
            {
                throw new NotSupportedException("Attempted to cast Datum to array, but Datum was unsupported type " + datum.type);
            }
        }

        public override Spec.Datum ConvertObject(IGroupingDictionary<TKey, TValue> dictionary)
        {
            //if (dictionary == null)
            //    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
            throw new NotImplementedException("IGroupingDictionary objects are only currently supported for reading Group results");
        }

        #endregion
    }

    class GroupingDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IGroupingDictionary<TKey, TValue>
    {
        public GroupingDictionary(int capacity)
            : base(capacity)
        {
        }
    }
}
