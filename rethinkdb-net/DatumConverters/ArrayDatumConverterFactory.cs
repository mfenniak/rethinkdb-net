using System;
using System.Collections;
using System.Reflection;

namespace RethinkDb
{
    public class ArrayDatumConverterFactory : IDatumConverterFactory
    {
        public static readonly ArrayDatumConverterFactory Instance = new ArrayDatumConverterFactory();

        private ArrayDatumConverterFactory()
        {
        }

        public bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;
            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");

            if (!typeof(T).IsArray)
                return false;

            datumConverter = new ReflectionArrayConverter<T>(rootDatumConverterFactory);
            return true;
        }

        // FIXME: This ReflectionArrayConverter is likely to be many, many times slower than doing an emitted class
        // like DataContractDatumConverterFactory does.
        private class ReflectionArrayConverter<T> : AbstractDatumConverter<T>
        {
            private readonly IDatumConverterFactory rootDatumConverterFactory;

            public ReflectionArrayConverter(IDatumConverterFactory rootDatumConverterFactory)
            {
                this.rootDatumConverterFactory = rootDatumConverterFactory;
            }

            #region IDatumConverter<T> Members

            public override T ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                {
                    return default(T);
                }
                else if (datum.type == Spec.Datum.DatumType.R_ARRAY)
                {
                    var retval = Array.CreateInstance(typeof(T).GetElementType(), datum.r_array.Count);

                    var converter = typeof(DatumConverterFactoryExtensions)
                        .GetMethod("Get", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IDatumConverterFactory) }, null)
                        .MakeGenericMethod(typeof(T).GetElementType()).Invoke(null, new object[] { rootDatumConverterFactory });
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
                    throw new NotSupportedException("Attempted to cast Datum to array, but Datum was unsupported type " + datum.type);
            }

            public override Spec.Datum ConvertObject(T arrayObject)
            {
                if (arrayObject == null)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };

                var retval = new Spec.Datum() { type = Spec.Datum.DatumType.R_ARRAY };

                var converter = typeof(DatumConverterFactoryExtensions)
                    .GetMethod("Get", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IDatumConverterFactory) }, null)
                    .MakeGenericMethod(typeof(T).GetElementType()).Invoke(null, new object[] { rootDatumConverterFactory });
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
