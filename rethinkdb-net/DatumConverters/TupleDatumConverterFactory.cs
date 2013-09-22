using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace RethinkDb
{
    public class TupleDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly TupleDatumConverterFactory Instance = new TupleDatumConverterFactory();

        private TupleDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");
            if (!IsTypeSupported(typeof(T)))
                return false;

            datumConverter = new TupleConverter<T>(rootDatumConverterFactory);
            return true;
        }

        public bool IsTypeSupported(Type t)
        {
            if (!t.IsGenericType)
                return false;

            var gtd = t.GetGenericTypeDefinition();
            return
                gtd.GetGenericTypeDefinition().Equals(typeof(Tuple<>)) ||
                gtd.GetGenericTypeDefinition().Equals(typeof(Tuple<,>)) ||
                gtd.GetGenericTypeDefinition().Equals(typeof(Tuple<,,>)) ||
                gtd.GetGenericTypeDefinition().Equals(typeof(Tuple<,,,>)) ||
                gtd.GetGenericTypeDefinition().Equals(typeof(Tuple<,,,,>)) ||
                gtd.GetGenericTypeDefinition().Equals(typeof(Tuple<,,,,,>)) ||
                gtd.GetGenericTypeDefinition().Equals(typeof(Tuple<,,,,,,>));
        }

        private class TupleConverter<T> : AbstractReferenceTypeDatumConverter<T>
        {
            private readonly ConstructorInfo tupleConstructor;
            private readonly IDatumConverter[] itemConverters;

            public TupleConverter(IDatumConverterFactory rootDatumConverterFactory)
            {
                var typeArguments = typeof(T).GetGenericArguments();
                tupleConstructor = typeof(T).GetConstructor(typeArguments);
                itemConverters = new IDatumConverter[typeArguments.Length];
                for (int i = 0; i < typeArguments.Length; i++)
                    itemConverters[i] = rootDatumConverterFactory.Get(typeArguments[i]);
            }

            #region IDatumConverter<T> Members

            public override T ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                {
                    return default(T);
                }
                else if (datum.type == Spec.Datum.DatumType.R_OBJECT)
                {
                    if (itemConverters.Length != 2)
                        throw new NotSupportedException("TupleDatumConverter only supports OBJECT values if it's a two-tuple; this one is a " + typeof(T).FullName);

                    object item1 = null;
                    object item2 = null;

                    foreach (var assocPair in datum.r_object)
                    {
                        // left/right for a join
                        if (assocPair.key == "left")
                            item1 = itemConverters[0].ConvertDatum(assocPair.val);
                        else if (assocPair.key == "right")
                            item2 = itemConverters[1].ConvertDatum(assocPair.val);

                        // group/reduction for a grouped map reduce
                        else if (assocPair.key == "group")
                            item1 = itemConverters[0].ConvertDatum(assocPair.val);
                        else if (assocPair.key == "reduction")
                            item2 = itemConverters[1].ConvertDatum(assocPair.val);
                        else
                            throw new InvalidOperationException("Unexpected key/value pair in tuple object: " + assocPair.key + "; expected left/right or group/reduction");
                    }

                    return (T)(tupleConstructor.Invoke(new object[] { item1, item2 }));
                }
                else if (datum.type == Spec.Datum.DatumType.R_ARRAY)
                {
                    if (itemConverters.Length != datum.r_array.Count)
                        throw new InvalidOperationException(String.Format("Unexpected array of length {0} where tuple of type {1} was expected", datum.r_array.Count, typeof(T)));

                    object[] values = new object[itemConverters.Length];
                    for (int i = 0; i < itemConverters.Length; i++)
                        values[i] = itemConverters[i].ConvertDatum(datum.r_array[i]);
                    return (T)(tupleConstructor.Invoke(values));
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to tuple, but Datum was unsupported type " + datum.type);
            }

            public override Spec.Datum ConvertObject(T arrayObject)
            {
                throw new NotSupportedException("TupleDatumConverterFactory only supports Datum->Tuple currently");
            }

            #endregion
        }
    }
}
