using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RethinkDb
{
    public class CompoundIndexKeys
    {
        private CompoundIndexKeys() { }

        internal object[] Values { get; set; }

        public static CompoundIndexKeys Make<T1, T2>(T1 value1, T2 value2)
        {
            return new CompoundIndexKeys { Values = new object[] { value1, value2 } };
        }

        public static CompoundIndexKeys Make<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            return new CompoundIndexKeys { Values = new object[] { value1, value2, value3 } };
        }

        public static CompoundIndexKeys Make<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            return new CompoundIndexKeys { Values = new object[] { value1, value2, value3, value4 } };
        }

        public static CompoundIndexKeys Make<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            return new CompoundIndexKeys { Values = new object[] { value1, value2, value3, value4, value5 } };
        }

        public static CompoundIndexKeys Make<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
        {
            return new CompoundIndexKeys { Values = new object[] { value1, value2, value3, value4, value5, value6 } };
        }

        public static CompoundIndexKeys Make<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
        {
            return new CompoundIndexKeys { Values = new object[] { value1, value2, value3, value4, value5, value6, value7 } };
        }

        public static CompoundIndexKeys Make<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
        {
            return new CompoundIndexKeys { Values = new object[] { value1, value2, value3, value4, value5, value6, value7, value8 } };
        }

        public static CompoundIndexKeys Make<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9)
        {
            return new CompoundIndexKeys { Values = new object[] { value1, value2, value3, value4, value5, value6, value7, value8, value9 } };
        }
    }
}
