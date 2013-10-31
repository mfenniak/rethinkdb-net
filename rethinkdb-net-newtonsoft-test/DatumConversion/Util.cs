using System;
using System.Globalization;
using System.Text;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test.DatumConversion
{
    public class Util
    {
        public static void DatumCamelCase( Datum d )
        {
            foreach( var pair in d.r_object )
            {
                pair.key = ToCamelCase( pair.key );
                DatumCamelCase( pair.val );
            }
            foreach( var ele in d.r_array )
            {
                DatumCamelCase( ele );
            }
        }

        private static string ToCamelCase( string s )
        {
            if( String.IsNullOrEmpty( s ) )
            {
                return s;
            }
            if( !Char.IsUpper( s[0] ) )
            {
                return s;
            }
            var builder = new StringBuilder();
            for( int i = 0; i < s.Length; i++ )
            {
                bool flag = ( i + 1 ) < s.Length;
                if( ( ( i == 0 ) || !flag ) || Char.IsUpper( s[i + 1] ) )
                {
                    char ch = Char.ToLower( s[i], CultureInfo.InvariantCulture );
                    builder.Append( ch );
                }
                else
                {
                    builder.Append( s.Substring( i ) );
                    break;
                }
            }
            return builder.ToString();
        }
    }
}