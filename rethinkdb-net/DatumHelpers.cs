using System;
using RethinkDb.Spec;
using System.Collections.Generic;
using System.Linq;

namespace RethinkDb
{
    public static class DatumHelpers
    {
        public static Datum ToDatum(this string str)
        {
            return new Datum
            {
                type = Datum.DatumType.R_STR,
                r_str = str
            };
        }

        public static Datum ToDatum(this bool bl)
        {
            return new Datum
            {
                type = Datum.DatumType.R_BOOL,
                r_bool = bl
            };
        }

        public static Datum ToDatum(this double num)
        {
            return new Datum
            {
                type = Datum.DatumType.R_NUM,
                r_num = num
            };
        }

        public static Datum ToDatum(this long num)
        {
            return ((double)num).ToDatum();
        }

        public static Datum ToDatum(this int num)
        {
            return ((double)num).ToDatum();
        }

        public static Datum ToDatum(this Dictionary<string, string> kvPairs)
        {
            var dtm = new Datum { type = Datum.DatumType.R_OBJECT };
            dtm.r_object.AddRange(kvPairs.Select(DatumHelpers.ToDatum));
            return dtm;
        }

        public static Datum.AssocPair ToDatum(this KeyValuePair<string, string> kvPair)
        {
            return new Datum.AssocPair() { key = kvPair.Key, val = kvPair.Value.ToDatum() };
        }
    }
}

