using System;

namespace RethinkDb
{
    public class PrimitiveDatumConverterFactory : IDatumConverterFactory
    {
        public static readonly PrimitiveDatumConverterFactory Instance = new PrimitiveDatumConverterFactory();

        private PrimitiveDatumConverterFactory()
        {
        }

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
            else if (typeof (T) == typeof(int))
                return (IDatumConverter<T>)IntDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(int?))
                return (IDatumConverter<T>)NullableIntDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(uint))
                return (IDatumConverter<T>)UnsignedIntDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(uint?))
                return (IDatumConverter<T>)NullableUnsignedIntDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(long))
                return (IDatumConverter<T>)LongDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(long?))
                return (IDatumConverter<T>)NullableLongDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(ulong))
                return (IDatumConverter<T>)UnsignedLongDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(ulong?))
                return (IDatumConverter<T>)NullableUnsignedLongDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(short))
                return (IDatumConverter<T>)ShortDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(short?))
                return (IDatumConverter<T>)NullableShortDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(ushort))
                return (IDatumConverter<T>)UnsignedShortDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(ushort?))
                return (IDatumConverter<T>)NullableUnsignedShortDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(float))
                return (IDatumConverter<T>)FloatDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(float?))
                return (IDatumConverter<T>)NullableFloatDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(decimal))
                return (IDatumConverter<T>)DecimalDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(decimal?))
                return (IDatumConverter<T>)NullableDecimalDatumConverter.Instance.Value;
            else if (typeof(T).IsArray && IsTypeSupported(typeof(T).GetElementType()))
                return ArrayDatumConverterFactory.Instance.Get<T>(this);
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
            else if (t == typeof(int))
                return true;
            else if (t == typeof(int?))
                return true;
            else if (t == typeof(long))
                return true;
            else if (t == typeof(long?))
                return true;
            else if (t == typeof(short))
                return true;
            else if (t == typeof(short?))
                return true;
            else if (t.IsArray && IsTypeSupported(t.GetElementType()))
                return true;
            else
                return false;
        }

        public class StringDatumConverter : IDatumConverter<string>
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

        public class BoolDatumConverter : IDatumConverter<bool>
        {
            public static readonly Lazy<BoolDatumConverter> Instance = new Lazy<BoolDatumConverter>(() => new BoolDatumConverter());

            #region IDatumConverter<bool> Members

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

        public class NullableBoolDatumConverter : IDatumConverter<bool?>
        {
            public static readonly Lazy<NullableBoolDatumConverter> Instance = new Lazy<NullableBoolDatumConverter>(() => new NullableBoolDatumConverter());

            #region IDatumConverter<bool?> Members

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

        public class DoubleDatumConverter : IDatumConverter<double>
        {
            public static readonly Lazy<DoubleDatumConverter> Instance = new Lazy<DoubleDatumConverter>(() => new DoubleDatumConverter());

            #region IDatumConverter<double> Members

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

        public class NullableDoubleDatumConverter : IDatumConverter<double?>
        {
            public static readonly Lazy<NullableDoubleDatumConverter> Instance = new Lazy<NullableDoubleDatumConverter>(() => new NullableDoubleDatumConverter());

            #region IDatumConverter<double?> Members

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

        public class IntDatumConverter : IDatumConverter<int>
        {
            public static readonly Lazy<IntDatumConverter> Instance = new Lazy<IntDatumConverter>(() => new IntDatumConverter());

            #region IDatumConverter<int> Members

            public int ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable int, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > int.MaxValue || datum.r_num < int.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of int");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (int)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to int");
                    }
                }                    
                else
                    throw new NotSupportedException("Attempted to cast Datum to Int, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(int value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class NullableIntDatumConverter : IDatumConverter<int?>
        {
            public static readonly Lazy<NullableIntDatumConverter> Instance = new Lazy<NullableIntDatumConverter>(() => new NullableIntDatumConverter());

            #region IDatumConverter<int?> Members

            public int? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > int.MaxValue || datum.r_num < int.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of int");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (int?)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to int");
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to Int, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(int? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value.Value };
            }

            #endregion
        }

        public class UnsignedIntDatumConverter : IDatumConverter<uint>
        {
            public static readonly Lazy<UnsignedIntDatumConverter> Instance = new Lazy<UnsignedIntDatumConverter>(() => new UnsignedIntDatumConverter());

            #region IDatumConverter<uint> Members

            public uint ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable unsigned int, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > uint.MaxValue || datum.r_num < uint.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of unsigned int");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (uint)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to unsigned int");
                    }
                }                    
                else
                    throw new NotSupportedException("Attempted to cast Datum to unsigned Int, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(uint value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class NullableUnsignedIntDatumConverter : IDatumConverter<uint?>
        {
            public static readonly Lazy<NullableUnsignedIntDatumConverter> Instance = new Lazy<NullableUnsignedIntDatumConverter>(() => new NullableUnsignedIntDatumConverter());

            #region IDatumConverter<uint?> Members

            public uint? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > uint.MaxValue || datum.r_num < uint.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of unsigned int");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (uint?)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to unsigned int");
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to unsigned Int, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(uint? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value.Value };
            }

            #endregion
        }

        public class LongDatumConverter : IDatumConverter<long>
        {
            public static readonly Lazy<LongDatumConverter> Instance = new Lazy<LongDatumConverter>(() => new LongDatumConverter());

            #region IDatumConverter<long> Members

            public long ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable long, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    var valueAsLong = (long)datum.r_num;

                    if (valueAsLong >= long.MaxValue || valueAsLong <= long.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum to non-nullable long, but Datum outside range of valid long");
                    }
                    if (datum.r_num % 1 == 0)
                    {
                        return (long)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to non-nullable long");
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to Long, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(long value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class NullableLongDatumConverter : IDatumConverter<long?>
        {
            public static readonly Lazy<NullableLongDatumConverter> Instance = new Lazy<NullableLongDatumConverter>(() => new NullableLongDatumConverter());

            #region IDatumConverter<long?> Members

            public long? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    var valueAsLong = (long)datum.r_num;

                    if (valueAsLong >= long.MaxValue || valueAsLong <= long.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast long with a value outside the range of a double to Datum");
                    }
                    if (datum.r_num % 1 == 0)
                    {
                        return (long?)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to long");
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to Long, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(long? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value.Value };
            }

            #endregion
        }

        public class UnsignedLongDatumConverter : IDatumConverter<ulong>
        {
            public static readonly Lazy<UnsignedLongDatumConverter> Instance = new Lazy<UnsignedLongDatumConverter>(() => new UnsignedLongDatumConverter());

            #region IDatumConverter<ulong> Members

            public ulong ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable unsigned long, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    var valueAsLong = (ulong)datum.r_num;

                    if (valueAsLong >= ulong.MaxValue || valueAsLong <= ulong.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum to non-nullable unsigned long, but Datum outside range of valid unsigned long");
                    }
                    if (datum.r_num % 1 == 0)
                    {
                        return (ulong)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to non-nullable unsigned long");
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to unsigned Long, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(ulong value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class NullableUnsignedLongDatumConverter : IDatumConverter<ulong?>
        {
            public static readonly Lazy<NullableUnsignedLongDatumConverter> Instance = new Lazy<NullableUnsignedLongDatumConverter>(() => new NullableUnsignedLongDatumConverter());

            #region IDatumConverter<ulong?> Members

            public ulong? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    var valueAsLong = (ulong)datum.r_num;

                    if (valueAsLong >= ulong.MaxValue || valueAsLong <= ulong.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast unsigned long with a value outside the range of a double to Datum");
                    }
                    if (datum.r_num % 1 == 0)
                    {
                        return (ulong?)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to unsigned long");
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to unsigned Long, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(ulong? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value.Value };
            }

            #endregion
        }

        public class ShortDatumConverter : IDatumConverter<short>
        {
            public static readonly Lazy<ShortDatumConverter> Instance = new Lazy<ShortDatumConverter>(() => new ShortDatumConverter());

            #region IDatumConverter<short> Members

            public short ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable short, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > short.MaxValue || datum.r_num < short.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of short");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (short)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to short");
                    }
                }                    
                else
                    throw new NotSupportedException("Attempted to cast Datum to Short, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(short value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class NullableShortDatumConverter : IDatumConverter<short?>
        {
            public static readonly Lazy<NullableShortDatumConverter> Instance = new Lazy<NullableShortDatumConverter>(() => new NullableShortDatumConverter());

            #region IDatumConverter<short?> Members

            public short? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > short.MaxValue || datum.r_num < short.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of short");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (short?)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to short");
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to Short, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(short? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value.Value };
            }

            #endregion
        }

        public class UnsignedShortDatumConverter : IDatumConverter<ushort>
        {
            public static readonly Lazy<UnsignedShortDatumConverter> Instance = new Lazy<UnsignedShortDatumConverter>(() => new UnsignedShortDatumConverter());

            #region IDatumConverter<ushort> Members

            public ushort ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable unsigned short, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > ushort.MaxValue || datum.r_num < ushort.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of unsigned short");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (ushort)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to unsigned short");
                    }
                }                    
                else
                    throw new NotSupportedException("Attempted to cast Datum to unsigned Short, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(ushort value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class NullableUnsignedShortDatumConverter : IDatumConverter<ushort?>
        {
            public static readonly Lazy<NullableUnsignedShortDatumConverter> Instance = new Lazy<NullableUnsignedShortDatumConverter>(() => new NullableUnsignedShortDatumConverter());

            #region IDatumConverter<ushort?> Members

            public ushort? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > ushort.MaxValue || datum.r_num < ushort.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of unsigned short");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (ushort?)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to unsigned short");
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to unsigned Short, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(ushort? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value.Value };
            }

            #endregion
        }

        public class FloatDatumConverter : IDatumConverter<float>
        {
            public static readonly Lazy<FloatDatumConverter> Instance = new Lazy<FloatDatumConverter>(() => new FloatDatumConverter());

            #region IDatumConverter<float> Members

            public float ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable float, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    var valueAsFloat = (float)datum.r_num;

                    if (valueAsFloat >= float.MaxValue || valueAsFloat <= float.MinValue)                    
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of float");
                    }
                    else
                    {
                        return (float)datum.r_num;
                    }
                }                    
                else
                    throw new NotSupportedException("Attempted to cast Datum to Float, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(float value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class NullableFloatDatumConverter : IDatumConverter<float?>
        {
            public static readonly Lazy<NullableFloatDatumConverter> Instance = new Lazy<NullableFloatDatumConverter>(() => new NullableFloatDatumConverter());

            #region IDatumConverter<float?> Members

            public float? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    var valueAsFloat = (float)datum.r_num;

                    if (valueAsFloat >= float.MaxValue || valueAsFloat <= float.MinValue)  
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of float");
                    }
                    else
                    {
                        return (float?)datum.r_num;
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to Float, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(float? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value.Value };
            }

            #endregion
        }

        public class DecimalDatumConverter : IDatumConverter<decimal>
        {
            public static readonly Lazy<DecimalDatumConverter> Instance = new Lazy<DecimalDatumConverter>(() => new DecimalDatumConverter());

            #region IDatumConverter<decimal> Members

            public decimal ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable decimal, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num >= Convert.ToDouble(decimal.MaxValue) || datum.r_num <= Convert.ToDouble(decimal.MinValue))
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of decimal");
                    }
                    else
                    {
                        return Convert.ToDecimal(datum.r_num);
                    }
                }                    
                else
                    throw new NotSupportedException("Attempted to cast Datum to Decimal, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(decimal value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = Convert.ToDouble(value) };
            }

            #endregion
        }

        public class NullableDecimalDatumConverter : IDatumConverter<decimal?>
        {
            public static readonly Lazy<NullableDecimalDatumConverter> Instance = new Lazy<NullableDecimalDatumConverter>(() => new NullableDecimalDatumConverter());

            #region IDatumConverter<decimal?> Members

            public decimal? ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num >= (double)decimal.MaxValue || datum.r_num <= (double)decimal.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of decimal");
                    }
                    else
                    {
                        return Convert.ToDecimal(datum.r_num);
                    }
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to Decimal, but Datum was unsupported type " + datum.type);
            }

            public Spec.Datum ConvertObject(decimal? value)
            {
                if (!value.HasValue)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = Convert.ToDouble(value.Value) };
            }

            #endregion
        }
    }
}
