using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft
{
    public class DatumWriterToken
    {
        public DatumWriterToken(Datum.DatumType type)
        {
            this.Datum = new Datum() {type = type};
        }

        public DatumWriterToken(Datum datum)
        {
            Datum = datum;
        }

        public DatumWriterToken Parent { get; set; }
        public Datum Datum { get; set; }
    }
}