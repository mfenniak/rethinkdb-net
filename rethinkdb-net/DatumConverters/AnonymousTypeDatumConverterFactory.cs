using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;

namespace RethinkDb.DatumConverters
{
    public class AnonymousTypeDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly AnonymousTypeDatumConverterFactory Instance = new AnonymousTypeDatumConverterFactory();

        private AnonymousTypeDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");
            if (!IsTypeSupported(typeof(T)))
                return false;

            datumConverter = new AnonymousTypeConverter<T>(rootDatumConverterFactory);
            return true;
        }

        public bool IsTypeSupported(Type t)
        {
            if (t.IsClass && t.Name.Contains("Anon") && t.Name.StartsWith("<>") && t.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length == 1)
                return true;
            return false;
        }

        public class AnonymousTypeConverter<T> : AbstractReferenceTypeDatumConverter<T>, IObjectDatumConverter
        {
            private readonly ConstructorInfo typeConstructor;
            private readonly List<PropertyInfo> properties;

            private class PropertyInfo
            {
                public string Name;
                public int Index;
                public IDatumConverter DatumConverter;
                public MethodInfo GetMethod;
            }

            public AnonymousTypeConverter(IDatumConverterFactory innerTypeConverterFactory)
            {
                typeConstructor = typeof(T).GetConstructors()[0];

                properties = new List<PropertyInfo>();
                foreach (var property in typeof(T).GetProperties())
                {
                    var pi = new PropertyInfo();
                    pi.Name = property.Name;
                    pi.Index = properties.Count;
                    pi.DatumConverter = innerTypeConverterFactory.Get(property.PropertyType);
                    pi.GetMethod = property.GetGetMethod();
                    properties.Add(pi);
                }
            }

            #region IDatumConverter<T> Members

            public override T ConvertDatum(Spec.Datum datum)
            {
                if (datum.type == Spec.Datum.DatumType.R_NULL)
                {
                    return default(T);
                }
                else if (datum.type == Spec.Datum.DatumType.R_OBJECT)
                {
                    object[] constructorParameters = new object[properties.Count];

                    foreach (var assocPair in datum.r_object)
                    {
                        var property = properties.Where(pi => String.Equals(pi.Name, assocPair.key, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                        if (property == null)
                            throw new InvalidOperationException("Unexpected key/value pair in anonymous-type object: " + assocPair.key);
                        constructorParameters[property.Index] = property.DatumConverter.ConvertDatum(assocPair.val);
                    }

                    return (T)(typeConstructor.Invoke(constructorParameters));
                }
                else
                    throw new NotSupportedException("Attempted to cast Datum to anonymous type, but Datum was unsupported type " + datum.type);
            }

            public override Spec.Datum ConvertObject(T anonymousObject)
            {
                if (anonymousObject == null)
                    return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };

                var datum = new Spec.Datum() { type = Spec.Datum.DatumType.R_OBJECT };
                foreach (var property in properties)
                {
                    var value = property.GetMethod.Invoke(anonymousObject, new object[0]);
                    datum.r_object.Add(new Spec.Datum.AssocPair() {
                        key = property.Name,
                        val = property.DatumConverter.ConvertObject(value),
                    });
                }
                return datum;
            }

            #endregion
            #region IObjectDatumConverter implementation

            public string GetDatumFieldName(MemberInfo memberInfo)
            {
                return memberInfo.Name;
            }

            #endregion
        }
    }
}
