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

        Spec.Term IWriteQuery<T>.GenerateTerm(IDatumConverter<T> converter)
        {
            var insertTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.INSERT,
            };
            insertTerm.args.Add(((ISequenceQuery<T>)tableTerm).GenerateTerm());

            var objectArray = new Spec.Datum()
            {
                type = Spec.Datum.DatumType.R_ARRAY,
            };
            foreach (var obj in objects)
            {
                objectArray.r_array.Add(converter.ConvertObject(obj));
            }
            insertTerm.args.Add(new Spec.Term()
            {
                type = Spec.Term.TermType.DATUM,
                datum = objectArray,
            });

            if (upsert)
            {
                insertTerm.optargs.Add(new Spec.Term.AssocPair()
                {
                    key = "upsert",
                    val = new Spec.Term()
                    {
                        type = Spec.Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_BOOL,
                            r_bool = upsert,
                        }
                    }
                });
            }

            return insertTerm;
        }
    }
}
