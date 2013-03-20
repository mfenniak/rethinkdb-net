using System;
using System.Collections;

namespace RethinkDb
{
    public class ArrayDatumConverterFactory
    {
        public static readonly ArrayDatumConverterFactory Instance = new ArrayDatumConverterFactory();

        private ArrayDatumConverterFactory()
        {
        }

        public IDatumConverter<T> Get<T>(IDatumConverterFactory innerTypeConverterFactory)
        {
            if (!typeof(T).IsArray)
                throw new NotSupportedException(String.Format("Type {0} is not supported by PrimitiveDatumConverterFactory", typeof(T)));
            return new ReflectionArrayConverter<T>(innerTypeConverterFactory);
        }

        // FIXME: This ReflectionArrayConverter is likely to be many, many times slower than doing an emitted class
        // like DataContractDatumConverterFactory does.
        private class ReflectionArrayConverter<T> : IDatumConverter<T>
        {
            private readonly IDatumConverterFactory innerTypeConverterFactory;

            public ReflectionArrayConverter(IDatumConverterFactory innerTypeConverterFactory)
            {
                this.innerTypeConverterFactory = innerTypeConverterFactory;
            }

            #region IDatumConverter<T> Members

            public T ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                {
                    return default(T);
                }
                else if (datum.type == Spec.Datum.DatumType.R_ARRAY)
                {
                    var retval = Array.CreateInstance(typeof(T).GetElementType(), datum.r_array.Count);

                    var converter = innerTypeConverterFactory.GetType().GetMethod("Get").MakeGenericMethod(typeof(T).GetElementType()).Invoke(innerTypeConverterFactory, new object[0]);
                    var convertMethod = typeof(IDatumConverter<>).MakeGenericType(typeof(T).GetElementType()).GetMethod("ConvertDatum");

                    for (int i = 0; i < datum.r_array.Count; i++)
                    {
                        retval.SetValue(
                            convertMethod.Invoke(converter, new object[] { datum.r_array[i] }),
                            i
                        );
                    }

                    return (T)Convert.ChangeType(retval, typeof(T));
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to string, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(T arrayObject)
            {
                if (arrayObject == null)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };

                var retval = new Spec.Datum() { type = Spec.Datum.DatumType.R_ARRAY };
                var converter = innerTypeConverterFactory.GetType().GetMethod("Get").MakeGenericMethod(typeof(T).GetElementType()).Invoke(innerTypeConverterFactory, new object[0]);
                var convertMethod = typeof(IDatumConverter<>).MakeGenericType(typeof(T).GetElementType()).GetMethod("ConvertObject");
                var array = (IEnumerable)arrayObject;
                foreach (var obj in array)
                    retval.r_array.Add((Spec.Datum)convertMethod.Invoke(converter, new object[] { obj }));

                return retval;
            }

            #endregion
        }
    }
}
