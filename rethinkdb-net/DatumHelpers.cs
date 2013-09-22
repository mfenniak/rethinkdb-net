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
    }
}

