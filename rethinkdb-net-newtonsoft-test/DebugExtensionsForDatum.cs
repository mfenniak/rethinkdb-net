using System;
using System.Text;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test
{
    public static class DebugExtensionsForDatum
    {
        public static string ToDebugString(this Datum d)
        {
            var sb = new StringBuilder();
            d.PrintDatum(0, ref sb);
            return sb.ToString();
        }

        public static void ToConsoleDebug(this Datum d)
        {
            Console.WriteLine(d.ToDebugString());
        }

        public static void PrintDatum(this Datum d, int level, ref StringBuilder sb)
        {
            var padding = new string(' ', 4 * level);

            sb.AppendFormat(padding + "Datum_Type: {0}", d.type);
            sb.AppendLine();

            if (d.type == Datum.DatumType.R_OBJECT)
            {
                sb.AppendLine(padding + "Datum_R_OBJECT [AssocPair]:");
                foreach (var assoc in d.r_object)
                {
                    sb.AppendFormat(padding + ">Key: '{0}'", assoc.key);
                    sb.AppendLine();
                    sb.AppendFormat(padding + " Value [Datum]:");
                    sb.AppendLine();
                    PrintDatum(assoc.val, level + 1, ref sb);
                }
            }
            else if (d.type == Datum.DatumType.R_ARRAY)
            {
                sb.AppendLine(padding + "[[");
                for (var idx = 0; idx < d.r_array.Count; idx++)
                {
                    var datum = d.r_array[idx];
                    PrintDatum(datum, level + 1, ref sb);
                    if (idx != d.r_array.Count - 1)
                        sb.AppendLine(padding + ",,");
                }
                sb.AppendLine(padding + "]]");
            }
            else if (d.type == Datum.DatumType.R_NULL)
            {
            }
            else if (d.type == Datum.DatumType.R_BOOL)
            {
                sb.AppendFormat(padding + "Datum_R_BOOL: '{0}'", d.r_bool);
                sb.AppendLine();
            }
            else if (d.type == Datum.DatumType.R_STR)
            {
                sb.AppendFormat(padding + "Datum_R_STRING: '{0}'", d.r_str);
                sb.AppendLine();
            }
            else if (d.type == Datum.DatumType.R_NUM)
            {
                sb.AppendFormat(padding + "Datum_R_NUM: '{0}'", d.r_num);
                sb.AppendLine();
            }
        }
    }
}