using System;
using System.Linq.Expressions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
#if false
    public class GroupedMapReduceQuery<TOriginal, TGroup, TMap> : ISequenceQuery<Tuple<TGroup, TMap>>
    {
        private readonly ISequenceQuery<TOriginal> sequenceQuery;
        private readonly Expression<Func<TOriginal, TGroup>> grouping;
        private readonly Expression<Func<TOriginal, TMap>> mapping;
        private readonly Expression<Func<TMap, TMap, TMap>> reduction;
        private readonly bool baseProvided;
        private readonly TMap @base;

        public GroupedMapReduceQuery(ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TGroup>> grouping, Expression<Func<TOriginal, TMap>> mapping, Expression<Func<TMap, TMap, TMap>> reduction)
        {
            this.sequenceQuery = sequenceQuery;
            this.grouping = grouping;
            this.mapping = mapping;
            this.reduction = reduction;
        }

        public GroupedMapReduceQuery(ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TGroup>> grouping, Expression<Func<TOriginal, TMap>> mapping, Expression<Func<TMap, TMap, TMap>> reduction, TMap @base)
            : this(sequenceQuery, grouping, mapping, reduction)
        {
            this.baseProvided = true;
            this.@base = @base;
        }

        public Term GenerateTerm (IDatumConverterFactory datumConverterFactory)
        {
            var retval = new Term()
            {
                type = Term.TermType.GROUP,
            };
            retval.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));
            retval.args.Add(ExpressionUtils.CreateFunctionTerm(datumConverterFactory, grouping));
            retval.args.Add(ExpressionUtils.CreateFunctionTerm(datumConverterFactory, mapping));
            retval.args.Add(ExpressionUtils.CreateFunctionTerm(datumConverterFactory, reduction));

            if (this.baseProvided)
            {
                retval.optargs.Add(new Term.AssocPair()
                {
                    key = "base",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = datumConverterFactory.Get<TMap>().ConvertObject(@base)
                    }
                });
            }

            return retval;
        }
    }
#endif
}

