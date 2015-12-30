using UnityEngine;
using System.Collections;
using System.Globalization;
using System;

namespace NumberExtension {
    public static class Extension {
        public static string ToThousandFormatString( this ulong number ) {
            CultureInfo ci = new CultureInfo( "en-us" );
            return number.ToString( "N0", ci );
        }

        public static string ToThousandFormatString( this int number ) {
            CultureInfo ci = new CultureInfo( "en-us" );
            return number.ToString( "N0", ci );
        }

        public static ulong ParseThousandFormatToNumber( this string numberStr ) {
            string result = String.Join( "", numberStr.Split( ',' ) );
            return ulong.Parse( result );
        }

        public static ulong ToUlong( this object value ) {
            return ulong.Parse( value.ToString() );
        }

        public static int ToInt( this object value ) {
            return int.Parse( value.ToString() );
        }

        public static float ToFloat( this object value ) {
            return float.Parse( value.ToString() );
        }

        public static double ToDouble( this object value ) {
            return double.Parse( value.ToString() );
        }
    }

}
