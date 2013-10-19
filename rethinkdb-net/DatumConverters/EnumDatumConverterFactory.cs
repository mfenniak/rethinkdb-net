using System;
using System.Linq;

namespace RethinkDb.DatumConverters
{
    public class EnumDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly EnumDatumConverterFactory Instance = new EnumDatumConverterFactory();

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;
            if (typeof(T).IsEnum)
                datumConverter = EnumDatumConverter<T>.Instance.Value;
            return datumConverter != null;
        }
    }

    public class EnumDatumConverter<T> : AbstractValueTypeDatumConverter<T>
    {
        public static readonly Lazy<EnumDatumConverter<T>> Instance = new Lazy<EnumDatumConverter<T>>(() => new EnumDatumConverter<T>());

        #region IDatumConverter<Enum> Members

        public override T ConvertDatum(Spec.Datum datum)
        {
            var value = PrimitiveDatumConverterFactory.UnsignedLongDatumConverter.Instance.Value.ConvertDatum(datum);
            return (T)Enum.ToObject(typeof(T), value);
        }

        public override Spec.Datum ConvertObject(T enumValue)
        {
            var value = Convert.ToUInt64(enumValue);
            return PrimitiveDatumConverterFactory.UnsignedLongDatumConverter.Instance.Value.ConvertObject(value);
        }

        #endregion
    }
}