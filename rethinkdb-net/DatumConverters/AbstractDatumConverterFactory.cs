using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public abstract class AbstractDatumConverterFactory : IDatumConverterFactory
    {
        public abstract bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter);

        // This is really ugly, using a helper class and reflection to call the generic TryGet<T> method.  But,
        // I can't see any alternative due to the generic out parameter, and I'm making the assumptions that
        // (a) non-generic version of TryGet will be less frequently used than the generic version, and (b) the
        // generic version is easier to write, so the non-generic version should be the uglier one.
        public bool TryGet(Type datumType, IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter datumConverter)
        {
            var helperType = typeof(GenericHelper<>).MakeGenericType(datumType);
            var helperMethod = helperType.GetMethod("TryGet", BindingFlags.Public | BindingFlags.Static);
            var retval = (Tuple<bool, IDatumConverter>)helperMethod.Invoke(null, new object[] { this, rootDatumConverterFactory });
            datumConverter = retval.Item2;
            return retval.Item1;
        }

        private static class GenericHelper<T>
        {
            public static Tuple<bool, IDatumConverter> TryGet(IDatumConverterFactory target, IDatumConverterFactory rootDatumConverterFactory)
            {
                IDatumConverter<T> datumConverter;
                bool success = target.TryGet<T>(rootDatumConverterFactory, out datumConverter);
                return new Tuple<bool, IDatumConverter>(success, datumConverter);
            }
        }

        public Type GetBestNativeTypeForDatum(Spec.Datum datum)
        {
            // Attempt to auto-detect the best native type for a given Datum.

            switch (datum.type)
            {
                case Datum.DatumType.R_ARRAY:
                    {
                        var hasNullValues = datum.r_array.Any(d => d.type == Datum.DatumType.R_NULL);

                        var arrayValuesExcludingNulls = datum.r_array.Where(d => d.type != Datum.DatumType.R_NULL);

                        var nativeTypesExcludingNulls = arrayValuesExcludingNulls.Select(GetBestNativeTypeForDatum).Distinct().ToArray();

                        if (nativeTypesExcludingNulls.Length == 0)
                            // only have nulls, or, empty
                            return typeof(object[]);

                        if (nativeTypesExcludingNulls.Length == 2 && nativeTypesExcludingNulls.Contains(typeof(double)) && nativeTypesExcludingNulls.Contains(typeof(int)))
                            // we have numbers, both ints and doubles; we'll make an array of doubles as the return value.
                            // This is a special case; only works because we know GetBestNativeTypeForDatum will only return int or double.  If that changes,
                            // we need a more sophisticated manner to get the best numeric type here.
                            nativeTypesExcludingNulls = new [] { typeof(double) };

                        if (nativeTypesExcludingNulls.Length == 1)
                        {
                            Type arrayContentType = nativeTypesExcludingNulls[0];
                            if (!hasNullValues || !arrayContentType.IsValueType)
                                // we either have no nulls, or, this type can be assigned to null, so we just use the type
                                return arrayContentType.MakeArrayType();
                            else
                                // the type is Nullable<T>[], where T is the type of all the objects in the array
                                return typeof(Nullable<>).MakeGenericType(arrayContentType).MakeArrayType();
                        }

                        throw new RethinkDbRuntimeException("Heterogeneous arrays are not currently supported as their types are indistinguishable");
                    }
                
                case Datum.DatumType.R_BOOL:
                    return typeof(bool);
                
                case Datum.DatumType.R_NULL:
                    return typeof(object);
                
                case Datum.DatumType.R_NUM:
                    if (datum.r_num == Math.Floor(datum.r_num))
                        return typeof(int);
                    else
                        return typeof(double);
                
                case Datum.DatumType.R_OBJECT:
                    {
                        var reqlTypeDatum = datum.r_object.SingleOrDefault(kvp => kvp.key == "$reql_type$");
                        if (reqlTypeDatum != null && reqlTypeDatum.val.type == Datum.DatumType.R_STR)
                        {
                            var reqlType = reqlTypeDatum.val.r_str;
                            switch (reqlType)
                            {
                                case "BINARY":
                                    return typeof(byte[]);
                                case "TIME":
                                    return typeof(DateTimeOffset);
                                default:
                                    throw new RethinkDbInternalErrorException("Unrecognized reql_type");
                            }
                        }

                        return typeof(Dictionary<string, object>);
                    }
                
                case Datum.DatumType.R_STR:
                    return typeof(string);
                
                case Datum.DatumType.R_JSON:
                    throw new RethinkDbInternalErrorException("Unsupported datum type");

                default:
                    throw new RethinkDbInternalErrorException("Unrecognized datum type");
            }
        }
    }
}
