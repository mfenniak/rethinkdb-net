using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace RethinkDb
{
    public class TupleDatumConverterFactory
    {
        public static readonly TupleDatumConverterFactory Instance = new TupleDatumConverterFactory();
        private static readonly IDictionary<Type, IDictionary<IDatumConverterFactory, object>> tupleConverterCache = new Dictionary<Type, IDictionary<IDatumConverterFactory, object>>();

        private TupleDatumConverterFactory()
        {
        }

        public IDatumConverter<T> Get<T>(IDatumConverterFactory innerTypeConverterFactory)
        {
            if (IsTypeSupported(typeof(T)))
            {
                IDictionary<IDatumConverterFactory, object> datumConverterBasedCache;
                if (!tupleConverterCache.TryGetValue(typeof(T), out datumConverterBasedCache))
                    tupleConverterCache[typeof(T)] = datumConverterBasedCache = new Dictionary<IDatumConverterFactory, object>();

                object retval;
                if (!datumConverterBasedCache.TryGetValue(innerTypeConverterFactory, out retval))
                    datumConverterBasedCache[innerTypeConverterFactory] = retval = new TupleConverter<T>(innerTypeConverterFactory);

                return (IDatumConverter<T>)retval;
            }
            else if (typeof(T).FullName.StartsWith("System.Tuple"))
                throw new NotSupportedException("Unsupported Tuple type: " + typeof(T) + "; only two-item Tuples are currently supported for left/right ReQL return values");
            else
                throw new NotSupportedException("Unsupported type in TupleDatumConverterFactory: " + typeof(T));
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

        // FIXME: This TupleConverter, using reflection, is likely to be many, many times slower than doing an emitted
        // class like DataContractDatumConverterFactory does.
        private class TupleConverter<T> : IDatumConverter<T>
        {
            private readonly ConstructorInfo tupleConstructor;
            private readonly object[] itemConverters;

            public TupleConverter(IDatumConverterFactory innerTypeConverterFactory)
            {
                var genericGetMethod = innerTypeConverterFactory.GetType().GetMethod("Get");

                var typeArguments = typeof(T).GetGenericArguments();
                tupleConstructor = typeof(T).GetConstructor(typeArguments);
                itemConverters = new object[typeArguments.Length];
                for (int i = 0; i < typeArguments.Length; i++)
                    itemConverters[i] = genericGetMethod.MakeGenericMethod(typeArguments[i]).Invoke(innerTypeConverterFactory, new object[0]);
            }

            private object ReflectedConversion(Spec.Datum datum, dynamic typeDatumConverter)
            {
                return typeDatumConverter.ConvertDatum(datum);
            }

            #region IDatumConverter<T> Members

            public T ConvertDatum(Spec.Datum datum)
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
                            item1 = ReflectedConversion(assocPair.val, itemConverters[0]);
                        else if (assocPair.key == "right")
                            item2 = ReflectedConversion(assocPair.val, itemConverters[1]);

                        // group/reduction for a grouped map reduce
                        else if (assocPair.key == "group")
                            item1 = ReflectedConversion(assocPair.val, itemConverters[0]);
                        else if (assocPair.key == "reduction")
                            item2 = ReflectedConversion(assocPair.val, itemConverters[1]);
                        else
                            throw new InvalidOperationException("Unexpected key/value pair in tuple object: " + assocPair.key + "; expected left/right or group/reduction");
                    }

                    return (T)(tupleConstructor.Invoke(new object[] { item1, item2 }));
                }
                else if (datum.type == Spec.Datum.DatumType.R_ARRAY)
                {
                    if (itemConverters.Length != datum.r_array.Count)
                        return default(T);
                        //throw new InvalidOperationException(String.Format("Unexpected array of length {0} where tuple of type {1} was expected", datum.r_array.Count, typeof(T)));

                    object[] values = new object[itemConverters.Length];
                    for (int i = 0; i < itemConverters.Length; i++)
                        values[i] = ReflectedConversion(datum.r_array[i], itemConverters[i]);
                    return (T)(tupleConstructor.Invoke(values));
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to tuple, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(T arrayObject)
            {
                throw new NotSupportedException("TupleDatumConverterFactory only supports Datum->Tuple currently");
            }

            #endregion
        }
    }
}
