using System;
using System.Collections;
using System.Reflection;

namespace RethinkDb
{
    public class ArrayDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly ArrayDatumConverterFactory Instance = new ArrayDatumConverterFactory();

        private ArrayDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;
            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");

            if (!typeof(T).IsArray)
                return false;

            datumConverter = new ArrayConverter<T>(rootDatumConverterFactory);
            return true;
        }

        private class ArrayConverter<T> : AbstractReferenceTypeDatumConverter<T>
        {
            private readonly IDatumConverter arrayTypeConverter;

            public ArrayConverter(IDatumConverterFactory rootDatumConverterFactory)
            {
                this.arrayTypeConverter = rootDatumConverterFactory.Get(typeof(T).GetElementType());
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
                    for (int i = 0; i < datum.r_array.Count; i++)
                        retval.SetValue(arrayTypeConverter.ConvertDatum(datum.r_array [i]), i);
                    return (T)Convert.ChangeType(retval, typeof(T));
                }
                else
                {
                    throw new NotSupportedException("Attempted to cast Datum to array, but Datum was unsupported type " + datum.type);
                }
            }

            public override Spec.Datum ConvertObject(T arrayObject)
            {
                if (arrayObject == null)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };

                var retval = new Spec.Datum() { type = Spec.Datum.DatumType.R_ARRAY };
                var array = (IEnumerable)arrayObject;
                foreach (var obj in array)
                    retval.r_array.Add(arrayTypeConverter.ConvertObject(obj));
                return retval;
            }

            #endregion
        }
    }
}
