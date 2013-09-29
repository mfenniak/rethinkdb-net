using System;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public abstract class AbstractReferenceTypeDatumConverter<T> : IDatumConverter<T>
    {
        protected AbstractReferenceTypeDatumConverter()
        {
            // Theoretically, `where T : class` is what I'd like to do here, but that constraint is difficult to
            // satisfy in some places.
            if (typeof(T).IsValueType)
                throw new InvalidOperationException("AbstractValueTypeDatumConverter should only be used on value types, not type " + typeof(T));
        }

        public abstract T ConvertDatum(Datum datum);
        public abstract Datum ConvertObject(T value);

        #region IDatumConverter implementation

        object IDatumConverter.ConvertDatum(Datum datum)
        {
            return ConvertDatum(datum);
        }

        Datum IDatumConverter.ConvertObject(object @object)
        {
            return ConvertObject((T)@object);
        }

        #endregion
    }
}

