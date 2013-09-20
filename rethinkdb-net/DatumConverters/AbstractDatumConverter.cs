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
            // Because <T> may be a value-type, we need to handle null-conversion here
            if (datum.type == Datum.DatumType.R_NULL)
                return null;
            else
                return ConvertDatum(datum);
        }

        Datum IDatumConverter.ConvertObject(object @object)
        {
            // Because <T> may be a value-type, we need to handle null-conversion here
            if (@object == null)
                return new Datum() { type = Datum.DatumType.R_NULL };
            else
                return ConvertObject((T)@object);
        }

        #endregion
    }
}

