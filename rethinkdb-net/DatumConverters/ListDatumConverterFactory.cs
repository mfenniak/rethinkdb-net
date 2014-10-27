using System;
using System.Collections.Generic;
using System.Linq;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public class ListDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly ListDatumConverterFactory Instance = new ListDatumConverterFactory();

        private ListDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            if (typeof(T).IsGenericType &&
                typeof(T).GetGenericTypeDefinition().Equals(typeof(List<>)))
            {
                Type listDatumConverterType = typeof(ListDatumConverter<>).MakeGenericType(
                    typeof(T).GetGenericArguments()[0]
                );
                datumConverter = (IDatumConverter<T>)Activator.CreateInstance(listDatumConverterType, rootDatumConverterFactory);
                return true;
            }

            if (typeof(T).IsGenericType &&
                typeof(T).GetGenericTypeDefinition().Equals(typeof(IList<>)))
            {
                Type listDatumConverterType = typeof(IListDatumConverter<>).MakeGenericType(
                    typeof(T).GetGenericArguments()[0]
                );
                datumConverter = (IDatumConverter<T>)Activator.CreateInstance(listDatumConverterType, rootDatumConverterFactory);
                return true;
            }

            datumConverter = null;
            return false;
        }

        private class ListDatumConverter<T> : AbstractReferenceTypeDatumConverter<List<T>>
        {
            private IDatumConverter<T[]> arrayDatumConverter;

            public ListDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
            {
                this.arrayDatumConverter = new ArrayDatumConverter<T[]>(rootDatumConverterFactory);
            }

            public override List<T> ConvertDatum(Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                return new List<T>(arrayDatumConverter.ConvertDatum(datum));
            }

            public override Datum ConvertObject(List<T> value)
            {
                if (value == null)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                return arrayDatumConverter.ConvertObject(value.ToArray());
            }
        }

        private class IListDatumConverter<T> : AbstractReferenceTypeDatumConverter<IList<T>>
        {
            private IDatumConverter<T[]> arrayDatumConverter;

            public IListDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
            {
                this.arrayDatumConverter = new ArrayDatumConverter<T[]>(rootDatumConverterFactory);
            }

            public override IList<T> ConvertDatum(Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                return new List<T>(arrayDatumConverter.ConvertDatum(datum));
            }

            public override Datum ConvertObject(IList<T> value)
            {
                if (value == null)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                var list = value as List<T>;
                if (list != null)
                    // Use native List<T>.ToArray if possible, as it is a direct in-memory copy and fast
                    return arrayDatumConverter.ConvertObject(list.ToArray());
                else
                    // If value isn't a List<T>, then use Linq's IEnumerable<T>.ToArray extension method; not as fast
                    // as List<T>'s as it copies the values one by one.
                    return arrayDatumConverter.ConvertObject(value.ToArray());
            }
        }
    }
}

