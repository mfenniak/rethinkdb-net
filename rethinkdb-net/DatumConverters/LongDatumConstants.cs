using System;

namespace RethinkDb
{
    public static class LongDatumConstants
    {
        public static long MaxValue = (long)Math.Pow(2, 53);
        public static long MinValue = (long)Math.Pow(-2, 53);
    }
}

