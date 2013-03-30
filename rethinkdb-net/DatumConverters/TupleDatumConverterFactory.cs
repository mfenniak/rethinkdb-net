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
            if (typeof(T).GetGenericTypeDefinition().Equals(typeof(Tuple<,>)))
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
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Tuple<,>)))
                return true;
            return false;
        }

        // FIXME: This TupleConverter, using reflection, is likely to be many, many times slower than doing an emitted
        // class like DataContractDatumConverterFactory does.
        private class TupleConverter<T> : IDatumConverter<T>
        {
            private readonly Type item1Type;
            private readonly Type item2Type;
            private readonly object item1TypeDatumConverter;
            private readonly object item2TypeDatumConverter;
            private readonly ConstructorInfo tupleConstructor; 

            public TupleConverter(IDatumConverterFactory innerTypeConverterFactory)
            {
                var typeArguments = typeof(T).GetGenericArguments();
                item1Type = typeArguments[0];
                item2Type = typeArguments[1];

                tupleConstructor = typeof(T).GetConstructor(new Type[] { item1Type, item2Type });

                var genericGetMethod = innerTypeConverterFactory.GetType().GetMethod("Get");
                item1TypeDatumConverter = genericGetMethod.MakeGenericMethod(item1Type).Invoke(innerTypeConverterFactory, new object[0]);
                item2TypeDatumConverter = genericGetMethod.MakeGenericMethod(item2Type).Invoke(innerTypeConverterFactory, new object[0]);
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
                    object item1 = null;
                    object item2 = null;

                    foreach (var assocPair in datum.r_object)
                    {
                        if (assocPair.key == "left")
                            item1 = ReflectedConversion(assocPair.val, item1TypeDatumConverter);
                        else if (assocPair.key == "right")
                            item2 = ReflectedConversion(assocPair.val, item2TypeDatumConverter);
                        else
                            throw new InvalidOperationException("Unexpected key/value pair in tuple object: " + assocPair.key + "; expected only left and right");
                    }

                    return (T)(tupleConstructor.Invoke(new object[] { item1, item2 }));
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
