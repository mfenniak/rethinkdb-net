using System.Collections.Generic;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class InsertQuery<T> : IWriteQuery<DmlResponse>
    {
        private readonly ITableQuery<T> tableTerm;
        private readonly IEnumerable<T> objects;
        private readonly Conflict conflict;

        public InsertQuery(ITableQuery<T> tableTerm, IEnumerable<T> objects, Conflict conflict)
        {
            this.tableTerm = tableTerm;
            this.objects = objects;
            this.conflict = conflict;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var insertTerm = new Term()
            {
                type = Term.TermType.INSERT,
            };
            insertTerm.args.Add(tableTerm.GenerateTerm(datumConverterFactory));

            var objectArray = new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
            };
            var converter = datumConverterFactory.Get<T>();
            foreach (var obj in objects)
            {
                objectArray.r_array.Add(converter.ConvertObject(obj));
            }
            insertTerm.args.Add(new Term()
            {
                type = Term.TermType.DATUM,
                datum = objectArray,
            });

            insertTerm.optargs.Add(new Term.AssocPair()
            {
                key = "conflict",
                val = new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = conflict.ToString().ToLower(),
                    }
                }
            });

            return insertTerm;
        }
    }
}
