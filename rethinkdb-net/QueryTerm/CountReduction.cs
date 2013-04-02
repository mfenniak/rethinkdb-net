using System;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class CountReduction : IGroupByReduction<double>
    {
        public static readonly CountReduction Instance = new CountReduction();
        private Term retval;

        private CountReduction()
        {
        }

        public Term GenerateReductionObject(IDatumConverterFactory datumConverterFactory)
        {
            if (retval == null)
            {
                var newValue = new Term() {
                    type = Term.TermType.MAKE_OBJ
                };
                newValue.optargs.Add(new Term.AssocPair() {
                    key = "COUNT",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_BOOL,
                            r_bool = true
                        }
                    }
                });
                retval = newValue;
            }

            return retval;
        }
    }
}

