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

            if (typeof(CompoundIndexKey).IsAssignableFrom(typeof(T)))
            {
                var retval = Activator.CreateInstance(
                    typeof(CompoundIndexKeyDatumConverterShim<>).MakeGenericType(typeof(T)),
                    new object[] { rootDatumConverterFactory }
                );
                datumConverter = (IDatumConverter<T>)retval;
            }

            return datumConverter != null;
        }

        public class CompoundIndexKeyDatumConverterShim<T> : AbstractReferenceTypeDatumConverter<T>
            where T : CompoundIndexKey
        {
            private readonly CompoundIndexKeyDatumConverter datumConverter;

            public CompoundIndexKeyDatumConverterShim(IDatumConverterFactory rootDatumConverterFactory)
            {
                this.datumConverter = new CompoundIndexKeyDatumConverter(rootDatumConverterFactory);
            }

            #region implemented abstract members of AbstractReferenceTypeDatumConverter

            public override T ConvertDatum(Datum datum)
            {
                throw new NotSupportedException("Converting back to a CompoundIndexKey object is not supported.");
            }

            public override Datum ConvertObject(T value)
            {
                return datumConverter.ConvertObject(value);
            }

            #endregion
        }

        public class CompoundIndexKeyDatumConverter : AbstractReferenceTypeDatumConverter<CompoundIndexKey>
        {
            private readonly IDatumConverterFactory rootDatumConverterFactory;

            public CompoundIndexKeyDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
            {
                this.rootDatumConverterFactory = rootDatumConverterFactory;
            }

            public override CompoundIndexKey ConvertDatum(Datum datum)
            {
                throw new NotSupportedException("Converting back to a CompoundIndexKey object is not supported.");
            }

            public override Datum ConvertObject(CompoundIndexKey compoundIndexKey)
            {
                if (compoundIndexKey == null)
                    return new Datum { type = Datum.DatumType.R_NULL };

                var retval = new Datum {type = Datum.DatumType.R_ARRAY};
                foreach (var key in compoundIndexKey.KeyValues)
                {
                    var converter = rootDatumConverterFactory.Get(key.GetType());
                    retval.r_array.Add(converter.ConvertObject(key));
                }

                return retval;
            }
        }
    }
}
