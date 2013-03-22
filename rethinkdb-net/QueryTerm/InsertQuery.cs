using System.Collections.Generic;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class InsertQuery<T> : IWriteQuery<T>
    {
        private readonly TableQuery<T> tableTerm;
        private readonly IEnumerable<T> objects;
        private readonly bool upsert;

        public InsertQuery(TableQuery<T> tableTerm, IEnumerable<T> objects, bool upsert)
        {
            this.tableTerm = tableTerm;
            this.objects = objects;
            this.upsert = upsert;
        }

        public Term GenerateTerm(IDatumConverter<T> converter)
        {
            var insertTerm = new Term()
            {
                type = Term.TermType.INSERT,
            };
            insertTerm.args.Add(tableTerm.GenerateTerm());

            var objectArray = new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
            };
            foreach (var obj in objects)
            {
                objectArray.r_array.Add(converter.ConvertObject(obj));
            }
            insertTerm.args.Add(new Term()
            {
                type = Term.TermType.DATUM,
                datum = objectArray,
            });

            if (upsert)
            {
                insertTerm.optargs.Add(new Term.AssocPair()
                {
                    key = "upsert",
                    val = new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_BOOL,
                            r_bool = upsert,
                        }
                    }
                });
            }

            return insertTerm;
        }
    }
}
