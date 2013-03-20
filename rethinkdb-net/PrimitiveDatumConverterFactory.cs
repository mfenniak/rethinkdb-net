using System;

namespace RethinkDb
{
    public class PrimitiveDatumConverterFactory : IDatumConverterFactory
    {
        public IDatumConverter<T> Get<T>()
        {
            if (typeof(T) == typeof(string))
                return (IDatumConverter<T>)StringDatumConverter.Instance.Value;
            else if (typeof(T) == typeof(bool))
                return (IDatumConverter<T>)BoolDatumConverter.Instance.Value;
            else if (typeof(T) == typeof(bool?))
                return (IDatumConverter<T>)NullableBoolDatumConverter.Instance.Value;
            else if (typeof(T) == typeof(double))
                return (IDatumConverter<T>)DoubleDatumConverter.Instance.Value;
            else if (typeof(T) == typeof(double?))
                return (IDatumConverter<T>)NullableDoubleDatumConverter.Instance.Value;
            else
                throw new NotSupportedException(String.Format("Type {0} is not supported by PrimitiveDatumConverterFactory", typeof(T)));
        }

        public bool IsTypeSupported(Type t)
        {
            if (t == typeof(string))
                return true;
            else if (t == typeof(bool))
                return true;
            else if (t == typeof(bool?))
                return true;
            else if (t == typeof(double))
                return true;
            else if (t == typeof(double?))
                return true;
            else
                return false;
        }

        private class StringDatumConverter : IDatumConverter<string>
        {
            public static readonly Lazy<StringDatumConverter> Instance = new Lazy<StringDatumConverter>(() => new StringDatumConverter());

            #region IDatumConverter<string> Members

            public string ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_STR)
                    return datum.r_str;
                else
                    throw new NotSupportedException("Attempted to cast Datum to string, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(string str)
            {
                if (str == null)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_STR, r_str = str };
            }

            #endregion
        }

        private class BoolDatumConverter : IDatumConverter<bool>
        {
            public static readonly Lazy<BoolDatumConverter> Instance = new Lazy<BoolDatumConverter>(() => new BoolDatumConverter());

            #region IDatumConverter<string> Members

            public bool ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable bool, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_BOOL)
                    return datum.r_bool;
                else
                    throw new NotSupportedException("Attempted to cast Datum to bool, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(bool value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_BOOL, r_bool = value };
            }

            #endregion
        }

        private class NullableBoolDatumConverter : IDatumConverter<bool?>
        {
            public static readonly Lazy<NullableBoolDatumConverter> Instance = new Lazy<NullableBoolDatumConverter>(() => new NullableBoolDatumConverter());

            #region IDatumConverter<string> Members

            public bool? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_BOOL)
                    return datum.r_bool;
                else
                    throw new NotSupportedException("Attempted to cast Datum to bool, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(bool? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_BOOL, r_bool = value.Value };
            }

            #endregion
        }

        private class DoubleDatumConverter : IDatumConverter<double>
        {
            public static readonly Lazy<DoubleDatumConverter> Instance = new Lazy<DoubleDatumConverter>(() => new DoubleDatumConverter());

            #region IDatumConverter<string> Members

            public double ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable double, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                    return datum.r_num;
                else
                    throw new NotSupportedException("Attempted to cast Datum to Double, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(double value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        private class NullableDoubleDatumConverter : IDatumConverter<double?>
        {
            public static readonly Lazy<NullableDoubleDatumConverter> Instance = new Lazy<NullableDoubleDatumConverter>(() => new NullableDoubleDatumConverter());

            #region IDatumConverter<string> Members

            public double? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                    return datum.r_num;
                else
                    throw new NotSupportedException("Attempted to cast Datum to Double, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(double? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value.Value };
            }

            #endregion
        }
    }
}
