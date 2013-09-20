using System;
using RethinkDb.Spec;

namespace RethinkDb
{
    public abstract class AbstractDatumConverter<T> : IDatumConverter<T>
    {
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

