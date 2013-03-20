using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace RethinkDb
{
    public class DataContractDatumConverterFactory : IDatumConverterFactory
    {
        private static PrimitiveDatumConverterFactory primitiveDatumConverterFactory = new PrimitiveDatumConverterFactory();
        private static object dynamicModuleLock = new object();
        private static AssemblyBuilder assemblyBuilder = null;
        private static ModuleBuilder dynamicModule = null;

        private static class Cache<TType>
        {
            public static Lazy<IDatumConverter<TType>> Instance = new Lazy<IDatumConverter<TType>>(DataContractDatumConverterFactory.Create<TType>);
        }

        public IDatumConverter<T> Get<T>()
        {
            if (primitiveDatumConverterFactory.IsTypeSupported(typeof(T)))
                return primitiveDatumConverterFactory.Get<T>();
            else
                return Cache<T>.Instance.Value;
        }

        private static IDatumConverter<T> Create<T>()
        {
            var dataContractAttribute = typeof(T).GetCustomAttribute<DataContractAttribute>();
            if (dataContractAttribute == null)
                throw new NotSupportedException(String.Format("Type {0} is not marked with DataContractAttribute", typeof(T)));

            lock (dynamicModuleLock)
            {
                if (dynamicModule == null)
                {
                    var myDomain = AppDomain.CurrentDomain;
                    var name = new AssemblyName("DataContractDynamicAssembly");

                    assemblyBuilder = myDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                    dynamicModule = assemblyBuilder.DefineDynamicModule("DataContractDynamicAssembly");

                    //assemblyBuilder = myDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
                    //dynamicModule = assemblyBuilder.DefineDynamicModule("DataContractDynamicAssembly", "DataContractDynamicAssembly.dll");
                }

                TypeBuilder type = dynamicModule.DefineType("DataContractDatumConverterFactory." + typeof(T).FullName, TypeAttributes.Class | TypeAttributes.Public);
                type.AddInterfaceImplementation(typeof(IDatumConverter<T>));

                DefineConvertDatum<T>(type);
                DefineConvertObject<T>(type);

                Type finalType = type.CreateType();

                //assemblyBuilder.Save("DataContractDynamicAssembly.dll");

                return (IDatumConverter<T>)Activator.CreateInstance(finalType);
            }
        }

        private static void DefineConvertDatum<T>(TypeBuilder type)
        {
            MethodBuilder meth = type.DefineMethod(
                "ConvertDatum",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(T),
                new Type[] {
                    typeof(Spec.Datum)
                });
            meth.DefineParameter(1, System.Reflection.ParameterAttributes.None, "datum");

            var gen = meth.GetILGenerator();
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ret);
        }

        //private static string TestConvertDatum(Spec.Datum test)
        //{
        //    if (test.type != Spec.Datum.DatumType.R_STR)
        //        throw new InvalidOperationException("Attempted to cast Datum to string, but Datum was type " + test.type.ToString());
        //    return test.r_str;
        //}

        private static void DefineConvertObject<T>(TypeBuilder type)
        {
            MethodBuilder meth = type.DefineMethod(
                "ConvertObject",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(Spec.Datum),
                new Type[] {
                    typeof(T)
                });
            meth.DefineParameter(1, System.Reflection.ParameterAttributes.None, "obj");

            var gen = meth.GetILGenerator();


            var retval = gen.DeclareLocal(typeof(Spec.Datum));
            var fieldDatum = gen.DeclareLocal(typeof(Spec.Datum));
            var factory = gen.DeclareLocal(typeof(IDatumConverterFactory));
            var keyValue = gen.DeclareLocal(typeof(Spec.Datum.AssocPair));

            Label nullObjectLabel = gen.DefineLabel();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brtrue, nullObjectLabel);

            gen.Emit(OpCodes.Newobj, typeof(DataContractDatumConverterFactory).GetConstructor(new Type[] { }));
            gen.Emit(OpCodes.Stloc, factory);

            gen.Emit(OpCodes.Newobj, typeof(Spec.Datum).GetConstructor(new Type[] { }));
            gen.Emit(OpCodes.Stloc, retval);

            gen.Emit(OpCodes.Ldloc, retval);
            gen.Emit(OpCodes.Ldc_I4, (int)Spec.Datum.DatumType.R_OBJECT);
            gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetSetMethod());

            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var dataMemberAttribute = field.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberAttribute == null)
                    continue;

                string fieldName;
                if (!String.IsNullOrEmpty(dataMemberAttribute.Name))
                    fieldName = dataMemberAttribute.Name;
                else
                    fieldName = field.Name;

                gen.Emit(OpCodes.Ldloc, factory);
                gen.Emit(OpCodes.Callvirt, typeof(IDatumConverterFactory).GetMethod("Get").MakeGenericMethod(field.FieldType));
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Ldfld, field);
                gen.Emit(OpCodes.Callvirt, typeof(IDatumConverter<>).MakeGenericType(field.FieldType).GetMethod("ConvertObject"));
                gen.Emit(OpCodes.Stloc, fieldDatum);

                gen.Emit(OpCodes.Newobj, typeof(Spec.Datum.AssocPair).GetConstructor(new Type[0]));
                gen.Emit(OpCodes.Stloc, keyValue);

                gen.Emit(OpCodes.Ldloc, keyValue);
                gen.Emit(OpCodes.Ldstr, fieldName);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum.AssocPair).GetProperty("key").GetSetMethod());

                gen.Emit(OpCodes.Ldloc, keyValue);
                gen.Emit(OpCodes.Ldloc, fieldDatum);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum.AssocPair).GetProperty("val").GetSetMethod());

                gen.Emit(OpCodes.Ldloc, retval);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("r_object").GetGetMethod());
                gen.Emit(OpCodes.Ldloc, keyValue);
                gen.Emit(OpCodes.Callvirt, typeof(System.Collections.Generic.List<Spec.Datum.AssocPair>).GetMethod("Add"));
            }

            gen.Emit(OpCodes.Ldloc, retval);
            gen.Emit(OpCodes.Ret);

            gen.MarkLabel(nullObjectLabel);
            gen.Emit(OpCodes.Newobj, typeof(Spec.Datum).GetConstructor(new Type[] { }));
            gen.Emit(OpCodes.Stloc, retval);
            gen.Emit(OpCodes.Ldloc, retval);
            gen.Emit(OpCodes.Ldc_I4, (int)Spec.Datum.DatumType.R_NULL);
            gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetSetMethod());
            gen.Emit(OpCodes.Ldloc, retval);
            gen.Emit(OpCodes.Ret);
        }

        //private static Spec.Datum TestConvertObject(string test)
        //{
        //    if (test == null)
        //        return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };

        //    return new Spec.Datum()
        //    {
        //        type = Spec.Datum.DatumType.R_STR,
        //        r_str = test
        //    };
        //}
    }
}
