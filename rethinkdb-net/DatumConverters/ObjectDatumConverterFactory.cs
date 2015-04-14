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
                datumConverter = (IDatumConverter<T>)ObjectDatumConverter.Instance.Value;

            return datumConverter != null;
        }
    }

    public class ObjectDatumConverter : AbstractReferenceTypeDatumConverter<Object>
    {
        public static readonly Lazy<ObjectDatumConverter> Instance = new Lazy<ObjectDatumConverter>(() => new ObjectDatumConverter());

        #region IDatumConverter<Uri> Members

        public override Object ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Datum.DatumType.R_NULL)
                return null;

            // What to do here... I don't know why someone would be doing this in the first place, we really only expected
            // to return null here.  Maybe it should be polymorphic behavior; return the best matching type for the
            // datum?
            throw new Exception("Not able to convert non-null Datum to an Object");
        }

        public override Spec.Datum ConvertObject(Object obj)
        {                
            if (obj == null)
                return new Datum() { type = Datum.DatumType.R_NULL };

            // Again, maybe this should be some polymorphic behavior?
            throw new Exception("Not able to convert non-null Object to a ReQL datum");
        }

        #endregion
    }
}
