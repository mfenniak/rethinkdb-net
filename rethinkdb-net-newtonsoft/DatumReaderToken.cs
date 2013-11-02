using System.Collections.Generic;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft
{
    public class DatumReaderToken
    {
        public DatumReaderToken(Datum d)
        {
            this.Datum = d;
            if (Datum.type == Datum.DatumType.R_OBJECT)
                this.AssocPairs = Datum.r_object.GetEnumerator();
            else if (Datum.type == Datum.DatumType.R_ARRAY)
                this.Array = Datum.r_array.GetEnumerator();
        }

        public bool IsObject
        {
            get { return Datum.type == Datum.DatumType.R_OBJECT; }
        }

        public bool IsArray
        {
            get { return Datum.type == Datum.DatumType.R_ARRAY; }
        }

        public IEnumerator<Datum> Array { get; private set; }
        public IEnumerator<Datum.AssocPair> AssocPairs { get; private set; }
        public Datum Datum { get; private set; }
    }
}