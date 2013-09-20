using System;
using RethinkDb.Spec;

namespace RethinkDb
{
    public abstract class AbstractValueTypeDatumConverter<T> : IDatumConverter<T>
    {
        protected AbstractValueTypeDatumConverter()
        {
            // Theoretically, `where T : struct` is what I'd like to do here, but that constraint is difficult to
            // satisfy in some places.
            if (!typeof(T).IsValueType)
                throw new InvalidOperationException("AbstractValueTypeDatumConverter should only be used on value types, not type " + typeof(T));
        }

        public abstract T ConvertDatum(Datum datum);
        public abstract Datum ConvertObject(T value);

        #region IDatumConverter implementation

        object IDatumConverter.ConvertDatum(Datum datum)
        {
            // A null datum is an error here in a the value-type datum converter; if you want to support
            // null values, use a NullableDatumConverterFactory<T>.
            if (datum.type == Datum.DatumType.R_NULL)
                throw new NotSupportedException("Attempted to cast Datum to non-nullable type " + typeof(T) + ", but Datum was null");

            return ConvertDatum(datum);
        }

        Datum IDatumConverter.ConvertObject(object @object)
        {
            // A null datum is an error here in a the value-type datum converter; if you want to support
            // null values, use a NullableDatumConverterFactory<T>.
            if (@object == null)
                throw new NotSupportedException("Attempted to cast object to non-nullable type " + typeof(T) + ", but object was null");

            return ConvertObject((T)@object);
        }

        #endregion
    }
}

