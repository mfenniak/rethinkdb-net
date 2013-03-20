using System;
using System.Reflection;
using System.Reflection.Emit;

namespace RethinkDb
{
    public class PrimitiveDatumConverterFactory : IDatumConverterFactory
    {
        private static object dynamicModuleLock = new object();
        private static AssemblyBuilder assemblyBuilder = null;
        private static ModuleBuilder dynamicModule = null;

        private static class Cache<TType>
        {
            public static Lazy<IDatumConverter<TType>> Instance = new Lazy<IDatumConverter<TType>>(PrimitiveDatumConverterFactory.Create<TType>);
        }

        public IDatumConverter<T> Get<T>()
        {
            if (!IsTypeSupported(typeof(T)))
                throw new NotSupportedException(String.Format("Type {0} is not supported by PrimitiveDatumConverterFactory", typeof(T)));
            return Cache<T>.Instance.Value;
        }

        public bool IsTypeSupported(Type t)
        {
            if (t == typeof(string))
                return true;
            return false;
        }

        private static IDatumConverter<T> Create<T>()
        {
            lock (dynamicModuleLock)
            {
                if (dynamicModule == null)
                {
                    var myDomain = AppDomain.CurrentDomain;
                    var name = new AssemblyName("PrimitiveDynamicAssembly");

                    assemblyBuilder = myDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                    dynamicModule = assemblyBuilder.DefineDynamicModule("PrimitiveDynamicAssembly");

                    //assemblyBuilder = myDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
                    //dynamicModule = assemblyBuilder.DefineDynamicModule("PrimitiveDynamicAssembly", "PrimitiveDynamicAssembly.dll");
                }

                TypeBuilder type = dynamicModule.DefineType("PrimitiveDatumConverterFactory." + typeof(T).FullName, TypeAttributes.Class | TypeAttributes.Public);
                type.AddInterfaceImplementation(typeof(IDatumConverter<T>));

                DefineConvertDatum<T>(type);
                DefineConvertObject<T>(type);

                Type finalType = type.CreateType();

                //assemblyBuilder.Save("PrimitiveDynamicAssembly.dll");

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

            if (typeof(T) == typeof(string))
            {
                Label nullDatumTypeLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetGetMethod());
                gen.Emit(OpCodes.Ldc_I4, (int)Spec.Datum.DatumType.R_NULL);
                gen.Emit(OpCodes.Ceq);
                gen.Emit(OpCodes.Brtrue, nullDatumTypeLabel);

                Label incorrectDatumTypeLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetGetMethod());
                gen.Emit(OpCodes.Ldc_I4, (int)Spec.Datum.DatumType.R_STR);
                gen.Emit(OpCodes.Ceq);
                gen.Emit(OpCodes.Brfalse, incorrectDatumTypeLabel);

                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("r_str").GetGetMethod());
                gen.Emit(OpCodes.Ret);

                gen.MarkLabel(nullDatumTypeLabel);
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ret);

                gen.MarkLabel(incorrectDatumTypeLabel);
                gen.Emit(OpCodes.Ldstr, "Attempted to cast Datum to string, but Datum was type ");
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetGetMethod());
                gen.Emit(OpCodes.Box, typeof(Spec.Datum.DatumType));
                gen.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
                gen.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
                gen.Emit(OpCodes.Newobj, typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) }));
                gen.Emit(OpCodes.Throw);
            }
            else
            {
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ret);
            }
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

            if (typeof(T) == typeof(string))
            {
                var local = gen.DeclareLocal(typeof(Spec.Datum));

                Label nullObjectLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ceq);
                gen.Emit(OpCodes.Brtrue, nullObjectLabel);

                gen.Emit(OpCodes.Newobj, typeof(Spec.Datum).GetConstructor(new Type[] { }));
                gen.Emit(OpCodes.Stloc, local);

                gen.Emit(OpCodes.Ldloc, local);
                gen.Emit(OpCodes.Ldc_I4, (int)Spec.Datum.DatumType.R_STR);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetSetMethod());

                gen.Emit(OpCodes.Ldloc, local);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("r_str").GetSetMethod());

                gen.Emit(OpCodes.Ldloc, local);
                gen.Emit(OpCodes.Ret);

                gen.MarkLabel(nullObjectLabel);
                gen.Emit(OpCodes.Newobj, typeof(Spec.Datum).GetConstructor(new Type[] { }));
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloc, local);
                gen.Emit(OpCodes.Ldc_I4, (int)Spec.Datum.DatumType.R_NULL);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetSetMethod());
                gen.Emit(OpCodes.Ldloc, local);
                gen.Emit(OpCodes.Ret);
            }
            else
            {
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Ret);
            }
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
