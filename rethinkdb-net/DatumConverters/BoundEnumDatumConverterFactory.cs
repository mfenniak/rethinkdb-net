using System;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public class BoundEnumDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly BoundEnumDatumConverterFactory Instance = new BoundEnumDatumConverterFactory();

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;
            if (typeof(T) == typeof(Bound))
                datumConverter = (IDatumConverter<T>)BoundEnumDatumConverter.Instance;
            return datumConverter != null;
        }
    }

    public class BoundEnumDatumConverter : AbstractValueTypeDatumConverter<Bound>
    {
        public static readonly BoundEnumDatumConverter Instance = new BoundEnumDatumConverter();

        #region IDatumConverter<Enum> Members

        public override Bound ConvertDatum(Spec.Datum datum)
        {
            switch (datum.r_str)
            {
                case "open":
                    return Bound.Open;
                case "closed":
                    return Bound.Closed;
                default:
                    throw new NotSupportedException("Bound value must be open or closed");
            }
        }

        public override Spec.Datum ConvertObject(Bound enumValue)
        {
            var retval = new Datum() { type = Datum.DatumType.R_STR };
            switch (enumValue)
            {
                case Bound.Open:
                    retval.r_str = "open";
                    break;
                case Bound.Closed:
                    retval.r_str = "closed";
                    break;
                default:
                    throw new NotSupportedException("Bound value must be open or closed");
            }
            return retval;
        }

        #endregion
    }
}
