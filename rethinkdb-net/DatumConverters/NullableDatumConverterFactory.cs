using System;
using RethinkDb.Spec;

namespace RethinkDb
{
    public class NullableDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly NullableDatumConverterFactory Instance = new NullableDatumConverterFactory();

        private NullableDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;
            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type converterType = typeof(NullableDatumConverter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
                datumConverter = (IDatumConverter<T>)Activator.CreateInstance(converterType, rootDatumConverterFactory);
                return true;
            }
            else
                return false;

        }

        private class NullableDatumConverter<T> : IDatumConverter<Nullable<T>>
            where T : struct
        {
            private readonly IDatumConverter<T> innerConverter;

            public NullableDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
            {
                this.innerConverter = rootDatumConverterFactory.Get<T>(rootDatumConverterFactory);
            }

            #region IDatumConverter<T> Members

            public Nullable<T> ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return new Nullable<T>();
                else
                    return new Nullable<T>(innerConverter.ConvertDatum(datum));
            }

            public Spec.Datum ConvertObject(Nullable<T> nullableObject)
            {
                if (nullableObject.HasValue)
                    return innerConverter.ConvertObject(nullableObject.Value);
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
            }

            #endregion
            #region IDatumConverter members

            object IDatumConverter.ConvertDatum(Datum datum)
            {
                return ConvertDatum(datum);
            }

            Datum IDatumConverter.ConvertObject(object @object)
            {
                return ConvertObject((Nullable<T>)@object);
            }

            #endregion
        }
    }
}
