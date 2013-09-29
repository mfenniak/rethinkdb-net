using System;

namespace RethinkDb.DatumConverters
{
    public class PrimitiveDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly PrimitiveDatumConverterFactory Instance = new PrimitiveDatumConverterFactory();

        private PrimitiveDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (typeof(T) == typeof(string))
                datumConverter = (IDatumConverter<T>)StringDatumConverter.Instance.Value;
            else if (typeof(T) == typeof(char))
                datumConverter = (IDatumConverter<T>)CharDatumConverter.Instance.Value;
            else if (typeof(T) == typeof(bool))
                datumConverter = (IDatumConverter<T>)BoolDatumConverter.Instance.Value;
            else if (typeof(T) == typeof(double))
                datumConverter = (IDatumConverter<T>)DoubleDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(int))
                datumConverter = (IDatumConverter<T>)IntDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(uint))
                datumConverter = (IDatumConverter<T>)UnsignedIntDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(long))
                datumConverter = (IDatumConverter<T>)LongDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(ulong))
                datumConverter = (IDatumConverter<T>)UnsignedLongDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(short))
                datumConverter = (IDatumConverter<T>)ShortDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(ushort))
                datumConverter = (IDatumConverter<T>)UnsignedShortDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(float))
                datumConverter = (IDatumConverter<T>)FloatDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(decimal))
                datumConverter = (IDatumConverter<T>)DecimalDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(byte))
                datumConverter = (IDatumConverter<T>)ByteDatumConverter.Instance.Value;
            else if (typeof (T) == typeof(sbyte))
                datumConverter = (IDatumConverter<T>)SignedByteDatumConverter.Instance.Value;

            return datumConverter != null;
        }

        public class StringDatumConverter : AbstractReferenceTypeDatumConverter<string>
        {
            public static readonly Lazy<StringDatumConverter> Instance = new Lazy<StringDatumConverter>(() => new StringDatumConverter());

            #region IDatumConverter<string> Members

            public override string ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    return null;
                else if (datum.type == Spec.Datum.DatumType.R_STR)
                    return datum.r_str;
                else
                    throw new NotSupportedException("Attempted to cast Datum to string, but Datum was unsupported type " + datum.type);
            }

            public override Spec.Datum ConvertObject(string str)
            {
                if (str == null)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
                else
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_STR, r_str = str };
            }

            #endregion
        }

        public class CharDatumConverter : AbstractValueTypeDatumConverter<char>
        {
            public static readonly Lazy<CharDatumConverter> Instance = new Lazy<CharDatumConverter>(() => new CharDatumConverter());

            #region IDatumConverter<char> Members

            public override char ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable char, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_STR)
                {
                    if (datum.r_str.Length != 1)
                    {
                        throw new NotSupportedException("Attempted to cast Datum to char, but Datum was not a single character");
                    }

                    return datum.r_str[0];
                }                 
                else
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable char, but Datum was unsupported type " + datum.type);
            }

            public override Spec.Datum ConvertObject(char str)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_STR, r_str = str.ToString() };
            }

            #endregion
        }

        public class BoolDatumConverter : AbstractValueTypeDatumConverter<bool>
        {
            public static readonly Lazy<BoolDatumConverter> Instance = new Lazy<BoolDatumConverter>(() => new BoolDatumConverter());

            #region IDatumConverter<bool> Members

            public override bool ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable bool, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_BOOL)
                    return datum.r_bool;
                else
                    throw new NotSupportedException("Attempted to cast Datum to bool, but Datum was unsupported type " + datum.type);
            }

            public override Spec.Datum ConvertObject(bool value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_BOOL, r_bool = value };
            }

            #endregion
        }

        public class DoubleDatumConverter : AbstractValueTypeDatumConverter<double>
        {
            public static readonly Lazy<DoubleDatumConverter> Instance = new Lazy<DoubleDatumConverter>(() => new DoubleDatumConverter());

            #region IDatumConverter<double> Members

            public override double ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable double, but Datum was null");
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                    return datum.r_num;
                else
                    throw new NotSupportedException("Attempted to cast Datum to Double, but Datum was unsupported type " + datum.type);
            }

            public override Spec.Datum ConvertObject(double value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class IntDatumConverter : AbstractValueTypeDatumConverter<int>
        {
            public static readonly Lazy<IntDatumConverter> Instance = new Lazy<IntDatumConverter>(() => new IntDatumConverter());

            #region IDatumConverter<int> Members

            public override int ConvertDatum(Spec.Datum datum)
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

            public override Spec.Datum ConvertObject(int value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class UnsignedIntDatumConverter : AbstractValueTypeDatumConverter<uint>
        {
            public static readonly Lazy<UnsignedIntDatumConverter> Instance = new Lazy<UnsignedIntDatumConverter>(() => new UnsignedIntDatumConverter());

            #region IDatumConverter<uint> Members

            public override uint ConvertDatum(Spec.Datum datum)
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

            public override Spec.Datum ConvertObject(uint value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class LongDatumConverter : AbstractValueTypeDatumConverter<long>
        {
            public static readonly Lazy<LongDatumConverter> Instance = new Lazy<LongDatumConverter>(() => new LongDatumConverter());

            #region IDatumConverter<long> Members

            public override long ConvertDatum(Spec.Datum datum)
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

            public override Spec.Datum ConvertObject(long value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class UnsignedLongDatumConverter : AbstractValueTypeDatumConverter<ulong>
        {
            public static readonly Lazy<UnsignedLongDatumConverter> Instance = new Lazy<UnsignedLongDatumConverter>(() => new UnsignedLongDatumConverter());

            #region IDatumConverter<ulong> Members

            public override ulong ConvertDatum(Spec.Datum datum)
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

            public override Spec.Datum ConvertObject(ulong value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class ShortDatumConverter : AbstractValueTypeDatumConverter<short>
        {
            public static readonly Lazy<ShortDatumConverter> Instance = new Lazy<ShortDatumConverter>(() => new ShortDatumConverter());

            #region IDatumConverter<short> Members

            public override short ConvertDatum(Spec.Datum datum)
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

            public override Spec.Datum ConvertObject(short value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class UnsignedShortDatumConverter : AbstractValueTypeDatumConverter<ushort>
        {
            public static readonly Lazy<UnsignedShortDatumConverter> Instance = new Lazy<UnsignedShortDatumConverter>(() => new UnsignedShortDatumConverter());

            #region IDatumConverter<ushort> Members

            public override ushort ConvertDatum(Spec.Datum datum)
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

            public override Spec.Datum ConvertObject(ushort value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class FloatDatumConverter : AbstractValueTypeDatumConverter<float>
        {
            public static readonly Lazy<FloatDatumConverter> Instance = new Lazy<FloatDatumConverter>(() => new FloatDatumConverter());

            #region IDatumConverter<float> Members

            public override float ConvertDatum(Spec.Datum datum)
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

            public override Spec.Datum ConvertObject(float value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class DecimalDatumConverter : AbstractValueTypeDatumConverter<decimal>
        {
            public static readonly Lazy<DecimalDatumConverter> Instance = new Lazy<DecimalDatumConverter>(() => new DecimalDatumConverter());

            #region IDatumConverter<decimal> Members

            public override decimal ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                {
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable decimal, but Datum was null");
                }
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

            public override Spec.Datum ConvertObject(decimal value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = Convert.ToDouble(value) };
            }

            #endregion
        }

        public class ByteDatumConverter : AbstractValueTypeDatumConverter<byte>
        {
            public static readonly Lazy<ByteDatumConverter> Instance = new Lazy<ByteDatumConverter>(() => new ByteDatumConverter());

            #region IDatumConverter<byte> Members

            public override byte ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                {
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable byte, but Datum was null");
                }
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > byte.MaxValue || datum.r_num < byte.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of byte");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (byte)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to byte");
                    }
                }                    
                else
                    throw new NotSupportedException("Attempted to cast Datum to Byte, but Datum was unsupported type " + datum.type);
            }

            public override Spec.Datum ConvertObject(byte value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }

        public class SignedByteDatumConverter : AbstractValueTypeDatumConverter<sbyte>
        {
            public static readonly Lazy<SignedByteDatumConverter> Instance = new Lazy<SignedByteDatumConverter>(() => new SignedByteDatumConverter());

            #region IDatumConverter<sbyte> Members

            public override sbyte ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                {
                    throw new NotSupportedException("Attempted to cast Datum to non-nullable signed byte, but Datum was null");
                }
                else if (datum.type == Spec.Datum.DatumType.R_NUM)
                {
                    if (datum.r_num > sbyte.MaxValue || datum.r_num < sbyte.MinValue)
                    {
                        throw new NotSupportedException("Attempted to cast Datum outside range of signed byte");
                    }

                    if (datum.r_num % 1 == 0)
                    {
                        return (sbyte)datum.r_num;
                    }
                    else
                    {
                        throw new NotSupportedException("Attempted to cast fractional Datum to signed byte");
                    }
                }                    
                else
                    throw new NotSupportedException("Attempted to cast Datum to signed Byte, but Datum was unsupported type " + datum.type);
            }

            public override Spec.Datum ConvertObject(sbyte value)
            {
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NUM, r_num = value };
            }

            #endregion
        }
    }
}
