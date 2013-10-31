using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RethinkDb.Newtonsoft.Test
{
    public class ObjectDumper
    {
        public static void Dump( object o )
        {
            Dump( o, 0, new ArrayList(), null );
        }

        public static string DumpToString( object o )
        {
            var sb = new StringBuilder();
            Dump( o, 0, new ArrayList(), sb );
            return sb.ToString();
        }

        private static string Pad( int level, string msg, params object[] args )
        {
            string val = String.Format( msg, args );
            return val.PadLeft( ( level * 4 ) + val.Length );
        }

        private static void Dump( object o, int level, ArrayList previous, StringBuilder sb )
        {
            Type type = null;

            if( o != null )
            {
                type = o.GetType();
            }

            Dump( o, type, null, level, previous, sb );
        }

        private static void Dump( object o, Type type, string name, int level, ArrayList previous, StringBuilder sb )
        {
            if( o == null )
            {
                string str;
                if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                {
                    var args = type.GetGenericArguments()
                        .Select( t => t.Name )
                        .ToArray();

                    var argStr = string.Join( ", ", args );

                    str = Pad( level, "[{1}?] {0}: (null)", name, argStr );
                }
                else
                {
                    str = Pad( level, "[{1}] {0}: (null)", name, type.Name );
                }
                if( sb == null )
                    Console.WriteLine( str );
                else
                    sb.AppendLine( str );
                return;
            }

            if( previous.Contains( o ) )
            {
                return;
            }

            previous.Add( o );

            if( type.IsPrimitive || o is string || o is Guid || o is Enum )
            {
                DumpPrimitive( o, type, name, level, previous, sb );
            }
            else
            {
                DumpComposite( o, type, name, level, previous, sb );
            }
        }

        private static void DumpPrimitive( object o, Type type, string name, int level, ArrayList previous, StringBuilder sb )
        {
            if( name != null )
            {
                var str = Pad( level, "[{1}] {0}: {2}", name, type.Name, o );
                if( sb == null )
                    Console.WriteLine( str );
                else
                    sb.AppendLine( str );
            }
            else
            {
                var str = Pad( level, "({0}) {1}", type.Name, o );
                if( sb == null )
                    Console.WriteLine( str );
                else
                    sb.AppendLine( str );
            }
        }

        private static void DumpComposite( object o, Type type, string name, int level, ArrayList previous, StringBuilder sb )
        {

            if( name != null )
            {
                var pad = Pad( level, "{0} ({1}):", name, type.Name );
                if( sb == null )
                    Console.WriteLine( pad );
                else
                    sb.AppendLine( pad );
            }
            else
            {
                var pad = Pad( level, "({0})", type.Name );
                if( sb == null )
                    Console.WriteLine( pad );
                else
                    sb.AppendLine( pad );
            }

            if( o is IDictionary )
            {
                DumpDictionary( (IDictionary)o, level, previous, sb );
            }
            else if( o is ICollection )
            {
                DumpCollection( (ICollection)o, level, previous, sb );
            }
            else
            {
                MemberInfo[] members = o.GetType().GetMembers( BindingFlags.Instance | BindingFlags.Public |
                                                               BindingFlags.NonPublic );

                foreach( MemberInfo member in members )
                {
                    try
                    {
                        DumpMember( o, member, level, previous, sb );
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void DumpCollection( ICollection collection, int level, ArrayList previous, StringBuilder sb )
        {
            foreach( object child in collection )
            {
                Dump( child, level + 1, previous, sb );
            }
        }

        private static void DumpDictionary( IDictionary dictionary, int level, ArrayList previous, StringBuilder sb )
        {
            foreach( object key in dictionary.Keys )
            {
                var pad = Pad( level + 1, "[{0}] ({1}):", key, key.GetType().Name );
                if( sb == null )
                    Console.WriteLine( pad );
                else
                    sb.AppendLine( pad );

                Dump( dictionary[key], level + 2, previous, sb );
            }
        }

        private static void DumpMember( object o, MemberInfo member, int level, ArrayList previous, StringBuilder sb )
        {
            if( member is MethodInfo || member is ConstructorInfo ||
                member is EventInfo )
                return;

            if( member is FieldInfo )
            {
                FieldInfo field = (FieldInfo)member;

                string name = member.Name;
                if( ( field.Attributes & FieldAttributes.Public ) == 0 )
                {
                    name = "#" + name;
                }
                if( name.Contains( "__BackingField" ) ) return;

                Dump( field.GetValue( o ), field.FieldType, name, level + 1, previous, sb );
            }
            else if( member is PropertyInfo )
            {
                PropertyInfo prop = (PropertyInfo)member;

                if( prop.GetIndexParameters().Length == 0 && prop.CanRead )
                {
                    string name = member.Name;
                    MethodInfo getter = prop.GetGetMethod();

                    if( ( getter.Attributes & MethodAttributes.Public ) == 0 )
                    {
                        name = "#" + name;
                    }

                    Dump( prop.GetValue( o, null ), prop.PropertyType, name, level + 1, previous, sb );
                }
            }
        }
    }
}