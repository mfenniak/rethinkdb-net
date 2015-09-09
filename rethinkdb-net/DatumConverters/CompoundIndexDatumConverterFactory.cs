using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public class CompoundIndexDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly CompoundIndexDatumConverterFactory Instance = new CompoundIndexDatumConverterFactory();

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");

            datumConverter = null;

            if (typeof(T) == typeof(CompoundIndexKeys))
                datumConverter = (IDatumConverter<T>)new CompoundIndexDatumConverter(rootDatumConverterFactory);

            return datumConverter != null;
        }

        public class CompoundIndexDatumConverter : AbstractReferenceTypeDatumConverter<CompoundIndexKeys>
        {
            private readonly IDatumConverterFactory _rootDatumConverterFactory;

            public CompoundIndexDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
            {
                _rootDatumConverterFactory = rootDatumConverterFactory;
            }

            public override CompoundIndexKeys ConvertDatum(Datum datum)
            {
                throw new NotSupportedException("Converting back to a CompoundIndexKeys object is not supported.");
            }

            public override Datum ConvertObject(CompoundIndexKeys compoundIndexKeys)
            {
                if (compoundIndexKeys == null)
                    return new Datum {type = Datum.DatumType.R_NULL};

                var retval = new Datum {type = Datum.DatumType.R_ARRAY};
                foreach (var key in compoundIndexKeys.Values)
                {
                    var converter = _rootDatumConverterFactory.Get(key.GetType());
                    retval.r_array.Add(converter.ConvertObject(key));
                }

                return retval;
            }
        }
    }
}
