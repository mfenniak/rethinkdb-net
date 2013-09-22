using System;
using RethinkDb.Spec;

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
    }
}

