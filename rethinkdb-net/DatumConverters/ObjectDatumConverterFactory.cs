using System;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public class ObjectDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly ObjectDatumConverterFactory Instance = new ObjectDatumConverterFactory();

        private ObjectDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (typeof(T) == typeof(Object))
                datumConverter = (IDatumConverter<T>)new ObjectDatumConverter(rootDatumConverterFactory);

            return datumConverter != null;
        }
    }

    public class ObjectDatumConverter : AbstractReferenceTypeDatumConverter<Object>
    {
        private IDatumConverterFactory rootDatumConverterFactory;

        public ObjectDatumConverter(IDatumConverterFactory rootDatumConverterFactory)
        {
            this.rootDatumConverterFactory = rootDatumConverterFactory;
        }

        #region IDatumConverter<Uri> Members

        public override Object ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Datum.DatumType.R_NULL)
                return null;

            Type valueType = rootDatumConverterFactory.GetBestNativeTypeForDatum(datum);
            var valueConverter = rootDatumConverterFactory.Get(valueType);
            return valueConverter.ConvertDatum(datum);
        }

        public override Spec.Datum ConvertObject(Object obj)
        {                
            if (obj == null)
                return new Datum() { type = Datum.DatumType.R_NULL };

            // What to do here... I don't know why someone would be doing this in the first place, we really only expected
            // to return null here.  Once we find the use-case that this works for, we'll worry about it.
            throw new Exception("Not able to convert non-null Object to a ReQL datum");
        }

        #endregion
    }
}
