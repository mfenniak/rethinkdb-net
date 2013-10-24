using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace RethinkDb.DatumConverters
{
    public class DataContractDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly DataContractDatumConverterFactory Instance = new DataContractDatumConverterFactory();
        private static readonly object dynamicModuleLock = new object();
        private static AssemblyBuilder assemblyBuilder = null;
        private static ModuleBuilder dynamicModule = null;

        private DataContractDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");

            datumConverter = null;

            var dataContractAttribute = typeof(T).GetCustomAttribute<DataContractAttribute>();
            if (dataContractAttribute == null)
                return false;

            if (typeof(T).IsEnum)
                // [DataContract] on an enum type isn't supported by this datum converter.  It possibly should be,
                // as using the [EnumValue] attribute allows users to assign arbitrary text values to enums. (#146)
                return false;

            Type datumConverterType = TypeCache<T>.Instance.Value;
            datumConverter = (IDatumConverter<T>)Activator.CreateInstance(datumConverterType, rootDatumConverterFactory);
            return true;
        }

        private static class TypeCache<TType>
        {
            public static Lazy<Type> Instance = new Lazy<Type>(DataContractDatumConverterFactory.Create<TType>);
        }

        private static Type Create<T>()
        {
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

                Type baseClass;
                if (typeof(T).IsValueType)
                    baseClass = typeof(AbstractValueTypeDatumConverter<T>);
                else
                    baseClass = typeof(AbstractReferenceTypeDatumConverter<T>);

                TypeBuilder type = dynamicModule.DefineType(
                    "DataContractDatumConverterFactory." + typeof(T).FullName,
                    TypeAttributes.Class | TypeAttributes.Public,
                    baseClass
                );
                type.AddInterfaceImplementation(typeof(IObjectDatumConverter));

                var datumConverterFactoryField = DefineDatumConverterField(type);
                DefineConstructor(type, datumConverterFactoryField);
                DefineConvertDatum<T>(type, datumConverterFactoryField);
                DefineConvertObject<T>(type, datumConverterFactoryField);
                DefineGetDatumFieldName<T>(type);

                Type finalType = type.CreateType();
                //assemblyBuilder.Save("DataContractDynamicAssembly.dll");
                return finalType;

            }
        }

        private static FieldBuilder DefineDatumConverterField(TypeBuilder type)
        {
            return type.DefineField("datumConverterFactory", typeof(IDatumConverterFactory), FieldAttributes.Private | FieldAttributes.InitOnly);
        }

        private static void DefineConstructor(TypeBuilder type, FieldInfo datumConverterFactoryField)
        {
            var meth = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(IDatumConverterFactory) });
            meth.DefineParameter(1, System.Reflection.ParameterAttributes.None, "datumConverterFactory");

            var gen = meth.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, datumConverterFactoryField);
            gen.Emit(OpCodes.Ret);
        }

        private static void DefineConvertDatum<T>(TypeBuilder type, FieldInfo datumConverterFactoryField)
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

            Label nullDatumTypeLabel = gen.DefineLabel();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetGetMethod());
            gen.Emit(OpCodes.Ldc_I4, (int)Spec.Datum.DatumType.R_NULL);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brtrue, nullDatumTypeLabel);

            Label notObjectDatumTypeLabel = gen.DefineLabel();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetGetMethod());
            gen.Emit(OpCodes.Ldc_I4, (int)Spec.Datum.DatumType.R_OBJECT);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse, notObjectDatumTypeLabel);

            var retval = gen.DeclareLocal(typeof(T));
            gen.Emit(OpCodes.Newobj, typeof(T).GetConstructor(new Type[0]));
            gen.Emit(OpCodes.Stloc, retval);

            var factory = gen.DeclareLocal(typeof(IDatumConverterFactory));
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, datumConverterFactoryField);
            gen.Emit(OpCodes.Stloc, factory);

            var index = gen.DeclareLocal(typeof(int));
            var keyValue = gen.DeclareLocal(typeof(Spec.Datum.AssocPair));

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

                var loopStart = gen.DefineLabel();
                var exitLoop = gen.DefineLabel();

                // index = 0
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Stloc, index);

                // index < arg0.r_object.Count
                gen.MarkLabel(loopStart);
                gen.Emit(OpCodes.Ldloc, index);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("r_object").GetGetMethod());
                gen.Emit(OpCodes.Callvirt, typeof(System.Collections.Generic.List<Spec.Datum.AssocPair>).GetProperty("Count").GetGetMethod());
                gen.Emit(OpCodes.Clt);
                // if false, exit loop
                gen.Emit(OpCodes.Brfalse, exitLoop);

                // keyValue = arg0.r_object[index]
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("r_object").GetGetMethod());
                gen.Emit(OpCodes.Ldloc, index);
                gen.Emit(OpCodes.Callvirt, typeof(System.Collections.Generic.List<Spec.Datum.AssocPair>).GetProperty("Item").GetGetMethod());
                gen.Emit(OpCodes.Stloc, keyValue);

                // keyValue.key == "field"
                var keyCheckFail = gen.DefineLabel();
                gen.Emit(OpCodes.Ldloc, keyValue);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum.AssocPair).GetProperty("key").GetGetMethod());
                gen.Emit(OpCodes.Ldstr, fieldName);
                gen.Emit(OpCodes.Call, typeof(string).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(string) }, null));
                // if false, skip
                gen.Emit(OpCodes.Brfalse, keyCheckFail);

                // ConvertDatum and store in retval's field
                gen.Emit(OpCodes.Ldloc, retval);
                gen.Emit(OpCodes.Ldloc, factory);
                gen.Emit(OpCodes.Call,
                    typeof(DatumConverterFactoryExtensions)
                        .GetMethod("Get", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IDatumConverterFactory) }, null)
                         .MakeGenericMethod(field.FieldType));
                gen.Emit(OpCodes.Ldloc, keyValue);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum.AssocPair).GetProperty("val").GetGetMethod());
                gen.Emit(OpCodes.Callvirt, typeof(IDatumConverter<>).MakeGenericType(field.FieldType).GetMethod("ConvertDatum"));
                gen.Emit(OpCodes.Stfld, field);

                gen.MarkLabel(keyCheckFail);

                // index += 1
                gen.Emit(OpCodes.Ldloc, index);
                gen.Emit(OpCodes.Ldc_I4_1);
                gen.Emit(OpCodes.Add);
                gen.Emit(OpCodes.Stloc, index);

                gen.Emit(OpCodes.Br, loopStart);

                gen.MarkLabel(exitLoop);
            }

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var dataMemberAttribute = property.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberAttribute == null)
                    continue;

                string propertyName;
                if (!String.IsNullOrEmpty(dataMemberAttribute.Name))
                    propertyName = dataMemberAttribute.Name;
                else
                    propertyName = property.Name;

                var loopStart = gen.DefineLabel();
                var exitLoop = gen.DefineLabel();

                // index = 0
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Stloc, index);

                // index < arg0.r_object.Count
                gen.MarkLabel(loopStart);
                gen.Emit(OpCodes.Ldloc, index);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("r_object").GetGetMethod());
                gen.Emit(OpCodes.Callvirt, typeof(System.Collections.Generic.List<Spec.Datum.AssocPair>).GetProperty("Count").GetGetMethod());
                gen.Emit(OpCodes.Clt);
                // if false, exit loop
                gen.Emit(OpCodes.Brfalse, exitLoop);

                // keyValue = arg0.r_object[index]
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("r_object").GetGetMethod());
                gen.Emit(OpCodes.Ldloc, index);
                gen.Emit(OpCodes.Callvirt, typeof(System.Collections.Generic.List<Spec.Datum.AssocPair>).GetProperty("Item").GetGetMethod());
                gen.Emit(OpCodes.Stloc, keyValue);

                // keyValue.key == "field"
                var keyCheckFail = gen.DefineLabel();
                gen.Emit(OpCodes.Ldloc, keyValue);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum.AssocPair).GetProperty("key").GetGetMethod());
                gen.Emit(OpCodes.Ldstr, propertyName);
                gen.Emit(OpCodes.Call, typeof(string).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(string) }, null));
                // if false, skip
                gen.Emit(OpCodes.Brfalse, keyCheckFail);

                // ConvertDatum and store in retval's field
                gen.Emit(OpCodes.Ldloc, retval);
                gen.Emit(OpCodes.Ldloc, factory);
                gen.Emit(OpCodes.Call,
                    typeof(DatumConverterFactoryExtensions)
                        .GetMethod("Get", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IDatumConverterFactory) }, null)
                         .MakeGenericMethod(property.PropertyType));
                gen.Emit(OpCodes.Ldloc, keyValue);
                gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum.AssocPair).GetProperty("val").GetGetMethod());
                gen.Emit(OpCodes.Callvirt, typeof(IDatumConverter<>).MakeGenericType(property.PropertyType).GetMethod("ConvertDatum"));
                gen.Emit(OpCodes.Callvirt, property.GetSetMethod());

                gen.MarkLabel(keyCheckFail);

                // index += 1
                gen.Emit(OpCodes.Ldloc, index);
                gen.Emit(OpCodes.Ldc_I4_1);
                gen.Emit(OpCodes.Add);
                gen.Emit(OpCodes.Stloc, index);

                gen.Emit(OpCodes.Br, loopStart);

                gen.MarkLabel(exitLoop);
            }

            gen.Emit(OpCodes.Ldloc, retval);
            gen.Emit(OpCodes.Ret);

            gen.MarkLabel(notObjectDatumTypeLabel);
            gen.Emit(OpCodes.Ldstr, "Attempted to convert Datum to object, but Datum was type ");
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, typeof(Spec.Datum).GetProperty("type").GetGetMethod());
            gen.Emit(OpCodes.Box, typeof(Spec.Datum.DatumType));
            gen.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
            gen.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
            gen.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) }));
            gen.Emit(OpCodes.Throw);

            gen.MarkLabel(nullDatumTypeLabel);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ret);
        }

        private static void DefineConvertObject<T>(TypeBuilder type, FieldInfo datumConverterFactoryField)
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

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, datumConverterFactoryField);
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

                Label? skipInclude = null;
                if (!dataMemberAttribute.EmitDefaultValue)
                {
                    var local = gen.DeclareLocal(field.FieldType);
                    gen.Emit(OpCodes.Ldloca, local);
                    gen.Emit(OpCodes.Initobj, field.FieldType);

                    skipInclude = gen.DefineLabel();

                    if (field.FieldType.IsPrimitive)
                    {
                        gen.Emit(OpCodes.Ldarg_1);
                        gen.Emit(OpCodes.Ldfld, field);
                        gen.Emit(OpCodes.Ldloc, local);
                        gen.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        gen.Emit(OpCodes.Ldarg_1);
                        gen.Emit(OpCodes.Ldfld, field);
                        gen.Emit(OpCodes.Box, field.FieldType);
                        gen.Emit(OpCodes.Ldloc, local);
                        gen.Emit(OpCodes.Box, field.FieldType);
                        gen.Emit(OpCodes.Call, typeof(Object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public));
                    }

                    gen.Emit(OpCodes.Brtrue, skipInclude.Value);
                }

                gen.Emit(OpCodes.Ldloc, factory);
                gen.Emit(OpCodes.Call,
                    typeof(DatumConverterFactoryExtensions)
                        .GetMethod("Get", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IDatumConverterFactory) }, null)
                         .MakeGenericMethod(field.FieldType));
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

                if (skipInclude.HasValue)
                    gen.MarkLabel(skipInclude.Value);
            }

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var dataMemberAttribute = property.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberAttribute == null)
                    continue;

                string fieldName;
                if (!String.IsNullOrEmpty(dataMemberAttribute.Name))
                    fieldName = dataMemberAttribute.Name;
                else
                    fieldName = property.Name;

                Label? skipInclude = null;
                if (!dataMemberAttribute.EmitDefaultValue)
                {
                    var local = gen.DeclareLocal(property.PropertyType);
                    gen.Emit(OpCodes.Ldloca, local);
                    gen.Emit(OpCodes.Initobj, property.PropertyType);

                    skipInclude = gen.DefineLabel();

                    if (property.PropertyType.IsPrimitive)
                    {
                        gen.Emit(OpCodes.Ldarg_1);
                        gen.Emit(OpCodes.Callvirt, property.GetGetMethod());
                        gen.Emit(OpCodes.Ldloc, local);
                        gen.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        gen.Emit(OpCodes.Ldarg_1);
                        gen.Emit(OpCodes.Callvirt, property.GetGetMethod());
                        gen.Emit(OpCodes.Box, property.PropertyType);
                        gen.Emit(OpCodes.Ldloc, local);
                        gen.Emit(OpCodes.Box, property.PropertyType);
                        gen.Emit(OpCodes.Call, typeof(Object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public));
                    }

                    gen.Emit(OpCodes.Brtrue, skipInclude.Value);
                }

                gen.Emit(OpCodes.Ldloc, factory);
                gen.Emit(OpCodes.Call,
                    typeof(DatumConverterFactoryExtensions)
                        .GetMethod("Get", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IDatumConverterFactory) }, null)
                         .MakeGenericMethod(property.PropertyType));
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Callvirt, property.GetGetMethod());
                gen.Emit(OpCodes.Callvirt, typeof(IDatumConverter<>).MakeGenericType(property.PropertyType).GetMethod("ConvertObject"));
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

                if (skipInclude.HasValue)
                    gen.MarkLabel(skipInclude.Value);
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

        private static void DefineGetDatumFieldName<T>(TypeBuilder type)
        {
            // FIXME: current implementation is just a series of If checks; could be more efficient with a dictionary
            // lookup. (issue #59)

            MethodBuilder meth = type.DefineMethod(
                "GetDatumFieldName",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(string),
                new Type[] {
                    typeof(MemberInfo)
                });
            meth.DefineParameter(1, System.Reflection.ParameterAttributes.None, "member");

            var gen = meth.GetILGenerator();
            var memberName = gen.DeclareLocal(typeof(string));

            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetProperty("Name").GetGetMethod());
            gen.Emit(OpCodes.Stloc, memberName);

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

                var nopeWrongField = gen.DefineLabel();

                gen.Emit(OpCodes.Ldloc, memberName);
                gen.Emit(OpCodes.Ldstr, field.Name); // the actual field name...
                gen.Emit(OpCodes.Call, typeof(string).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(string) }, null));
                gen.Emit(OpCodes.Brfalse, nopeWrongField);

                gen.Emit(OpCodes.Ldstr, fieldName); // the dataMemberAttribute's field name
                gen.Emit(OpCodes.Ret);

                gen.MarkLabel(nopeWrongField);
            }

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var dataMemberAttribute = property.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberAttribute == null)
                    continue;

                string fieldName;
                if (!String.IsNullOrEmpty(dataMemberAttribute.Name))
                    fieldName = dataMemberAttribute.Name;
                else
                    fieldName = property.Name;

                var nopeWrongField = gen.DefineLabel();

                gen.Emit(OpCodes.Ldloc, memberName);
                gen.Emit(OpCodes.Ldstr, property.Name); // the actual field name...
                gen.Emit(OpCodes.Call, typeof(string).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(string) }, null));
                gen.Emit(OpCodes.Brfalse, nopeWrongField);

                gen.Emit(OpCodes.Ldstr, fieldName); // the dataMemberAttribute's field name
                gen.Emit(OpCodes.Ret);

                gen.MarkLabel(nopeWrongField);
            }

            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ret);
        }
    }
}
