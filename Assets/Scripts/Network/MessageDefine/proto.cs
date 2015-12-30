using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

    public abstract class ProtoMessage {
        public abstract bool Parse( byte[] buf, int index, int msg_size );
        public abstract bool Serialize( byte[] buf, int index );
        public abstract int ByteSize();
        public abstract void Clear();
        public abstract bool IsInitialized();
        public abstract void Print();
        
        public void PrintBinary( byte[] buf, int buf_size ) {
            for ( int i = 0; i < buf_size; i++ ) 
                Console.Write( "{0:X2} ", buf[i] );
            Console.WriteLine();
        }

        public void PrintBinary( byte[] buf ) {
            PrintBinary( buf, buf.Length );
        }

        //----------int32----------
        protected int ReadInt( byte[] buf, int index, ref int read_len ) {
            ulong ret = 0;
            read_len = Varint.Decode( buf, index, ref ret );
            return (int)ret;
        }

        protected int WriteInt( byte[] buf, int index, int val ) {
            if ( val < 0 )
                return Varint.Encode64( buf, index, (ulong)val );
            else
                return Varint.Encode32( buf, index, (uint)val );
        }

        protected int WriteIntSize( int val ) {
            if ( val < 0 )
                return Varint.GetEncode64Size( (ulong)val );
            else
                return Varint.GetEncode32Size( (uint)val );
        }

        //----------uint32----------
        protected uint ReadUInt( byte[] buf, int index, ref int read_len ) {
            ulong ret = 0;
            read_len = Varint.Decode( buf, index, ref ret );
            return (uint)ret;
        }

        protected int WriteUInt( byte[] buf, int index, uint val ) {
            return Varint.Encode32( buf, index, val );
        }

        protected int WriteUIntSize( uint val ) {
            return Varint.GetEncode32Size( val );
        }

        //----------int64----------
        protected long ReadLong( byte[] buf, int index, ref int read_len ) {
            ulong ret = 0;
            read_len = Varint.Decode( buf, index, ref ret );
            return (long)ret;
        }

        protected int WriteLong( byte[] buf, int index, long val ) {
            return Varint.Encode64( buf, index, (ulong)val );
        }

        protected int WriteLongSize( long val ) {
            return Varint.GetEncode64Size( (ulong)val );
        }
        
        //----------uint64----------
        protected ulong ReadULong( byte[] buf, int index, ref int read_len ) {
            ulong ret = 0;
            read_len = Varint.Decode( buf, index, ref ret );
            return ret;
        }

        protected int WriteULong( byte[] buf, int index, ulong val ) {
            return Varint.Encode64( buf, index, val );
        }

        protected int WriteULongSize( ulong val ) {
            return Varint.GetEncode64Size( val );
        }

        //----------sint32----------
        protected int ReadSInt( byte[] buf, int index, ref int read_len ) {
            ulong ret = 0;
            read_len = Varint.Decode( buf, index, ref ret );
            int val = (int)ret;
            return Varint.Dezigzag32( val );
        }

        protected int WriteSInt( byte[] buf, int index, int val ) {
            val = Varint.Zigzag32( val );
            return Varint.Encode32( buf, index, (uint)val );
        }

        protected int WriteSIntSize( int val ) {
            val = Varint.Zigzag32( val );
            return Varint.GetEncode32Size( (uint)val );
        }

        //----------sint64----------
        protected long ReadSLong( byte[] buf, int index, ref int read_len ) {
            ulong ret = 0;
            read_len = Varint.Decode( buf, index, ref ret );
            long val = (long)ret;
            return Varint.Dezigzag64( val );
        }

        protected int WriteSLong( byte[] buf, int index, long val ) {
            val = Varint.Zigzag64( val );
            return Varint.Encode64( buf, index, (ulong)val );
        }

        protected int WriteSLongSize( long val ) {
            val = Varint.Zigzag64( val );
            return Varint.GetEncode64Size( (ulong)val );
        }

        //----------sfixed64----------
        protected const int SIZE_32BIT = 4;
        protected const int SIZE_64BIT = 8;
        protected long ReadSFixed64( byte[] buf, int index, ref int read_len ) {
            return (long) ReadFixed64( buf, index, ref read_len );
        }

        protected int WriteSFixed64( byte[] buf, int index, long val ) {
            for ( int i = 0, sh = 0; i < 8; i++, sh += 8)
                buf[index+i] = (byte)(val >> sh);
            return SIZE_64BIT;
        }

        //----------fixed64----------
        protected ulong ReadFixed64( byte[] buf, int index, ref int read_len ) {
            read_len = SIZE_64BIT;
            uint ret_low = (uint) (buf[index+0]
                          | (buf[index+1] << 8)
                          | (buf[index+2] << 16)
                          | (buf[index+3] << 24) );
            uint ret_high = (uint) (buf[index+4]
                          | (buf[index+5] << 8)
                          | (buf[index+6] << 16)
                          | (buf[index+7] << 24) );
            return ((((ulong) ret_high) << 32) | ret_low);
        }

        protected int WriteFixed64( byte[] buf, int index, ulong val ) {
            for (int i = 0, sh = 0; i < 8; i++, sh += 8)
                buf[index+i] = (byte) (val >> sh);
            return SIZE_64BIT;
        }

        //----------sfixed32----------
        protected int ReadSFixed32( byte[] buf, int index, ref int read_len ) {
            read_len = SIZE_32BIT;
            return ( buf[index+0] | (buf[index+1] << 8) | (buf[index+2] << 16) | (buf[index+3] << 24));
        }

        protected int WriteSFixed32( byte[] buf, int index, int val ) {
            buf[index+0] = (byte)val;
            buf[index+1] = (byte)(val >> 8);
            buf[index+2] = (byte)(val >> 16);
            buf[index+3] = (byte)(val >> 24);
            return SIZE_32BIT;
        }
    
        //----------fixed32----------
        protected uint ReadFixed32( byte[] buf, int index, ref int read_len ) {
            read_len = SIZE_32BIT;
            return ( (uint)(buf[index+0] | (buf[index+1] << 8) | (buf[index+2] << 16) | (buf[index+3] << 24)));
        }

        protected int WriteFixed32( byte[] buf, int index, uint val ) {
            buf[index+0] = (byte)val;
            buf[index+1] = (byte)(val >> 8);
            buf[index+2] = (byte)(val >> 16);
            buf[index+3] = (byte)(val >> 24);
            return SIZE_32BIT;
        }

        //----------double----------
        protected double ReadDouble( byte[] buf, int index, ref int read_len ) {
            read_len = SIZE_64BIT;
            return BitConverter.ToDouble( buf, index );
        }

        protected int WriteDouble( byte[] buf, int index, double val ) {
            byte[] tmp = BitConverter.GetBytes( val );
            Array.Copy( tmp, 0, buf, index, SIZE_64BIT );
            return SIZE_64BIT;
        }

        //----------float----------
        protected float ReadFloat( byte[] buf, int index, ref int read_len ) {
            read_len = SIZE_32BIT;
            return BitConverter.ToSingle( buf, index );
        }

        protected int WriteFloat( byte[] buf, int index, float val ) {
            byte[] tmp = BitConverter.GetBytes( val );
            Array.Copy( tmp, 0, buf, index, SIZE_32BIT );
            return SIZE_32BIT;
        }

        //----------string----------
        public string ReadString( byte[] buf, int buf_size, int index, ref int read_len ) {
            int len = 0;
            int str_len = ReadInt( buf, index, ref len );
            if ( buf_size < len || str_len < 0 )
                return null;

            index += len;
            buf_size -= len;

            if ( buf_size < str_len ) 
                return null;

            read_len = str_len + len;
            if ( str_len > 0 )
                return System.Text.Encoding.UTF8.GetString( buf, index, str_len );
            else
                return "";
        }

        public int WriteString( byte[] buf, int buf_size, int index, string str ) {
            byte[] str_buf = System.Text.Encoding.UTF8.GetBytes( str );
            int str_len = str_buf.Length;
            int len = WriteInt( buf, index, str_len );
            
            index += len;
            buf_size -= len;

            if ( buf_size < str_len ) 
                throw new Exception( string.Format( "PBCS:WriteString error, buf_size:{0} < str_len:{1}", buf_size, str_len ) );

            Array.Copy( str_buf, 0, buf, index, str_len );
            return str_len + len;
        }

        public int WriteStringSize( string str ) {
            return System.Text.Encoding.UTF8.GetByteCount( str );
        }

        //----------bytes----------
        public byte[] ReadBytes( byte[] buf, int buf_size, int index, ref int read_len ) {
            int len = 0;
            int bytes_len = ReadInt( buf, index, ref len );
            if ( buf_size < len || bytes_len < 0 )
                return null;

            index += len;
            buf_size -= len;

            if ( buf_size < bytes_len )
                return null;

            byte[] ret = new byte[bytes_len];
            Array.Copy( buf, index, ret, 0, bytes_len );
            read_len = bytes_len + len;
            return ret;
        }

        public int WriteBytes( byte[] buf, int buf_size, int index, byte[] arr ) {
            int len = WriteInt( buf, index, arr.Length );
            index += len;
            buf_size -= len;

            if ( buf_size < arr.Length ) 
                throw new Exception( string.Format( "PBCS:WriteString error, buf_size:{0} < arr.Length:{1}", buf_size, arr.Length ) );

            Array.Copy( arr, 0, buf, index, arr.Length );
            return arr.Length + len;
        }

        //---------unknow field----------
        public const int  WIRE_TYPE_VARINT  = 0;
        public const int  WIRE_TYPE_64BIT   = 1;
        public const int  WIRE_TYPE_LENGTH  = 2;
        public const int  WIRE_TYPE_32_BIT  = 5;
        protected int GetUnknowFieldValueSize( byte[] buf, int index, int wire_tag ) {
            int wire_type = wire_tag & 0x7;
            int read_len = 0;
            
            switch ( wire_type ) {
            case WIRE_TYPE_VARINT:
                ReadULong( buf, index, ref read_len );
                break;
            case WIRE_TYPE_64BIT:
                read_len = SIZE_64BIT;
                break;
            case WIRE_TYPE_LENGTH:
                int size = ReadInt( buf, index, ref read_len );
                if ( size < 0 ) size = 0;
                read_len = size + read_len;
                break;
            case WIRE_TYPE_32_BIT:
                read_len = SIZE_32BIT;
                break;
            }

            return read_len;
        }
    }


    public class Varint {
        public static int Encode32( byte[] buf, int index, uint number ) {
            if (number < 0x80) {
                buf[index+0] = (byte) number ; 
                return 1;
            }
            buf[index+0] = (byte) (number | 0x80 );
            if (number < 0x4000) {
                buf[index+1] = (byte) (number >> 7 );
                return 2;
            }
            buf[index+1] = (byte) ((number >> 7) | 0x80 );
            if (number < 0x200000) {
                buf[index+2] = (byte) (number >> 14);
                return 3;
            }
            buf[index+2] = (byte) ((number >> 14) | 0x80 );
            if (number < 0x10000000) {
                buf[index+3] = (byte) (number >> 21);
                return 4;
            }
            buf[index+3] = (byte) ((number >> 21) | 0x80 );
            buf[index+4] = (byte) (number >> 28);
            return 5;
        }

        public static int GetEncode32Size( uint number ) {
            if (number < 0x80) 
                return 1;
            if (number < 0x4000) 
                return 2;
            if (number < 0x200000) 
                return 3;
            if (number < 0x10000000) 
                return 4;
            return 5;
        }

        public static int Encode64( byte[] buf, int index, ulong number ) {
            if ( (number & 0xffffffff) == number ) 
                return Encode32( buf, index, (uint)number );

            int i = 0;
            do {
                buf[index+i] = (byte)(number | 0x80);
                number >>= 7;
                ++i;
            } while (number >= 0x80);
            buf[index+i] = (byte)number;
            return i+1;
        }

        public static int GetEncode64Size( ulong number ) {
            if ( (number & 0xffffffff) == number ) 
                return GetEncode32Size( (uint)number );

            int i = 0;
            do {
                number >>= 7;
                ++i;
            } while (number >= 0x80);
            return i+1;
        }

        public static int Decode( byte[] buf, int index, ref ulong result ) {
            if ( (buf[index+0] & 0x80) == 0 ) {
                result = buf[index+0];
                return 1;
            }

            uint r = (uint)(buf[index+0]) & 0x7f;
            for ( int i = 1; i < 4; i++ ) {
                r |= (uint)( (buf[index+i] & 0x7f ) << (7*i) );
                if ( ( buf[index+i] & 0x80 ) == 0 ) {
                    result = r;
                    return i+1;
                }
            }

            ulong lr = 0;
            for ( int i = 4; i < 10; i++ ) {
                lr |= (ulong)(buf[index+i] & 0x7f ) << ( 7*(i-4) );
                if ( ( buf[index+i] & 0x80 ) == 0 ) {
                    result = (ulong)(lr >> 4) << 32;
                    result |= r | (((uint)lr & 0xf) << 28);
                    return i+1;
                }
            }

            result = 0;
            return 10;
        }

        public static int Zigzag32( int n ) {
            return (n << 1) ^ (n >> 31);
        }

        public static long Zigzag64( long n ) {
            return (n << 1) ^ (n >> 63);
        }

        public static int Dezigzag32( int n ) {
            return (int)((uint)n >> 1) ^ -(n & 1);
        }

        public static long Dezigzag64( long n ) {
            return (long)((ulong)n >> 1) ^ -(n & 1);
        }
    }

public partial class proto {
public partial class BattleRatingReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int victorypoints_limit = 3
    private int victorypoints_limit_ = 0;
    public int victorypoints_limit {
        get { return victorypoints_limit_; }
        set { victorypoints_limit_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_victorypoints_limit() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_victorypoints_limit() {
        victorypoints_limit_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int exp_give = 4
    private int exp_give_ = 0;
    public int exp_give {
        get { return exp_give_; }
        set { exp_give_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_exp_give() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_exp_give() {
        exp_give_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_victorypoints_limit() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, victorypoints_limit );
        }
        if ( has_exp_give() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, exp_give );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                victorypoints_limit = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                exp_give = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.BattleRatingReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        victorypoints_limit_ = 0;
        exp_give_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_victorypoints_limit() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( victorypoints_limit );
        }
        if ( has_exp_give() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( exp_give );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "victorypoints_limit: " );
        if ( has_victorypoints_limit() ) 
            Console.WriteLine( victorypoints_limit );
        Console.Write( "exp_give: " );
        if ( has_exp_give() ) 
            Console.WriteLine( exp_give );
    }

}
}

public partial class proto {
public partial class BattlefieldReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int battlefield_type = 3
    private int battlefield_type_ = 0;
    public int battlefield_type {
        get { return battlefield_type_; }
        set { battlefield_type_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_battlefield_type() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_battlefield_type() {
        battlefield_type_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int battlefield_unit_config = 4
    private int battlefield_unit_config_ = 0;
    public int battlefield_unit_config {
        get { return battlefield_unit_config_; }
        set { battlefield_unit_config_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_battlefield_unit_config() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_battlefield_unit_config() {
        battlefield_unit_config_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field string scene_resfile = 5
    private string scene_resfile_ = "";
    public string scene_resfile {
        get { return scene_resfile_; }
        set { scene_resfile_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xffffffef;
              else
                  has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_scene_resfile() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_scene_resfile() {
        scene_resfile_ = "";
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int battlefield_wid = 6
    private int battlefield_wid_ = 0;
    public int battlefield_wid {
        get { return battlefield_wid_; }
        set { battlefield_wid_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_battlefield_wid() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_battlefield_wid() {
        battlefield_wid_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field int deployarea_len = 7
    private int deployarea_len_ = 0;
    public int deployarea_len {
        get { return deployarea_len_; }
        set { deployarea_len_ = value; 
              has_flag_0 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_deployarea_len() {
        return ( has_flag_0 & 0x40 ) != 0;
    }
    public void clear_deployarea_len() {
        deployarea_len_ = 0;
        has_flag_0 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    //field int deptharea_len = 8
    private int deptharea_len_ = 0;
    public int deptharea_len {
        get { return deptharea_len_; }
        set { deptharea_len_ = value; 
              has_flag_0 |= 0x80;
              cached_byte_size = 0; }
    }
    public bool has_deptharea_len() {
        return ( has_flag_0 & 0x80 ) != 0;
    }
    public void clear_deptharea_len() {
        deptharea_len_ = 0;
        has_flag_0 &= 0xffffff7f;
        cached_byte_size = 0;
    }

    //field int basearea_len = 9
    private int basearea_len_ = 0;
    public int basearea_len {
        get { return basearea_len_; }
        set { basearea_len_ = value; 
              has_flag_0 |= 0x100;
              cached_byte_size = 0; }
    }
    public bool has_basearea_len() {
        return ( has_flag_0 & 0x100 ) != 0;
    }
    public void clear_basearea_len() {
        basearea_len_ = 0;
        has_flag_0 &= 0xfffffeff;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_battlefield_type() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, battlefield_type );
        }
        if ( has_battlefield_unit_config() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, battlefield_unit_config );
        }
        if ( has_scene_resfile() ) {
            __index += WriteInt( buf, __index, 42 );
            __index += WriteString( buf, buf_size-__index, __index, scene_resfile );
        }
        if ( has_battlefield_wid() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, battlefield_wid );
        }
        if ( has_deployarea_len() ) {
            __index += WriteInt( buf, __index, 56 );
            __index += WriteInt( buf, __index, deployarea_len );
        }
        if ( has_deptharea_len() ) {
            __index += WriteInt( buf, __index, 64 );
            __index += WriteInt( buf, __index, deptharea_len );
        }
        if ( has_basearea_len() ) {
            __index += WriteInt( buf, __index, 72 );
            __index += WriteInt( buf, __index, basearea_len );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                battlefield_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                battlefield_unit_config = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 42:
                scene_resfile = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( scene_resfile == null ) return false;
                __index += read_len;
                break;
            case 48:
                battlefield_wid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 56:
                deployarea_len = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 64:
                deptharea_len = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 72:
                basearea_len = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.BattlefieldReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        battlefield_type_ = 0;
        battlefield_unit_config_ = 0;
        scene_resfile_ = "";
        battlefield_wid_ = 0;
        deployarea_len_ = 0;
        deptharea_len_ = 0;
        basearea_len_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_battlefield_type() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( battlefield_type );
        }
        if ( has_battlefield_unit_config() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( battlefield_unit_config );
        }
        if ( has_scene_resfile() ) {
            cached_byte_size += WriteIntSize( 42 );
            int size = WriteStringSize( scene_resfile );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_battlefield_wid() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( battlefield_wid );
        }
        if ( has_deployarea_len() ) {
            cached_byte_size += WriteIntSize( 56 );
            cached_byte_size += WriteIntSize( deployarea_len );
        }
        if ( has_deptharea_len() ) {
            cached_byte_size += WriteIntSize( 64 );
            cached_byte_size += WriteIntSize( deptharea_len );
        }
        if ( has_basearea_len() ) {
            cached_byte_size += WriteIntSize( 72 );
            cached_byte_size += WriteIntSize( basearea_len );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "battlefield_type: " );
        if ( has_battlefield_type() ) 
            Console.WriteLine( battlefield_type );
        Console.Write( "battlefield_unit_config: " );
        if ( has_battlefield_unit_config() ) 
            Console.WriteLine( battlefield_unit_config );
        Console.Write( "scene_resfile: " );
        if ( has_scene_resfile() ) 
            Console.WriteLine( scene_resfile );
        Console.Write( "battlefield_wid: " );
        if ( has_battlefield_wid() ) 
            Console.WriteLine( battlefield_wid );
        Console.Write( "deployarea_len: " );
        if ( has_deployarea_len() ) 
            Console.WriteLine( deployarea_len );
        Console.Write( "deptharea_len: " );
        if ( has_deptharea_len() ) 
            Console.WriteLine( deptharea_len );
        Console.Write( "basearea_len: " );
        if ( has_basearea_len() ) 
            Console.WriteLine( basearea_len );
    }

}
}

public partial class proto {
public partial class BreakingRateReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int breaking_rate_limit = 3
    private int breaking_rate_limit_ = 0;
    public int breaking_rate_limit {
        get { return breaking_rate_limit_; }
        set { breaking_rate_limit_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_breaking_rate_limit() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_breaking_rate_limit() {
        breaking_rate_limit_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int victorypoints_give = 4
    private int victorypoints_give_ = 0;
    public int victorypoints_give {
        get { return victorypoints_give_; }
        set { victorypoints_give_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_victorypoints_give() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_victorypoints_give() {
        victorypoints_give_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_breaking_rate_limit() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, breaking_rate_limit );
        }
        if ( has_victorypoints_give() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, victorypoints_give );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                breaking_rate_limit = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                victorypoints_give = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.BreakingRateReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        breaking_rate_limit_ = 0;
        victorypoints_give_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_breaking_rate_limit() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( breaking_rate_limit );
        }
        if ( has_victorypoints_give() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( victorypoints_give );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "breaking_rate_limit: " );
        if ( has_breaking_rate_limit() ) 
            Console.WriteLine( breaking_rate_limit );
        Console.Write( "victorypoints_give: " );
        if ( has_victorypoints_give() ) 
            Console.WriteLine( victorypoints_give );
    }

}
}

public partial class proto {
public partial class BuffReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int bufftype = 3
    private int bufftype_ = 0;
    public int bufftype {
        get { return bufftype_; }
        set { bufftype_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_bufftype() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_bufftype() {
        bufftype_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int effect_val = 4
    private int effect_val_ = 0;
    public int effect_val {
        get { return effect_val_; }
        set { effect_val_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_effect_val() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_effect_val() {
        effect_val_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int intervaltime = 5
    private int intervaltime_ = 0;
    public int intervaltime {
        get { return intervaltime_; }
        set { intervaltime_ = value; 
              has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_intervaltime() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_intervaltime() {
        intervaltime_ = 0;
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int time = 6
    private int time_ = 0;
    public int time {
        get { return time_; }
        set { time_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_time() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_time() {
        time_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field string cartoon_res = 7
    private string cartoon_res_ = "";
    public string cartoon_res {
        get { return cartoon_res_; }
        set { cartoon_res_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xffffffbf;
              else
                  has_flag_0 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_cartoon_res() {
        return ( has_flag_0 & 0x40 ) != 0;
    }
    public void clear_cartoon_res() {
        cartoon_res_ = "";
        has_flag_0 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    //field string explain = 8
    private string explain_ = "";
    public string explain {
        get { return explain_; }
        set { explain_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xffffff7f;
              else
                  has_flag_0 |= 0x80;
              cached_byte_size = 0; }
    }
    public bool has_explain() {
        return ( has_flag_0 & 0x80 ) != 0;
    }
    public void clear_explain() {
        explain_ = "";
        has_flag_0 &= 0xffffff7f;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_bufftype() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, bufftype );
        }
        if ( has_effect_val() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, effect_val );
        }
        if ( has_intervaltime() ) {
            __index += WriteInt( buf, __index, 40 );
            __index += WriteInt( buf, __index, intervaltime );
        }
        if ( has_time() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, time );
        }
        if ( has_cartoon_res() ) {
            __index += WriteInt( buf, __index, 58 );
            __index += WriteString( buf, buf_size-__index, __index, cartoon_res );
        }
        if ( has_explain() ) {
            __index += WriteInt( buf, __index, 66 );
            __index += WriteString( buf, buf_size-__index, __index, explain );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                bufftype = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                effect_val = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 40:
                intervaltime = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 48:
                time = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 58:
                cartoon_res = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( cartoon_res == null ) return false;
                __index += read_len;
                break;
            case 66:
                explain = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( explain == null ) return false;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.BuffReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        bufftype_ = 0;
        effect_val_ = 0;
        intervaltime_ = 0;
        time_ = 0;
        cartoon_res_ = "";
        explain_ = "";
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_bufftype() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( bufftype );
        }
        if ( has_effect_val() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( effect_val );
        }
        if ( has_intervaltime() ) {
            cached_byte_size += WriteIntSize( 40 );
            cached_byte_size += WriteIntSize( intervaltime );
        }
        if ( has_time() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( time );
        }
        if ( has_cartoon_res() ) {
            cached_byte_size += WriteIntSize( 58 );
            int size = WriteStringSize( cartoon_res );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_explain() ) {
            cached_byte_size += WriteIntSize( 66 );
            int size = WriteStringSize( explain );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "bufftype: " );
        if ( has_bufftype() ) 
            Console.WriteLine( bufftype );
        Console.Write( "effect_val: " );
        if ( has_effect_val() ) 
            Console.WriteLine( effect_val );
        Console.Write( "intervaltime: " );
        if ( has_intervaltime() ) 
            Console.WriteLine( intervaltime );
        Console.Write( "time: " );
        if ( has_time() ) 
            Console.WriteLine( time );
        Console.Write( "cartoon_res: " );
        if ( has_cartoon_res() ) 
            Console.WriteLine( cartoon_res );
        Console.Write( "explain: " );
        if ( has_explain() ) 
            Console.WriteLine( explain );
    }

}
}

public partial class proto {
public partial class C2SLogin: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field string account = 1
    private string account_ = "";
    public string account {
        get { return account_; }
        set { account_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffe;
              else
                  has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_account() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_account() {
        account_ = "";
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string pasw = 2
    private string pasw_ = "";
    public string pasw {
        get { return pasw_; }
        set { pasw_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_pasw() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_pasw() {
        pasw_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int version = 3
    private int version_ = 0;
    public int version {
        get { return version_; }
        set { version_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_version() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_version() {
        version_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field string pf = 4
    private string pf_ = "";
    public string pf {
        get { return pf_; }
        set { pf_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffff7;
              else
                  has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_pf() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_pf() {
        pf_ = "";
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field string extparam = 5
    private string extparam_ = "";
    public string extparam {
        get { return extparam_; }
        set { extparam_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xffffffef;
              else
                  has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_extparam() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_extparam() {
        extparam_ = "";
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int pid = 6
    private int pid_ = 0;
    public int pid {
        get { return pid_; }
        set { pid_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_pid() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_pid() {
        pid_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field string gameversion = 7
    private string gameversion_ = "";
    public string gameversion {
        get { return gameversion_; }
        set { gameversion_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xffffffbf;
              else
                  has_flag_0 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_gameversion() {
        return ( has_flag_0 & 0x40 ) != 0;
    }
    public void clear_gameversion() {
        gameversion_ = "";
        has_flag_0 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    //field string phonemark = 8
    private string phonemark_ = "";
    public string phonemark {
        get { return phonemark_; }
        set { phonemark_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xffffff7f;
              else
                  has_flag_0 |= 0x80;
              cached_byte_size = 0; }
    }
    public bool has_phonemark() {
        return ( has_flag_0 & 0x80 ) != 0;
    }
    public void clear_phonemark() {
        phonemark_ = "";
        has_flag_0 &= 0xffffff7f;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_account() ) {
            __index += WriteInt( buf, __index, 10 );
            __index += WriteString( buf, buf_size-__index, __index, account );
        }
        if ( has_pasw() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, pasw );
        }
        if ( has_version() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, version );
        }
        if ( has_pf() ) {
            __index += WriteInt( buf, __index, 34 );
            __index += WriteString( buf, buf_size-__index, __index, pf );
        }
        if ( has_extparam() ) {
            __index += WriteInt( buf, __index, 42 );
            __index += WriteString( buf, buf_size-__index, __index, extparam );
        }
        if ( has_pid() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, pid );
        }
        if ( has_gameversion() ) {
            __index += WriteInt( buf, __index, 58 );
            __index += WriteString( buf, buf_size-__index, __index, gameversion );
        }
        if ( has_phonemark() ) {
            __index += WriteInt( buf, __index, 66 );
            __index += WriteString( buf, buf_size-__index, __index, phonemark );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 10:
                account = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( account == null ) return false;
                __index += read_len;
                break;
            case 18:
                pasw = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( pasw == null ) return false;
                __index += read_len;
                break;
            case 24:
                version = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 34:
                pf = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( pf == null ) return false;
                __index += read_len;
                break;
            case 42:
                extparam = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( extparam == null ) return false;
                __index += read_len;
                break;
            case 48:
                pid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 58:
                gameversion = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( gameversion == null ) return false;
                __index += read_len;
                break;
            case 66:
                phonemark = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( phonemark == null ) return false;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.C2SLogin" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        account_ = "";
        pasw_ = "";
        version_ = 0;
        pf_ = "";
        extparam_ = "";
        pid_ = 0;
        gameversion_ = "";
        phonemark_ = "";
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_account() ) {
            cached_byte_size += WriteIntSize( 10 );
            int size = WriteStringSize( account );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_pasw() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( pasw );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_version() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( version );
        }
        if ( has_pf() ) {
            cached_byte_size += WriteIntSize( 34 );
            int size = WriteStringSize( pf );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_extparam() ) {
            cached_byte_size += WriteIntSize( 42 );
            int size = WriteStringSize( extparam );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_pid() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( pid );
        }
        if ( has_gameversion() ) {
            cached_byte_size += WriteIntSize( 58 );
            int size = WriteStringSize( gameversion );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_phonemark() ) {
            cached_byte_size += WriteIntSize( 66 );
            int size = WriteStringSize( phonemark );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x7 ) != 0x7 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "account: " );
        if ( has_account() ) 
            Console.WriteLine( account );
        Console.Write( "pasw: " );
        if ( has_pasw() ) 
            Console.WriteLine( pasw );
        Console.Write( "version: " );
        if ( has_version() ) 
            Console.WriteLine( version );
        Console.Write( "pf: " );
        if ( has_pf() ) 
            Console.WriteLine( pf );
        Console.Write( "extparam: " );
        if ( has_extparam() ) 
            Console.WriteLine( extparam );
        Console.Write( "pid: " );
        if ( has_pid() ) 
            Console.WriteLine( pid );
        Console.Write( "gameversion: " );
        if ( has_gameversion() ) 
            Console.WriteLine( gameversion );
        Console.Write( "phonemark: " );
        if ( has_phonemark() ) 
            Console.WriteLine( phonemark );
    }

}
}

public partial class proto {
public partial class DefensiveUnitsReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int units_res_id = 3
    private int units_res_id_ = 0;
    public int units_res_id {
        get { return units_res_id_; }
        set { units_res_id_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_units_res_id() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_units_res_id() {
        units_res_id_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field string explain = 4
    private string explain_ = "";
    public string explain {
        get { return explain_; }
        set { explain_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffff7;
              else
                  has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_explain() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_explain() {
        explain_ = "";
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_units_res_id() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, units_res_id );
        }
        if ( has_explain() ) {
            __index += WriteInt( buf, __index, 34 );
            __index += WriteString( buf, buf_size-__index, __index, explain );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                units_res_id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 34:
                explain = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( explain == null ) return false;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.DefensiveUnitsReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        units_res_id_ = 0;
        explain_ = "";
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_units_res_id() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( units_res_id );
        }
        if ( has_explain() ) {
            cached_byte_size += WriteIntSize( 34 );
            int size = WriteStringSize( explain );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "units_res_id: " );
        if ( has_units_res_id() ) 
            Console.WriteLine( units_res_id );
        Console.Write( "explain: " );
        if ( has_explain() ) 
            Console.WriteLine( explain );
    }

}
}

public partial class proto {
public partial class EffectVal: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int casterunitid = 1
    private int casterunitid_ = 0;
    public int casterunitid {
        get { return casterunitid_; }
        set { casterunitid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_casterunitid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_casterunitid() {
        casterunitid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int partid = 2
    private int partid_ = 0;
    public int partid {
        get { return partid_; }
        set { partid_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_partid() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_partid() {
        partid_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int val = 3
    private int val_ = 0;
    public int val {
        get { return val_; }
        set { val_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_val() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_val() {
        val_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_casterunitid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, casterunitid );
        }
        if ( has_partid() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, partid );
        }
        if ( has_val() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, val );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                casterunitid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                partid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 24:
                val = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.EffectVal" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        casterunitid_ = 0;
        partid_ = 0;
        val_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_casterunitid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( casterunitid );
        }
        if ( has_partid() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( partid );
        }
        if ( has_val() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( val );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "casterunitid: " );
        if ( has_casterunitid() ) 
            Console.WriteLine( casterunitid );
        Console.Write( "partid: " );
        if ( has_partid() ) 
            Console.WriteLine( partid );
        Console.Write( "val: " );
        if ( has_val() ) 
            Console.WriteLine( val );
    }

}
}

public partial class proto {
public partial class LevelDeployUnitReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int units_id = 3
    private int units_id_ = 0;
    public int units_id {
        get { return units_id_; }
        set { units_id_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_units_id() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_units_id() {
        units_id_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int battlefield_id = 4
    private int battlefield_id_ = 0;
    public int battlefield_id {
        get { return battlefield_id_; }
        set { battlefield_id_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_battlefield_id() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_battlefield_id() {
        battlefield_id_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int level = 5
    private int level_ = 0;
    public int level {
        get { return level_; }
        set { level_ = value; 
              has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_level() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_level() {
        level_ = 0;
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int x_position = 6
    private int x_position_ = 0;
    public int x_position {
        get { return x_position_; }
        set { x_position_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_x_position() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_x_position() {
        x_position_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field int z_position = 7
    private int z_position_ = 0;
    public int z_position {
        get { return z_position_; }
        set { z_position_ = value; 
              has_flag_0 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_z_position() {
        return ( has_flag_0 & 0x40 ) != 0;
    }
    public void clear_z_position() {
        z_position_ = 0;
        has_flag_0 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_units_id() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, units_id );
        }
        if ( has_battlefield_id() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, battlefield_id );
        }
        if ( has_level() ) {
            __index += WriteInt( buf, __index, 40 );
            __index += WriteInt( buf, __index, level );
        }
        if ( has_x_position() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, x_position );
        }
        if ( has_z_position() ) {
            __index += WriteInt( buf, __index, 56 );
            __index += WriteInt( buf, __index, z_position );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                units_id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                battlefield_id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 40:
                level = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 48:
                x_position = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 56:
                z_position = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.LevelDeployUnitReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        units_id_ = 0;
        battlefield_id_ = 0;
        level_ = 0;
        x_position_ = 0;
        z_position_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_units_id() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( units_id );
        }
        if ( has_battlefield_id() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( battlefield_id );
        }
        if ( has_level() ) {
            cached_byte_size += WriteIntSize( 40 );
            cached_byte_size += WriteIntSize( level );
        }
        if ( has_x_position() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( x_position );
        }
        if ( has_z_position() ) {
            cached_byte_size += WriteIntSize( 56 );
            cached_byte_size += WriteIntSize( z_position );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "units_id: " );
        if ( has_units_id() ) 
            Console.WriteLine( units_id );
        Console.Write( "battlefield_id: " );
        if ( has_battlefield_id() ) 
            Console.WriteLine( battlefield_id );
        Console.Write( "level: " );
        if ( has_level() ) 
            Console.WriteLine( level );
        Console.Write( "x_position: " );
        if ( has_x_position() ) 
            Console.WriteLine( x_position );
        Console.Write( "z_position: " );
        if ( has_z_position() ) 
            Console.WriteLine( z_position );
    }

}
}

public partial class proto {
public partial class LostUnitInfo: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int refid = 1
    private int refid_ = 0;
    public int refid {
        get { return refid_; }
        set { refid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_refid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_refid() {
        refid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int amount = 2
    private int amount_ = 0;
    public int amount {
        get { return amount_; }
        set { amount_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_amount() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_amount() {
        amount_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_refid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, refid );
        }
        if ( has_amount() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, amount );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                refid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                amount = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.LostUnitInfo" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        refid_ = 0;
        amount_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_refid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( refid );
        }
        if ( has_amount() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( amount );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "refid: " );
        if ( has_refid() ) 
            Console.WriteLine( refid );
        Console.Write( "amount: " );
        if ( has_amount() ) 
            Console.WriteLine( amount );
    }

}
}

public partial class proto {
public partial class MedalReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int medal_give = 3
    private int medal_give_ = 0;
    public int medal_give {
        get { return medal_give_; }
        set { medal_give_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_medal_give() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_medal_give() {
        medal_give_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_medal_give() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, medal_give );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                medal_give = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.MedalReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        medal_give_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_medal_give() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( medal_give );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "medal_give: " );
        if ( has_medal_give() ) 
            Console.WriteLine( medal_give );
    }

}
}

public partial class proto {
public partial class MessageHead: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int pid = 1
    private int pid_ = 0;
    public int pid {
        get { return pid_; }
        set { pid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_pid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_pid() {
        pid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int mid = 2
    private int mid_ = 0;
    public int mid {
        get { return mid_; }
        set { mid_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_mid() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_mid() {
        mid_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int reply_mid = 3
    private int reply_mid_ = 0;
    public int reply_mid {
        get { return reply_mid_; }
        set { reply_mid_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_reply_mid() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_reply_mid() {
        reply_mid_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int session_id = 4
    private int session_id_ = 0;
    public int session_id {
        get { return session_id_; }
        set { session_id_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_session_id() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_session_id() {
        session_id_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int gid = 5
    private int gid_ = 0;
    public int gid {
        get { return gid_; }
        set { gid_ = value; 
              has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_gid() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_gid() {
        gid_ = 0;
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int msgid = 6
    private int msgid_ = 0;
    public int msgid {
        get { return msgid_; }
        set { msgid_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_msgid() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_msgid() {
        msgid_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_pid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, pid );
        }
        if ( has_mid() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, mid );
        }
        if ( has_reply_mid() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, reply_mid );
        }
        if ( has_session_id() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, session_id );
        }
        if ( has_gid() ) {
            __index += WriteInt( buf, __index, 40 );
            __index += WriteInt( buf, __index, gid );
        }
        if ( has_msgid() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, msgid );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                pid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                mid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 24:
                reply_mid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                session_id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 40:
                gid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 48:
                msgid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.MessageHead" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        pid_ = 0;
        mid_ = 0;
        reply_mid_ = 0;
        session_id_ = 0;
        gid_ = 0;
        msgid_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_pid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( pid );
        }
        if ( has_mid() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( mid );
        }
        if ( has_reply_mid() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( reply_mid );
        }
        if ( has_session_id() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( session_id );
        }
        if ( has_gid() ) {
            cached_byte_size += WriteIntSize( 40 );
            cached_byte_size += WriteIntSize( gid );
        }
        if ( has_msgid() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( msgid );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "pid: " );
        if ( has_pid() ) 
            Console.WriteLine( pid );
        Console.Write( "mid: " );
        if ( has_mid() ) 
            Console.WriteLine( mid );
        Console.Write( "reply_mid: " );
        if ( has_reply_mid() ) 
            Console.WriteLine( reply_mid );
        Console.Write( "session_id: " );
        if ( has_session_id() ) 
            Console.WriteLine( session_id );
        Console.Write( "gid: " );
        if ( has_gid() ) 
            Console.WriteLine( gid );
        Console.Write( "msgid: " );
        if ( has_msgid() ) 
            Console.WriteLine( msgid );
    }

}
}

public partial class proto {
public partial class PartFireEvent: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int partid = 1
    private int partid_ = 0;
    public int partid {
        get { return partid_; }
        set { partid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_partid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_partid() {
        partid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field proto.PartFireInfo fireinfo = 2
    private List<proto.PartFireInfo> fireinfo_ = null;
    public int fireinfo_size() { return fireinfo_ == null ? 0 : fireinfo_.Count; }
    public proto.PartFireInfo fireinfo( int index ) { return fireinfo_[index]; }
    public void add_fireinfo( proto.PartFireInfo val ) { 
        if ( fireinfo_ == null ) fireinfo_ = new List<proto.PartFireInfo>();
        fireinfo_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_fireinfo() { 
        if ( fireinfo_ != null ) fireinfo_.Clear();
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        int list_count = 0;
        if ( has_partid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, partid );
        }
        list_count = fireinfo_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 18 );
            int size = fireinfo( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            fireinfo( i ).Serialize( buf, __index ); __index += size;
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                partid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                int fireinfo_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( fireinfo_size > msg_end-__index ) return false;
                proto.PartFireInfo fireinfo_tmp = new proto.PartFireInfo();
                add_fireinfo( fireinfo_tmp );
                if ( fireinfo_size > 0 ) {
                    if ( !fireinfo_tmp.Parse( buf, __index, fireinfo_size ) ) return false;
                    read_len = fireinfo_size;
                } else read_len = 0;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.PartFireEvent" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        partid_ = 0;
        if ( fireinfo_ != null ) fireinfo_.Clear();
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        int list_count = 0;
        if ( has_partid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( partid );
        }
        list_count = fireinfo_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = fireinfo( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        int list_count = 0;
        Console.Write( "partid: " );
        if ( has_partid() ) 
            Console.WriteLine( partid );
        Console.Write( "fireinfo: " );
        list_count = fireinfo_size();
        for ( int i = 0; i < list_count; i++ )
            fireinfo( i ).Print();
        Console.WriteLine();
    }

}
}

public partial class proto {
public partial class PartFireInfo: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int targetid = 1
    private int targetid_ = 0;
    public int targetid {
        get { return targetid_; }
        set { targetid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_targetid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_targetid() {
        targetid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int delayframe = 2
    private int delayframe_ = 0;
    public int delayframe {
        get { return delayframe_; }
        set { delayframe_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_delayframe() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_delayframe() {
        delayframe_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int val = 3
    private int val_ = 0;
    public int val {
        get { return val_; }
        set { val_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_val() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_val() {
        val_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_targetid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, targetid );
        }
        if ( has_delayframe() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, delayframe );
        }
        if ( has_val() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, val );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                targetid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                delayframe = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 24:
                val = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.PartFireInfo" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        targetid_ = 0;
        delayframe_ = 0;
        val_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_targetid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( targetid );
        }
        if ( has_delayframe() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( delayframe );
        }
        if ( has_val() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( val );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "targetid: " );
        if ( has_targetid() ) 
            Console.WriteLine( targetid );
        Console.Write( "delayframe: " );
        if ( has_delayframe() ) 
            Console.WriteLine( delayframe );
        Console.Write( "val: " );
        if ( has_val() ) 
            Console.WriteLine( val );
    }

}
}

public partial class proto {
public partial class PartReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;
    private uint has_flag_1;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int lv_max = 3
    private int lv_max_ = 0;
    public int lv_max {
        get { return lv_max_; }
        set { lv_max_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_lv_max() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_lv_max() {
        lv_max_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int target_type = 4
    private int target_type_ = 0;
    public int target_type {
        get { return target_type_; }
        set { target_type_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_target_type() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_target_type() {
        target_type_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int advantage_unitstrait_type = 5
    private int advantage_unitstrait_type_ = 0;
    public int advantage_unitstrait_type {
        get { return advantage_unitstrait_type_; }
        set { advantage_unitstrait_type_ = value; 
              has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_advantage_unitstrait_type() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_advantage_unitstrait_type() {
        advantage_unitstrait_type_ = 0;
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int disadvantage_unitstrait_type = 6
    private int disadvantage_unitstrait_type_ = 0;
    public int disadvantage_unitstrait_type {
        get { return disadvantage_unitstrait_type_; }
        set { disadvantage_unitstrait_type_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_disadvantage_unitstrait_type() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_disadvantage_unitstrait_type() {
        disadvantage_unitstrait_type_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field int ignore_unitstrait = 7
    private int ignore_unitstrait_ = 0;
    public int ignore_unitstrait {
        get { return ignore_unitstrait_; }
        set { ignore_unitstrait_ = value; 
              has_flag_0 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_ignore_unitstrait() {
        return ( has_flag_0 & 0x40 ) != 0;
    }
    public void clear_ignore_unitstrait() {
        ignore_unitstrait_ = 0;
        has_flag_0 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    //field int priority_unitstrait = 8
    private int priority_unitstrait_ = 0;
    public int priority_unitstrait {
        get { return priority_unitstrait_; }
        set { priority_unitstrait_ = value; 
              has_flag_0 |= 0x80;
              cached_byte_size = 0; }
    }
    public bool has_priority_unitstrait() {
        return ( has_flag_0 & 0x80 ) != 0;
    }
    public void clear_priority_unitstrait() {
        priority_unitstrait_ = 0;
        has_flag_0 &= 0xffffff7f;
        cached_byte_size = 0;
    }

    //field int priority_unitstrait_extend = 9
    private int priority_unitstrait_extend_ = 0;
    public int priority_unitstrait_extend {
        get { return priority_unitstrait_extend_; }
        set { priority_unitstrait_extend_ = value; 
              has_flag_0 |= 0x100;
              cached_byte_size = 0; }
    }
    public bool has_priority_unitstrait_extend() {
        return ( has_flag_0 & 0x100 ) != 0;
    }
    public void clear_priority_unitstrait_extend() {
        priority_unitstrait_extend_ = 0;
        has_flag_0 &= 0xfffffeff;
        cached_byte_size = 0;
    }

    //field int cast_cond = 10
    private int cast_cond_ = 0;
    public int cast_cond {
        get { return cast_cond_; }
        set { cast_cond_ = value; 
              has_flag_0 |= 0x200;
              cached_byte_size = 0; }
    }
    public bool has_cast_cond() {
        return ( has_flag_0 & 0x200 ) != 0;
    }
    public void clear_cast_cond() {
        cast_cond_ = 0;
        has_flag_0 &= 0xfffffdff;
        cached_byte_size = 0;
    }

    //field int cd = 11
    private int cd_ = 0;
    public int cd {
        get { return cd_; }
        set { cd_ = value; 
              has_flag_0 |= 0x400;
              cached_byte_size = 0; }
    }
    public bool has_cd() {
        return ( has_flag_0 & 0x400 ) != 0;
    }
    public void clear_cd() {
        cd_ = 0;
        has_flag_0 &= 0xfffffbff;
        cached_byte_size = 0;
    }

    //field int effect_type = 12
    private int effect_type_ = 0;
    public int effect_type {
        get { return effect_type_; }
        set { effect_type_ = value; 
              has_flag_0 |= 0x800;
              cached_byte_size = 0; }
    }
    public bool has_effect_type() {
        return ( has_flag_0 & 0x800 ) != 0;
    }
    public void clear_effect_type() {
        effect_type_ = 0;
        has_flag_0 &= 0xfffff7ff;
        cached_byte_size = 0;
    }

    //field int passive_effect_type = 13
    private int passive_effect_type_ = 0;
    public int passive_effect_type {
        get { return passive_effect_type_; }
        set { passive_effect_type_ = value; 
              has_flag_0 |= 0x1000;
              cached_byte_size = 0; }
    }
    public bool has_passive_effect_type() {
        return ( has_flag_0 & 0x1000 ) != 0;
    }
    public void clear_passive_effect_type() {
        passive_effect_type_ = 0;
        has_flag_0 &= 0xffffefff;
        cached_byte_size = 0;
    }

    //field int effect_val = 14
    private int effect_val_ = 0;
    public int effect_val {
        get { return effect_val_; }
        set { effect_val_ = value; 
              has_flag_0 |= 0x2000;
              cached_byte_size = 0; }
    }
    public bool has_effect_val() {
        return ( has_flag_0 & 0x2000 ) != 0;
    }
    public void clear_effect_val() {
        effect_val_ = 0;
        has_flag_0 &= 0xffffdfff;
        cached_byte_size = 0;
    }

    //field int effect_val_growthrate = 15
    private int effect_val_growthrate_ = 0;
    public int effect_val_growthrate {
        get { return effect_val_growthrate_; }
        set { effect_val_growthrate_ = value; 
              has_flag_0 |= 0x4000;
              cached_byte_size = 0; }
    }
    public bool has_effect_val_growthrate() {
        return ( has_flag_0 & 0x4000 ) != 0;
    }
    public void clear_effect_val_growthrate() {
        effect_val_growthrate_ = 0;
        has_flag_0 &= 0xffffbfff;
        cached_byte_size = 0;
    }

    //field int give_buff = 16
    private int give_buff_ = 0;
    public int give_buff {
        get { return give_buff_; }
        set { give_buff_ = value; 
              has_flag_0 |= 0x8000;
              cached_byte_size = 0; }
    }
    public bool has_give_buff() {
        return ( has_flag_0 & 0x8000 ) != 0;
    }
    public void clear_give_buff() {
        give_buff_ = 0;
        has_flag_0 &= 0xffff7fff;
        cached_byte_size = 0;
    }

    //field int skill_attach = 17
    private int skill_attach_ = 0;
    public int skill_attach {
        get { return skill_attach_; }
        set { skill_attach_ = value; 
              has_flag_0 |= 0x10000;
              cached_byte_size = 0; }
    }
    public bool has_skill_attach() {
        return ( has_flag_0 & 0x10000 ) != 0;
    }
    public void clear_skill_attach() {
        skill_attach_ = 0;
        has_flag_0 &= 0xfffeffff;
        cached_byte_size = 0;
    }

    //field int ammo_num = 18
    private int ammo_num_ = 0;
    public int ammo_num {
        get { return ammo_num_; }
        set { ammo_num_ = value; 
              has_flag_0 |= 0x20000;
              cached_byte_size = 0; }
    }
    public bool has_ammo_num() {
        return ( has_flag_0 & 0x20000 ) != 0;
    }
    public void clear_ammo_num() {
        ammo_num_ = 0;
        has_flag_0 &= 0xfffdffff;
        cached_byte_size = 0;
    }

    //field int energy_cost = 19
    private int energy_cost_ = 0;
    public int energy_cost {
        get { return energy_cost_; }
        set { energy_cost_ = value; 
              has_flag_0 |= 0x40000;
              cached_byte_size = 0; }
    }
    public bool has_energy_cost() {
        return ( has_flag_0 & 0x40000 ) != 0;
    }
    public void clear_energy_cost() {
        energy_cost_ = 0;
        has_flag_0 &= 0xfffbffff;
        cached_byte_size = 0;
    }

    //field int atk_range_max = 20
    private int atk_range_max_ = 0;
    public int atk_range_max {
        get { return atk_range_max_; }
        set { atk_range_max_ = value; 
              has_flag_0 |= 0x80000;
              cached_byte_size = 0; }
    }
    public bool has_atk_range_max() {
        return ( has_flag_0 & 0x80000 ) != 0;
    }
    public void clear_atk_range_max() {
        atk_range_max_ = 0;
        has_flag_0 &= 0xfff7ffff;
        cached_byte_size = 0;
    }

    //field int atk_range_min = 21
    private int atk_range_min_ = 0;
    public int atk_range_min {
        get { return atk_range_min_; }
        set { atk_range_min_ = value; 
              has_flag_0 |= 0x100000;
              cached_byte_size = 0; }
    }
    public bool has_atk_range_min() {
        return ( has_flag_0 & 0x100000 ) != 0;
    }
    public void clear_atk_range_min() {
        atk_range_min_ = 0;
        has_flag_0 &= 0xffefffff;
        cached_byte_size = 0;
    }

    //field int atk_angle = 22
    private int atk_angle_ = 0;
    public int atk_angle {
        get { return atk_angle_; }
        set { atk_angle_ = value; 
              has_flag_0 |= 0x200000;
              cached_byte_size = 0; }
    }
    public bool has_atk_angle() {
        return ( has_flag_0 & 0x200000 ) != 0;
    }
    public void clear_atk_angle() {
        atk_angle_ = 0;
        has_flag_0 &= 0xffdfffff;
        cached_byte_size = 0;
    }

    //field int attack_move_range = 23
    private int attack_move_range_ = 0;
    public int attack_move_range {
        get { return attack_move_range_; }
        set { attack_move_range_ = value; 
              has_flag_0 |= 0x400000;
              cached_byte_size = 0; }
    }
    public bool has_attack_move_range() {
        return ( has_flag_0 & 0x400000 ) != 0;
    }
    public void clear_attack_move_range() {
        attack_move_range_ = 0;
        has_flag_0 &= 0xffbfffff;
        cached_byte_size = 0;
    }

    //field int closein_range = 24
    private int closein_range_ = 0;
    public int closein_range {
        get { return closein_range_; }
        set { closein_range_ = value; 
              has_flag_0 |= 0x800000;
              cached_byte_size = 0; }
    }
    public bool has_closein_range() {
        return ( has_flag_0 & 0x800000 ) != 0;
    }
    public void clear_closein_range() {
        closein_range_ = 0;
        has_flag_0 &= 0xff7fffff;
        cached_byte_size = 0;
    }

    //field int shape_type = 25
    private int shape_type_ = 0;
    public int shape_type {
        get { return shape_type_; }
        set { shape_type_ = value; 
              has_flag_0 |= 0x1000000;
              cached_byte_size = 0; }
    }
    public bool has_shape_type() {
        return ( has_flag_0 & 0x1000000 ) != 0;
    }
    public void clear_shape_type() {
        shape_type_ = 0;
        has_flag_0 &= 0xfeffffff;
        cached_byte_size = 0;
    }

    //field int aoe_range = 26
    private int aoe_range_ = 0;
    public int aoe_range {
        get { return aoe_range_; }
        set { aoe_range_ = value; 
              has_flag_0 |= 0x2000000;
              cached_byte_size = 0; }
    }
    public bool has_aoe_range() {
        return ( has_flag_0 & 0x2000000 ) != 0;
    }
    public void clear_aoe_range() {
        aoe_range_ = 0;
        has_flag_0 &= 0xfdffffff;
        cached_byte_size = 0;
    }

    //field int radiate_len = 27
    private int radiate_len_ = 0;
    public int radiate_len {
        get { return radiate_len_; }
        set { radiate_len_ = value; 
              has_flag_0 |= 0x4000000;
              cached_byte_size = 0; }
    }
    public bool has_radiate_len() {
        return ( has_flag_0 & 0x4000000 ) != 0;
    }
    public void clear_radiate_len() {
        radiate_len_ = 0;
        has_flag_0 &= 0xfbffffff;
        cached_byte_size = 0;
    }

    //field int radiate_wid = 28
    private int radiate_wid_ = 0;
    public int radiate_wid {
        get { return radiate_wid_; }
        set { radiate_wid_ = value; 
              has_flag_0 |= 0x8000000;
              cached_byte_size = 0; }
    }
    public bool has_radiate_wid() {
        return ( has_flag_0 & 0x8000000 ) != 0;
    }
    public void clear_radiate_wid() {
        radiate_wid_ = 0;
        has_flag_0 &= 0xf7ffffff;
        cached_byte_size = 0;
    }

    //field int missle_vel = 29
    private int missle_vel_ = 0;
    public int missle_vel {
        get { return missle_vel_; }
        set { missle_vel_ = value; 
              has_flag_0 |= 0x10000000;
              cached_byte_size = 0; }
    }
    public bool has_missle_vel() {
        return ( has_flag_0 & 0x10000000 ) != 0;
    }
    public void clear_missle_vel() {
        missle_vel_ = 0;
        has_flag_0 &= 0xefffffff;
        cached_byte_size = 0;
    }

    //field string cartoon_res = 30
    private string cartoon_res_ = "";
    public string cartoon_res {
        get { return cartoon_res_; }
        set { cartoon_res_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xdfffffff;
              else
                  has_flag_0 |= 0x20000000;
              cached_byte_size = 0; }
    }
    public bool has_cartoon_res() {
        return ( has_flag_0 & 0x20000000 ) != 0;
    }
    public void clear_cartoon_res() {
        cartoon_res_ = "";
        has_flag_0 &= 0xdfffffff;
        cached_byte_size = 0;
    }

    //field string iconfile = 31
    private string iconfile_ = "";
    public string iconfile {
        get { return iconfile_; }
        set { iconfile_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xbfffffff;
              else
                  has_flag_0 |= 0x40000000;
              cached_byte_size = 0; }
    }
    public bool has_iconfile() {
        return ( has_flag_0 & 0x40000000 ) != 0;
    }
    public void clear_iconfile() {
        iconfile_ = "";
        has_flag_0 &= 0xbfffffff;
        cached_byte_size = 0;
    }

    //field string note = 32
    private string note_ = "";
    public string note {
        get { return note_; }
        set { note_ = value; 
              if ( value == null )
                  has_flag_0 &= 0x7fffffff;
              else
                  has_flag_0 |= 0x80000000;
              cached_byte_size = 0; }
    }
    public bool has_note() {
        return ( has_flag_0 & 0x80000000 ) != 0;
    }
    public void clear_note() {
        note_ = "";
        has_flag_0 &= 0x7fffffff;
        cached_byte_size = 0;
    }

    //field int continued_time = 33
    private int continued_time_ = 0;
    public int continued_time {
        get { return continued_time_; }
        set { continued_time_ = value; 
              has_flag_1 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_continued_time() {
        return ( has_flag_1 & 0x1 ) != 0;
    }
    public void clear_continued_time() {
        continued_time_ = 0;
        has_flag_1 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int continuity_times = 34
    private int continuity_times_ = 0;
    public int continuity_times {
        get { return continuity_times_; }
        set { continuity_times_ = value; 
              has_flag_1 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_continuity_times() {
        return ( has_flag_1 & 0x2 ) != 0;
    }
    public void clear_continuity_times() {
        continuity_times_ = 0;
        has_flag_1 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int continuity_interval = 35
    private int continuity_interval_ = 0;
    public int continuity_interval {
        get { return continuity_interval_; }
        set { continuity_interval_ = value; 
              has_flag_1 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_continuity_interval() {
        return ( has_flag_1 & 0x4 ) != 0;
    }
    public void clear_continuity_interval() {
        continuity_interval_ = 0;
        has_flag_1 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int ac_initial = 36
    private int ac_initial_ = 0;
    public int ac_initial {
        get { return ac_initial_; }
        set { ac_initial_ = value; 
              has_flag_1 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_ac_initial() {
        return ( has_flag_1 & 0x8 ) != 0;
    }
    public void clear_ac_initial() {
        ac_initial_ = 0;
        has_flag_1 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_lv_max() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, lv_max );
        }
        if ( has_target_type() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, target_type );
        }
        if ( has_advantage_unitstrait_type() ) {
            __index += WriteInt( buf, __index, 40 );
            __index += WriteInt( buf, __index, advantage_unitstrait_type );
        }
        if ( has_disadvantage_unitstrait_type() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, disadvantage_unitstrait_type );
        }
        if ( has_ignore_unitstrait() ) {
            __index += WriteInt( buf, __index, 56 );
            __index += WriteInt( buf, __index, ignore_unitstrait );
        }
        if ( has_priority_unitstrait() ) {
            __index += WriteInt( buf, __index, 64 );
            __index += WriteInt( buf, __index, priority_unitstrait );
        }
        if ( has_priority_unitstrait_extend() ) {
            __index += WriteInt( buf, __index, 72 );
            __index += WriteInt( buf, __index, priority_unitstrait_extend );
        }
        if ( has_cast_cond() ) {
            __index += WriteInt( buf, __index, 80 );
            __index += WriteInt( buf, __index, cast_cond );
        }
        if ( has_cd() ) {
            __index += WriteInt( buf, __index, 88 );
            __index += WriteInt( buf, __index, cd );
        }
        if ( has_effect_type() ) {
            __index += WriteInt( buf, __index, 96 );
            __index += WriteInt( buf, __index, effect_type );
        }
        if ( has_passive_effect_type() ) {
            __index += WriteInt( buf, __index, 104 );
            __index += WriteInt( buf, __index, passive_effect_type );
        }
        if ( has_effect_val() ) {
            __index += WriteInt( buf, __index, 112 );
            __index += WriteInt( buf, __index, effect_val );
        }
        if ( has_effect_val_growthrate() ) {
            __index += WriteInt( buf, __index, 120 );
            __index += WriteInt( buf, __index, effect_val_growthrate );
        }
        if ( has_give_buff() ) {
            __index += WriteInt( buf, __index, 128 );
            __index += WriteInt( buf, __index, give_buff );
        }
        if ( has_skill_attach() ) {
            __index += WriteInt( buf, __index, 136 );
            __index += WriteInt( buf, __index, skill_attach );
        }
        if ( has_ammo_num() ) {
            __index += WriteInt( buf, __index, 144 );
            __index += WriteInt( buf, __index, ammo_num );
        }
        if ( has_energy_cost() ) {
            __index += WriteInt( buf, __index, 152 );
            __index += WriteInt( buf, __index, energy_cost );
        }
        if ( has_atk_range_max() ) {
            __index += WriteInt( buf, __index, 160 );
            __index += WriteInt( buf, __index, atk_range_max );
        }
        if ( has_atk_range_min() ) {
            __index += WriteInt( buf, __index, 168 );
            __index += WriteInt( buf, __index, atk_range_min );
        }
        if ( has_atk_angle() ) {
            __index += WriteInt( buf, __index, 176 );
            __index += WriteInt( buf, __index, atk_angle );
        }
        if ( has_attack_move_range() ) {
            __index += WriteInt( buf, __index, 184 );
            __index += WriteInt( buf, __index, attack_move_range );
        }
        if ( has_closein_range() ) {
            __index += WriteInt( buf, __index, 192 );
            __index += WriteInt( buf, __index, closein_range );
        }
        if ( has_shape_type() ) {
            __index += WriteInt( buf, __index, 200 );
            __index += WriteInt( buf, __index, shape_type );
        }
        if ( has_aoe_range() ) {
            __index += WriteInt( buf, __index, 208 );
            __index += WriteInt( buf, __index, aoe_range );
        }
        if ( has_radiate_len() ) {
            __index += WriteInt( buf, __index, 216 );
            __index += WriteInt( buf, __index, radiate_len );
        }
        if ( has_radiate_wid() ) {
            __index += WriteInt( buf, __index, 224 );
            __index += WriteInt( buf, __index, radiate_wid );
        }
        if ( has_missle_vel() ) {
            __index += WriteInt( buf, __index, 232 );
            __index += WriteInt( buf, __index, missle_vel );
        }
        if ( has_cartoon_res() ) {
            __index += WriteInt( buf, __index, 242 );
            __index += WriteString( buf, buf_size-__index, __index, cartoon_res );
        }
        if ( has_iconfile() ) {
            __index += WriteInt( buf, __index, 250 );
            __index += WriteString( buf, buf_size-__index, __index, iconfile );
        }
        if ( has_note() ) {
            __index += WriteInt( buf, __index, 258 );
            __index += WriteString( buf, buf_size-__index, __index, note );
        }
        if ( has_continued_time() ) {
            __index += WriteInt( buf, __index, 264 );
            __index += WriteInt( buf, __index, continued_time );
        }
        if ( has_continuity_times() ) {
            __index += WriteInt( buf, __index, 272 );
            __index += WriteInt( buf, __index, continuity_times );
        }
        if ( has_continuity_interval() ) {
            __index += WriteInt( buf, __index, 280 );
            __index += WriteInt( buf, __index, continuity_interval );
        }
        if ( has_ac_initial() ) {
            __index += WriteInt( buf, __index, 288 );
            __index += WriteInt( buf, __index, ac_initial );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                lv_max = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                target_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 40:
                advantage_unitstrait_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 48:
                disadvantage_unitstrait_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 56:
                ignore_unitstrait = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 64:
                priority_unitstrait = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 72:
                priority_unitstrait_extend = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 80:
                cast_cond = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 88:
                cd = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 96:
                effect_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 104:
                passive_effect_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 112:
                effect_val = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 120:
                effect_val_growthrate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 128:
                give_buff = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 136:
                skill_attach = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 144:
                ammo_num = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 152:
                energy_cost = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 160:
                atk_range_max = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 168:
                atk_range_min = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 176:
                atk_angle = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 184:
                attack_move_range = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 192:
                closein_range = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 200:
                shape_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 208:
                aoe_range = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 216:
                radiate_len = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 224:
                radiate_wid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 232:
                missle_vel = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 242:
                cartoon_res = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( cartoon_res == null ) return false;
                __index += read_len;
                break;
            case 250:
                iconfile = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( iconfile == null ) return false;
                __index += read_len;
                break;
            case 258:
                note = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( note == null ) return false;
                __index += read_len;
                break;
            case 264:
                continued_time = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 272:
                continuity_times = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 280:
                continuity_interval = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 288:
                ac_initial = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.PartReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        lv_max_ = 0;
        target_type_ = 0;
        advantage_unitstrait_type_ = 0;
        disadvantage_unitstrait_type_ = 0;
        ignore_unitstrait_ = 0;
        priority_unitstrait_ = 0;
        priority_unitstrait_extend_ = 0;
        cast_cond_ = 0;
        cd_ = 0;
        effect_type_ = 0;
        passive_effect_type_ = 0;
        effect_val_ = 0;
        effect_val_growthrate_ = 0;
        give_buff_ = 0;
        skill_attach_ = 0;
        ammo_num_ = 0;
        energy_cost_ = 0;
        atk_range_max_ = 0;
        atk_range_min_ = 0;
        atk_angle_ = 0;
        attack_move_range_ = 0;
        closein_range_ = 0;
        shape_type_ = 0;
        aoe_range_ = 0;
        radiate_len_ = 0;
        radiate_wid_ = 0;
        missle_vel_ = 0;
        cartoon_res_ = "";
        iconfile_ = "";
        note_ = "";
        continued_time_ = 0;
        continuity_times_ = 0;
        continuity_interval_ = 0;
        ac_initial_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
        has_flag_1 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_lv_max() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( lv_max );
        }
        if ( has_target_type() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( target_type );
        }
        if ( has_advantage_unitstrait_type() ) {
            cached_byte_size += WriteIntSize( 40 );
            cached_byte_size += WriteIntSize( advantage_unitstrait_type );
        }
        if ( has_disadvantage_unitstrait_type() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( disadvantage_unitstrait_type );
        }
        if ( has_ignore_unitstrait() ) {
            cached_byte_size += WriteIntSize( 56 );
            cached_byte_size += WriteIntSize( ignore_unitstrait );
        }
        if ( has_priority_unitstrait() ) {
            cached_byte_size += WriteIntSize( 64 );
            cached_byte_size += WriteIntSize( priority_unitstrait );
        }
        if ( has_priority_unitstrait_extend() ) {
            cached_byte_size += WriteIntSize( 72 );
            cached_byte_size += WriteIntSize( priority_unitstrait_extend );
        }
        if ( has_cast_cond() ) {
            cached_byte_size += WriteIntSize( 80 );
            cached_byte_size += WriteIntSize( cast_cond );
        }
        if ( has_cd() ) {
            cached_byte_size += WriteIntSize( 88 );
            cached_byte_size += WriteIntSize( cd );
        }
        if ( has_effect_type() ) {
            cached_byte_size += WriteIntSize( 96 );
            cached_byte_size += WriteIntSize( effect_type );
        }
        if ( has_passive_effect_type() ) {
            cached_byte_size += WriteIntSize( 104 );
            cached_byte_size += WriteIntSize( passive_effect_type );
        }
        if ( has_effect_val() ) {
            cached_byte_size += WriteIntSize( 112 );
            cached_byte_size += WriteIntSize( effect_val );
        }
        if ( has_effect_val_growthrate() ) {
            cached_byte_size += WriteIntSize( 120 );
            cached_byte_size += WriteIntSize( effect_val_growthrate );
        }
        if ( has_give_buff() ) {
            cached_byte_size += WriteIntSize( 128 );
            cached_byte_size += WriteIntSize( give_buff );
        }
        if ( has_skill_attach() ) {
            cached_byte_size += WriteIntSize( 136 );
            cached_byte_size += WriteIntSize( skill_attach );
        }
        if ( has_ammo_num() ) {
            cached_byte_size += WriteIntSize( 144 );
            cached_byte_size += WriteIntSize( ammo_num );
        }
        if ( has_energy_cost() ) {
            cached_byte_size += WriteIntSize( 152 );
            cached_byte_size += WriteIntSize( energy_cost );
        }
        if ( has_atk_range_max() ) {
            cached_byte_size += WriteIntSize( 160 );
            cached_byte_size += WriteIntSize( atk_range_max );
        }
        if ( has_atk_range_min() ) {
            cached_byte_size += WriteIntSize( 168 );
            cached_byte_size += WriteIntSize( atk_range_min );
        }
        if ( has_atk_angle() ) {
            cached_byte_size += WriteIntSize( 176 );
            cached_byte_size += WriteIntSize( atk_angle );
        }
        if ( has_attack_move_range() ) {
            cached_byte_size += WriteIntSize( 184 );
            cached_byte_size += WriteIntSize( attack_move_range );
        }
        if ( has_closein_range() ) {
            cached_byte_size += WriteIntSize( 192 );
            cached_byte_size += WriteIntSize( closein_range );
        }
        if ( has_shape_type() ) {
            cached_byte_size += WriteIntSize( 200 );
            cached_byte_size += WriteIntSize( shape_type );
        }
        if ( has_aoe_range() ) {
            cached_byte_size += WriteIntSize( 208 );
            cached_byte_size += WriteIntSize( aoe_range );
        }
        if ( has_radiate_len() ) {
            cached_byte_size += WriteIntSize( 216 );
            cached_byte_size += WriteIntSize( radiate_len );
        }
        if ( has_radiate_wid() ) {
            cached_byte_size += WriteIntSize( 224 );
            cached_byte_size += WriteIntSize( radiate_wid );
        }
        if ( has_missle_vel() ) {
            cached_byte_size += WriteIntSize( 232 );
            cached_byte_size += WriteIntSize( missle_vel );
        }
        if ( has_cartoon_res() ) {
            cached_byte_size += WriteIntSize( 242 );
            int size = WriteStringSize( cartoon_res );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_iconfile() ) {
            cached_byte_size += WriteIntSize( 250 );
            int size = WriteStringSize( iconfile );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_note() ) {
            cached_byte_size += WriteIntSize( 258 );
            int size = WriteStringSize( note );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_continued_time() ) {
            cached_byte_size += WriteIntSize( 264 );
            cached_byte_size += WriteIntSize( continued_time );
        }
        if ( has_continuity_times() ) {
            cached_byte_size += WriteIntSize( 272 );
            cached_byte_size += WriteIntSize( continuity_times );
        }
        if ( has_continuity_interval() ) {
            cached_byte_size += WriteIntSize( 280 );
            cached_byte_size += WriteIntSize( continuity_interval );
        }
        if ( has_ac_initial() ) {
            cached_byte_size += WriteIntSize( 288 );
            cached_byte_size += WriteIntSize( ac_initial );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "lv_max: " );
        if ( has_lv_max() ) 
            Console.WriteLine( lv_max );
        Console.Write( "target_type: " );
        if ( has_target_type() ) 
            Console.WriteLine( target_type );
        Console.Write( "advantage_unitstrait_type: " );
        if ( has_advantage_unitstrait_type() ) 
            Console.WriteLine( advantage_unitstrait_type );
        Console.Write( "disadvantage_unitstrait_type: " );
        if ( has_disadvantage_unitstrait_type() ) 
            Console.WriteLine( disadvantage_unitstrait_type );
        Console.Write( "ignore_unitstrait: " );
        if ( has_ignore_unitstrait() ) 
            Console.WriteLine( ignore_unitstrait );
        Console.Write( "priority_unitstrait: " );
        if ( has_priority_unitstrait() ) 
            Console.WriteLine( priority_unitstrait );
        Console.Write( "priority_unitstrait_extend: " );
        if ( has_priority_unitstrait_extend() ) 
            Console.WriteLine( priority_unitstrait_extend );
        Console.Write( "cast_cond: " );
        if ( has_cast_cond() ) 
            Console.WriteLine( cast_cond );
        Console.Write( "cd: " );
        if ( has_cd() ) 
            Console.WriteLine( cd );
        Console.Write( "effect_type: " );
        if ( has_effect_type() ) 
            Console.WriteLine( effect_type );
        Console.Write( "passive_effect_type: " );
        if ( has_passive_effect_type() ) 
            Console.WriteLine( passive_effect_type );
        Console.Write( "effect_val: " );
        if ( has_effect_val() ) 
            Console.WriteLine( effect_val );
        Console.Write( "effect_val_growthrate: " );
        if ( has_effect_val_growthrate() ) 
            Console.WriteLine( effect_val_growthrate );
        Console.Write( "give_buff: " );
        if ( has_give_buff() ) 
            Console.WriteLine( give_buff );
        Console.Write( "skill_attach: " );
        if ( has_skill_attach() ) 
            Console.WriteLine( skill_attach );
        Console.Write( "ammo_num: " );
        if ( has_ammo_num() ) 
            Console.WriteLine( ammo_num );
        Console.Write( "energy_cost: " );
        if ( has_energy_cost() ) 
            Console.WriteLine( energy_cost );
        Console.Write( "atk_range_max: " );
        if ( has_atk_range_max() ) 
            Console.WriteLine( atk_range_max );
        Console.Write( "atk_range_min: " );
        if ( has_atk_range_min() ) 
            Console.WriteLine( atk_range_min );
        Console.Write( "atk_angle: " );
        if ( has_atk_angle() ) 
            Console.WriteLine( atk_angle );
        Console.Write( "attack_move_range: " );
        if ( has_attack_move_range() ) 
            Console.WriteLine( attack_move_range );
        Console.Write( "closein_range: " );
        if ( has_closein_range() ) 
            Console.WriteLine( closein_range );
        Console.Write( "shape_type: " );
        if ( has_shape_type() ) 
            Console.WriteLine( shape_type );
        Console.Write( "aoe_range: " );
        if ( has_aoe_range() ) 
            Console.WriteLine( aoe_range );
        Console.Write( "radiate_len: " );
        if ( has_radiate_len() ) 
            Console.WriteLine( radiate_len );
        Console.Write( "radiate_wid: " );
        if ( has_radiate_wid() ) 
            Console.WriteLine( radiate_wid );
        Console.Write( "missle_vel: " );
        if ( has_missle_vel() ) 
            Console.WriteLine( missle_vel );
        Console.Write( "cartoon_res: " );
        if ( has_cartoon_res() ) 
            Console.WriteLine( cartoon_res );
        Console.Write( "iconfile: " );
        if ( has_iconfile() ) 
            Console.WriteLine( iconfile );
        Console.Write( "note: " );
        if ( has_note() ) 
            Console.WriteLine( note );
        Console.Write( "continued_time: " );
        if ( has_continued_time() ) 
            Console.WriteLine( continued_time );
        Console.Write( "continuity_times: " );
        if ( has_continuity_times() ) 
            Console.WriteLine( continuity_times );
        Console.Write( "continuity_interval: " );
        if ( has_continuity_interval() ) 
            Console.WriteLine( continuity_interval );
        Console.Write( "ac_initial: " );
        if ( has_ac_initial() ) 
            Console.WriteLine( ac_initial );
    }

}
}

public partial class proto {
public partial class Parts: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int level = 2
    private int level_ = 0;
    public int level {
        get { return level_; }
        set { level_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_level() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_level() {
        level_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_level() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, level );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                level = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.Parts" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        level_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_level() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( level );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "level: " );
        if ( has_level() ) 
            Console.WriteLine( level );
    }

}
}

public partial class proto {
public partial class PartsReplaceTeamReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.PartsReplaceTeamReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
    }

}
}

public partial class proto {
public partial class Player: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int sn = 1
    private int sn_ = 0;
    public int sn {
        get { return sn_; }
        set { sn_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_sn() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_sn() {
        sn_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int gold = 3
    private int gold_ = 0;
    public int gold {
        get { return gold_; }
        set { gold_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_gold() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_gold() {
        gold_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int coins = 4
    private int coins_ = 0;
    public int coins {
        get { return coins_; }
        set { coins_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_coins() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_coins() {
        coins_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int level = 5
    private int level_ = 0;
    public int level {
        get { return level_; }
        set { level_ = value; 
              has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_level() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_level() {
        level_ = 0;
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_sn() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, sn );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_gold() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, gold );
        }
        if ( has_coins() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, coins );
        }
        if ( has_level() ) {
            __index += WriteInt( buf, __index, 40 );
            __index += WriteInt( buf, __index, level );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                sn = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                gold = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                coins = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 40:
                level = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.Player" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        sn_ = 0;
        name_ = "";
        gold_ = 0;
        coins_ = 0;
        level_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_sn() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( sn );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_gold() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( gold );
        }
        if ( has_coins() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( coins );
        }
        if ( has_level() ) {
            cached_byte_size += WriteIntSize( 40 );
            cached_byte_size += WriteIntSize( level );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        return true;
    }

    public override void Print() {
        Console.Write( "sn: " );
        if ( has_sn() ) 
            Console.WriteLine( sn );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "gold: " );
        if ( has_gold() ) 
            Console.WriteLine( gold );
        Console.Write( "coins: " );
        if ( has_coins() ) 
            Console.WriteLine( coins );
        Console.Write( "level: " );
        if ( has_level() ) 
            Console.WriteLine( level );
    }

}
}

public partial class proto {
public partial class ReferenceManager: ProtoMessage {
    public int cached_byte_size;

    //field proto.UnitReference units_res = 1
    private List<proto.UnitReference> units_res_ = null;
    public int units_res_size() { return units_res_ == null ? 0 : units_res_.Count; }
    public proto.UnitReference units_res( int index ) { return units_res_[index]; }
    public void add_units_res( proto.UnitReference val ) { 
        if ( units_res_ == null ) units_res_ = new List<proto.UnitReference>();
        units_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_units_res() { 
        if ( units_res_ != null ) units_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.PartReference parts_res = 2
    private List<proto.PartReference> parts_res_ = null;
    public int parts_res_size() { return parts_res_ == null ? 0 : parts_res_.Count; }
    public proto.PartReference parts_res( int index ) { return parts_res_[index]; }
    public void add_parts_res( proto.PartReference val ) { 
        if ( parts_res_ == null ) parts_res_ = new List<proto.PartReference>();
        parts_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_parts_res() { 
        if ( parts_res_ != null ) parts_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.SkillReference skill_res = 3
    private List<proto.SkillReference> skill_res_ = null;
    public int skill_res_size() { return skill_res_ == null ? 0 : skill_res_.Count; }
    public proto.SkillReference skill_res( int index ) { return skill_res_[index]; }
    public void add_skill_res( proto.SkillReference val ) { 
        if ( skill_res_ == null ) skill_res_ = new List<proto.SkillReference>();
        skill_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_skill_res() { 
        if ( skill_res_ != null ) skill_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.ShuttleReference shuttle_res = 4
    private List<proto.ShuttleReference> shuttle_res_ = null;
    public int shuttle_res_size() { return shuttle_res_ == null ? 0 : shuttle_res_.Count; }
    public proto.ShuttleReference shuttle_res( int index ) { return shuttle_res_[index]; }
    public void add_shuttle_res( proto.ShuttleReference val ) { 
        if ( shuttle_res_ == null ) shuttle_res_ = new List<proto.ShuttleReference>();
        shuttle_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_shuttle_res() { 
        if ( shuttle_res_ != null ) shuttle_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.ShuttleTeamReference shuttle_team_res = 5
    private List<proto.ShuttleTeamReference> shuttle_team_res_ = null;
    public int shuttle_team_res_size() { return shuttle_team_res_ == null ? 0 : shuttle_team_res_.Count; }
    public proto.ShuttleTeamReference shuttle_team_res( int index ) { return shuttle_team_res_[index]; }
    public void add_shuttle_team_res( proto.ShuttleTeamReference val ) { 
        if ( shuttle_team_res_ == null ) shuttle_team_res_ = new List<proto.ShuttleTeamReference>();
        shuttle_team_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_shuttle_team_res() { 
        if ( shuttle_team_res_ != null ) shuttle_team_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.PartsReplaceTeamReference parts_replace_team_res = 6
    private List<proto.PartsReplaceTeamReference> parts_replace_team_res_ = null;
    public int parts_replace_team_res_size() { return parts_replace_team_res_ == null ? 0 : parts_replace_team_res_.Count; }
    public proto.PartsReplaceTeamReference parts_replace_team_res( int index ) { return parts_replace_team_res_[index]; }
    public void add_parts_replace_team_res( proto.PartsReplaceTeamReference val ) { 
        if ( parts_replace_team_res_ == null ) parts_replace_team_res_ = new List<proto.PartsReplaceTeamReference>();
        parts_replace_team_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_parts_replace_team_res() { 
        if ( parts_replace_team_res_ != null ) parts_replace_team_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.BuffReference buff_res = 7
    private List<proto.BuffReference> buff_res_ = null;
    public int buff_res_size() { return buff_res_ == null ? 0 : buff_res_.Count; }
    public proto.BuffReference buff_res( int index ) { return buff_res_[index]; }
    public void add_buff_res( proto.BuffReference val ) { 
        if ( buff_res_ == null ) buff_res_ = new List<proto.BuffReference>();
        buff_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_buff_res() { 
        if ( buff_res_ != null ) buff_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.DefensiveUnitsReference defensive_units_res = 8
    private List<proto.DefensiveUnitsReference> defensive_units_res_ = null;
    public int defensive_units_res_size() { return defensive_units_res_ == null ? 0 : defensive_units_res_.Count; }
    public proto.DefensiveUnitsReference defensive_units_res( int index ) { return defensive_units_res_[index]; }
    public void add_defensive_units_res( proto.DefensiveUnitsReference val ) { 
        if ( defensive_units_res_ == null ) defensive_units_res_ = new List<proto.DefensiveUnitsReference>();
        defensive_units_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_defensive_units_res() { 
        if ( defensive_units_res_ != null ) defensive_units_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.BattlefieldReference battlefield_res = 9
    private List<proto.BattlefieldReference> battlefield_res_ = null;
    public int battlefield_res_size() { return battlefield_res_ == null ? 0 : battlefield_res_.Count; }
    public proto.BattlefieldReference battlefield_res( int index ) { return battlefield_res_[index]; }
    public void add_battlefield_res( proto.BattlefieldReference val ) { 
        if ( battlefield_res_ == null ) battlefield_res_ = new List<proto.BattlefieldReference>();
        battlefield_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_battlefield_res() { 
        if ( battlefield_res_ != null ) battlefield_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.LevelDeployUnitReference leveldeployunit_res = 10
    private List<proto.LevelDeployUnitReference> leveldeployunit_res_ = null;
    public int leveldeployunit_res_size() { return leveldeployunit_res_ == null ? 0 : leveldeployunit_res_.Count; }
    public proto.LevelDeployUnitReference leveldeployunit_res( int index ) { return leveldeployunit_res_[index]; }
    public void add_leveldeployunit_res( proto.LevelDeployUnitReference val ) { 
        if ( leveldeployunit_res_ == null ) leveldeployunit_res_ = new List<proto.LevelDeployUnitReference>();
        leveldeployunit_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_leveldeployunit_res() { 
        if ( leveldeployunit_res_ != null ) leveldeployunit_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.VictoryPointReference victorypoints_res = 11
    private List<proto.VictoryPointReference> victorypoints_res_ = null;
    public int victorypoints_res_size() { return victorypoints_res_ == null ? 0 : victorypoints_res_.Count; }
    public proto.VictoryPointReference victorypoints_res( int index ) { return victorypoints_res_[index]; }
    public void add_victorypoints_res( proto.VictoryPointReference val ) { 
        if ( victorypoints_res_ == null ) victorypoints_res_ = new List<proto.VictoryPointReference>();
        victorypoints_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_victorypoints_res() { 
        if ( victorypoints_res_ != null ) victorypoints_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.BreakingRateReference breaking_rate_res = 12
    private List<proto.BreakingRateReference> breaking_rate_res_ = null;
    public int breaking_rate_res_size() { return breaking_rate_res_ == null ? 0 : breaking_rate_res_.Count; }
    public proto.BreakingRateReference breaking_rate_res( int index ) { return breaking_rate_res_[index]; }
    public void add_breaking_rate_res( proto.BreakingRateReference val ) { 
        if ( breaking_rate_res_ == null ) breaking_rate_res_ = new List<proto.BreakingRateReference>();
        breaking_rate_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_breaking_rate_res() { 
        if ( breaking_rate_res_ != null ) breaking_rate_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.BattleRatingReference battleresult_rating_res = 13
    private List<proto.BattleRatingReference> battleresult_rating_res_ = null;
    public int battleresult_rating_res_size() { return battleresult_rating_res_ == null ? 0 : battleresult_rating_res_.Count; }
    public proto.BattleRatingReference battleresult_rating_res( int index ) { return battleresult_rating_res_[index]; }
    public void add_battleresult_rating_res( proto.BattleRatingReference val ) { 
        if ( battleresult_rating_res_ == null ) battleresult_rating_res_ = new List<proto.BattleRatingReference>();
        battleresult_rating_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_battleresult_rating_res() { 
        if ( battleresult_rating_res_ != null ) battleresult_rating_res_.Clear();
        cached_byte_size = 0;
    }

    //field proto.MedalReference medal_res = 14
    private List<proto.MedalReference> medal_res_ = null;
    public int medal_res_size() { return medal_res_ == null ? 0 : medal_res_.Count; }
    public proto.MedalReference medal_res( int index ) { return medal_res_[index]; }
    public void add_medal_res( proto.MedalReference val ) { 
        if ( medal_res_ == null ) medal_res_ = new List<proto.MedalReference>();
        medal_res_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_medal_res() { 
        if ( medal_res_ != null ) medal_res_.Clear();
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        int list_count = 0;
        list_count = units_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 10 );
            int size = units_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            units_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = parts_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 18 );
            int size = parts_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            parts_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = skill_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 26 );
            int size = skill_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            skill_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = shuttle_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 34 );
            int size = shuttle_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            shuttle_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = shuttle_team_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 42 );
            int size = shuttle_team_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            shuttle_team_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = parts_replace_team_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 50 );
            int size = parts_replace_team_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            parts_replace_team_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = buff_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 58 );
            int size = buff_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            buff_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = defensive_units_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 66 );
            int size = defensive_units_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            defensive_units_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = battlefield_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 74 );
            int size = battlefield_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            battlefield_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = leveldeployunit_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 82 );
            int size = leveldeployunit_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            leveldeployunit_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = victorypoints_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 90 );
            int size = victorypoints_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            victorypoints_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = breaking_rate_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 98 );
            int size = breaking_rate_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            breaking_rate_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = battleresult_rating_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 106 );
            int size = battleresult_rating_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            battleresult_rating_res( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = medal_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 114 );
            int size = medal_res( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            medal_res( i ).Serialize( buf, __index ); __index += size;
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 10:
                int units_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( units_res_size > msg_end-__index ) return false;
                proto.UnitReference units_res_tmp = new proto.UnitReference();
                add_units_res( units_res_tmp );
                if ( units_res_size > 0 ) {
                    if ( !units_res_tmp.Parse( buf, __index, units_res_size ) ) return false;
                    read_len = units_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 18:
                int parts_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( parts_res_size > msg_end-__index ) return false;
                proto.PartReference parts_res_tmp = new proto.PartReference();
                add_parts_res( parts_res_tmp );
                if ( parts_res_size > 0 ) {
                    if ( !parts_res_tmp.Parse( buf, __index, parts_res_size ) ) return false;
                    read_len = parts_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 26:
                int skill_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( skill_res_size > msg_end-__index ) return false;
                proto.SkillReference skill_res_tmp = new proto.SkillReference();
                add_skill_res( skill_res_tmp );
                if ( skill_res_size > 0 ) {
                    if ( !skill_res_tmp.Parse( buf, __index, skill_res_size ) ) return false;
                    read_len = skill_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 34:
                int shuttle_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( shuttle_res_size > msg_end-__index ) return false;
                proto.ShuttleReference shuttle_res_tmp = new proto.ShuttleReference();
                add_shuttle_res( shuttle_res_tmp );
                if ( shuttle_res_size > 0 ) {
                    if ( !shuttle_res_tmp.Parse( buf, __index, shuttle_res_size ) ) return false;
                    read_len = shuttle_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 42:
                int shuttle_team_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( shuttle_team_res_size > msg_end-__index ) return false;
                proto.ShuttleTeamReference shuttle_team_res_tmp = new proto.ShuttleTeamReference();
                add_shuttle_team_res( shuttle_team_res_tmp );
                if ( shuttle_team_res_size > 0 ) {
                    if ( !shuttle_team_res_tmp.Parse( buf, __index, shuttle_team_res_size ) ) return false;
                    read_len = shuttle_team_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 50:
                int parts_replace_team_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( parts_replace_team_res_size > msg_end-__index ) return false;
                proto.PartsReplaceTeamReference parts_replace_team_res_tmp = new proto.PartsReplaceTeamReference();
                add_parts_replace_team_res( parts_replace_team_res_tmp );
                if ( parts_replace_team_res_size > 0 ) {
                    if ( !parts_replace_team_res_tmp.Parse( buf, __index, parts_replace_team_res_size ) ) return false;
                    read_len = parts_replace_team_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 58:
                int buff_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( buff_res_size > msg_end-__index ) return false;
                proto.BuffReference buff_res_tmp = new proto.BuffReference();
                add_buff_res( buff_res_tmp );
                if ( buff_res_size > 0 ) {
                    if ( !buff_res_tmp.Parse( buf, __index, buff_res_size ) ) return false;
                    read_len = buff_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 66:
                int defensive_units_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( defensive_units_res_size > msg_end-__index ) return false;
                proto.DefensiveUnitsReference defensive_units_res_tmp = new proto.DefensiveUnitsReference();
                add_defensive_units_res( defensive_units_res_tmp );
                if ( defensive_units_res_size > 0 ) {
                    if ( !defensive_units_res_tmp.Parse( buf, __index, defensive_units_res_size ) ) return false;
                    read_len = defensive_units_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 74:
                int battlefield_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( battlefield_res_size > msg_end-__index ) return false;
                proto.BattlefieldReference battlefield_res_tmp = new proto.BattlefieldReference();
                add_battlefield_res( battlefield_res_tmp );
                if ( battlefield_res_size > 0 ) {
                    if ( !battlefield_res_tmp.Parse( buf, __index, battlefield_res_size ) ) return false;
                    read_len = battlefield_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 82:
                int leveldeployunit_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( leveldeployunit_res_size > msg_end-__index ) return false;
                proto.LevelDeployUnitReference leveldeployunit_res_tmp = new proto.LevelDeployUnitReference();
                add_leveldeployunit_res( leveldeployunit_res_tmp );
                if ( leveldeployunit_res_size > 0 ) {
                    if ( !leveldeployunit_res_tmp.Parse( buf, __index, leveldeployunit_res_size ) ) return false;
                    read_len = leveldeployunit_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 90:
                int victorypoints_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( victorypoints_res_size > msg_end-__index ) return false;
                proto.VictoryPointReference victorypoints_res_tmp = new proto.VictoryPointReference();
                add_victorypoints_res( victorypoints_res_tmp );
                if ( victorypoints_res_size > 0 ) {
                    if ( !victorypoints_res_tmp.Parse( buf, __index, victorypoints_res_size ) ) return false;
                    read_len = victorypoints_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 98:
                int breaking_rate_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( breaking_rate_res_size > msg_end-__index ) return false;
                proto.BreakingRateReference breaking_rate_res_tmp = new proto.BreakingRateReference();
                add_breaking_rate_res( breaking_rate_res_tmp );
                if ( breaking_rate_res_size > 0 ) {
                    if ( !breaking_rate_res_tmp.Parse( buf, __index, breaking_rate_res_size ) ) return false;
                    read_len = breaking_rate_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 106:
                int battleresult_rating_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( battleresult_rating_res_size > msg_end-__index ) return false;
                proto.BattleRatingReference battleresult_rating_res_tmp = new proto.BattleRatingReference();
                add_battleresult_rating_res( battleresult_rating_res_tmp );
                if ( battleresult_rating_res_size > 0 ) {
                    if ( !battleresult_rating_res_tmp.Parse( buf, __index, battleresult_rating_res_size ) ) return false;
                    read_len = battleresult_rating_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 114:
                int medal_res_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( medal_res_size > msg_end-__index ) return false;
                proto.MedalReference medal_res_tmp = new proto.MedalReference();
                add_medal_res( medal_res_tmp );
                if ( medal_res_size > 0 ) {
                    if ( !medal_res_tmp.Parse( buf, __index, medal_res_size ) ) return false;
                    read_len = medal_res_size;
                } else read_len = 0;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.ReferenceManager" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        if ( units_res_ != null ) units_res_.Clear();
        if ( parts_res_ != null ) parts_res_.Clear();
        if ( skill_res_ != null ) skill_res_.Clear();
        if ( shuttle_res_ != null ) shuttle_res_.Clear();
        if ( shuttle_team_res_ != null ) shuttle_team_res_.Clear();
        if ( parts_replace_team_res_ != null ) parts_replace_team_res_.Clear();
        if ( buff_res_ != null ) buff_res_.Clear();
        if ( defensive_units_res_ != null ) defensive_units_res_.Clear();
        if ( battlefield_res_ != null ) battlefield_res_.Clear();
        if ( leveldeployunit_res_ != null ) leveldeployunit_res_.Clear();
        if ( victorypoints_res_ != null ) victorypoints_res_.Clear();
        if ( breaking_rate_res_ != null ) breaking_rate_res_.Clear();
        if ( battleresult_rating_res_ != null ) battleresult_rating_res_.Clear();
        if ( medal_res_ != null ) medal_res_.Clear();
        cached_byte_size = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        int list_count = 0;
        list_count = units_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 10 );
            int size = units_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = parts_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = parts_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = skill_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 26 );
            int size = skill_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = shuttle_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 34 );
            int size = shuttle_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = shuttle_team_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 42 );
            int size = shuttle_team_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = parts_replace_team_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 50 );
            int size = parts_replace_team_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = buff_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 58 );
            int size = buff_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = defensive_units_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 66 );
            int size = defensive_units_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = battlefield_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 74 );
            int size = battlefield_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = leveldeployunit_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 82 );
            int size = leveldeployunit_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = victorypoints_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 90 );
            int size = victorypoints_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = breaking_rate_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 98 );
            int size = breaking_rate_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = battleresult_rating_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 106 );
            int size = battleresult_rating_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = medal_res_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 114 );
            int size = medal_res( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        return true;
    }

    public override void Print() {
        int list_count = 0;
        Console.Write( "units_res: " );
        list_count = units_res_size();
        for ( int i = 0; i < list_count; i++ )
            units_res( i ).Print();
        Console.WriteLine();
        Console.Write( "parts_res: " );
        list_count = parts_res_size();
        for ( int i = 0; i < list_count; i++ )
            parts_res( i ).Print();
        Console.WriteLine();
        Console.Write( "skill_res: " );
        list_count = skill_res_size();
        for ( int i = 0; i < list_count; i++ )
            skill_res( i ).Print();
        Console.WriteLine();
        Console.Write( "shuttle_res: " );
        list_count = shuttle_res_size();
        for ( int i = 0; i < list_count; i++ )
            shuttle_res( i ).Print();
        Console.WriteLine();
        Console.Write( "shuttle_team_res: " );
        list_count = shuttle_team_res_size();
        for ( int i = 0; i < list_count; i++ )
            shuttle_team_res( i ).Print();
        Console.WriteLine();
        Console.Write( "parts_replace_team_res: " );
        list_count = parts_replace_team_res_size();
        for ( int i = 0; i < list_count; i++ )
            parts_replace_team_res( i ).Print();
        Console.WriteLine();
        Console.Write( "buff_res: " );
        list_count = buff_res_size();
        for ( int i = 0; i < list_count; i++ )
            buff_res( i ).Print();
        Console.WriteLine();
        Console.Write( "defensive_units_res: " );
        list_count = defensive_units_res_size();
        for ( int i = 0; i < list_count; i++ )
            defensive_units_res( i ).Print();
        Console.WriteLine();
        Console.Write( "battlefield_res: " );
        list_count = battlefield_res_size();
        for ( int i = 0; i < list_count; i++ )
            battlefield_res( i ).Print();
        Console.WriteLine();
        Console.Write( "leveldeployunit_res: " );
        list_count = leveldeployunit_res_size();
        for ( int i = 0; i < list_count; i++ )
            leveldeployunit_res( i ).Print();
        Console.WriteLine();
        Console.Write( "victorypoints_res: " );
        list_count = victorypoints_res_size();
        for ( int i = 0; i < list_count; i++ )
            victorypoints_res( i ).Print();
        Console.WriteLine();
        Console.Write( "breaking_rate_res: " );
        list_count = breaking_rate_res_size();
        for ( int i = 0; i < list_count; i++ )
            breaking_rate_res( i ).Print();
        Console.WriteLine();
        Console.Write( "battleresult_rating_res: " );
        list_count = battleresult_rating_res_size();
        for ( int i = 0; i < list_count; i++ )
            battleresult_rating_res( i ).Print();
        Console.WriteLine();
        Console.Write( "medal_res: " );
        list_count = medal_res_size();
        for ( int i = 0; i < list_count; i++ )
            medal_res( i ).Print();
        Console.WriteLine();
    }

}
}

public partial class proto {
public partial class S2CFightFrameInfo: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int framecount = 1
    private int framecount_ = 0;
    public int framecount {
        get { return framecount_; }
        set { framecount_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_framecount() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_framecount() {
        framecount_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int fighttick = 2
    private int fighttick_ = 0;
    public int fighttick {
        get { return fighttick_; }
        set { fighttick_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_fighttick() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_fighttick() {
        fighttick_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field proto.UnitBehavior behaviorsequences = 3
    private List<proto.UnitBehavior> behaviorsequences_ = null;
    public int behaviorsequences_size() { return behaviorsequences_ == null ? 0 : behaviorsequences_.Count; }
    public proto.UnitBehavior behaviorsequences( int index ) { return behaviorsequences_[index]; }
    public void add_behaviorsequences( proto.UnitBehavior val ) { 
        if ( behaviorsequences_ == null ) behaviorsequences_ = new List<proto.UnitBehavior>();
        behaviorsequences_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_behaviorsequences() { 
        if ( behaviorsequences_ != null ) behaviorsequences_.Clear();
        cached_byte_size = 0;
    }

    //field proto.UnderAttackInfo underattacklist = 4
    private List<proto.UnderAttackInfo> underattacklist_ = null;
    public int underattacklist_size() { return underattacklist_ == null ? 0 : underattacklist_.Count; }
    public proto.UnderAttackInfo underattacklist( int index ) { return underattacklist_[index]; }
    public void add_underattacklist( proto.UnderAttackInfo val ) { 
        if ( underattacklist_ == null ) underattacklist_ = new List<proto.UnderAttackInfo>();
        underattacklist_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_underattacklist() { 
        if ( underattacklist_ != null ) underattacklist_.Clear();
        cached_byte_size = 0;
    }

    //field proto.UseSkill useskill = 5
    private List<proto.UseSkill> useskill_ = null;
    public int useskill_size() { return useskill_ == null ? 0 : useskill_.Count; }
    public proto.UseSkill useskill( int index ) { return useskill_[index]; }
    public void add_useskill( proto.UseSkill val ) { 
        if ( useskill_ == null ) useskill_ = new List<proto.UseSkill>();
        useskill_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_useskill() { 
        if ( useskill_ != null ) useskill_.Clear();
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        int list_count = 0;
        if ( has_framecount() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, framecount );
        }
        if ( has_fighttick() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, fighttick );
        }
        list_count = behaviorsequences_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 26 );
            int size = behaviorsequences( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            behaviorsequences( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = underattacklist_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 34 );
            int size = underattacklist( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            underattacklist( i ).Serialize( buf, __index ); __index += size;
        }
        list_count = useskill_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 42 );
            int size = useskill( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            useskill( i ).Serialize( buf, __index ); __index += size;
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                framecount = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                fighttick = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 26:
                int behaviorsequences_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( behaviorsequences_size > msg_end-__index ) return false;
                proto.UnitBehavior behaviorsequences_tmp = new proto.UnitBehavior();
                add_behaviorsequences( behaviorsequences_tmp );
                if ( behaviorsequences_size > 0 ) {
                    if ( !behaviorsequences_tmp.Parse( buf, __index, behaviorsequences_size ) ) return false;
                    read_len = behaviorsequences_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 34:
                int underattacklist_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( underattacklist_size > msg_end-__index ) return false;
                proto.UnderAttackInfo underattacklist_tmp = new proto.UnderAttackInfo();
                add_underattacklist( underattacklist_tmp );
                if ( underattacklist_size > 0 ) {
                    if ( !underattacklist_tmp.Parse( buf, __index, underattacklist_size ) ) return false;
                    read_len = underattacklist_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 42:
                int useskill_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( useskill_size > msg_end-__index ) return false;
                proto.UseSkill useskill_tmp = new proto.UseSkill();
                add_useskill( useskill_tmp );
                if ( useskill_size > 0 ) {
                    if ( !useskill_tmp.Parse( buf, __index, useskill_size ) ) return false;
                    read_len = useskill_size;
                } else read_len = 0;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.S2CFightFrameInfo" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        framecount_ = 0;
        fighttick_ = 0;
        if ( behaviorsequences_ != null ) behaviorsequences_.Clear();
        if ( underattacklist_ != null ) underattacklist_.Clear();
        if ( useskill_ != null ) useskill_.Clear();
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        int list_count = 0;
        if ( has_framecount() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( framecount );
        }
        if ( has_fighttick() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( fighttick );
        }
        list_count = behaviorsequences_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 26 );
            int size = behaviorsequences( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = underattacklist_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 34 );
            int size = underattacklist( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = useskill_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 42 );
            int size = useskill( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        int list_count = 0;
        Console.Write( "framecount: " );
        if ( has_framecount() ) 
            Console.WriteLine( framecount );
        Console.Write( "fighttick: " );
        if ( has_fighttick() ) 
            Console.WriteLine( fighttick );
        Console.Write( "behaviorsequences: " );
        list_count = behaviorsequences_size();
        for ( int i = 0; i < list_count; i++ )
            behaviorsequences( i ).Print();
        Console.WriteLine();
        Console.Write( "underattacklist: " );
        list_count = underattacklist_size();
        for ( int i = 0; i < list_count; i++ )
            underattacklist( i ).Print();
        Console.WriteLine();
        Console.Write( "useskill: " );
        list_count = useskill_size();
        for ( int i = 0; i < list_count; i++ )
            useskill( i ).Print();
        Console.WriteLine();
    }

}
}

public partial class proto {
public partial class S2CFightReport: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field proto.S2CFightReport.Result fightresult = 1
    private proto.S2CFightReport.Result fightresult_ = proto.S2CFightReport.Result.Win;
    public proto.S2CFightReport.Result fightresult {
        get { return fightresult_; }
        set { fightresult_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_fightresult() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_fightresult() {
        fightresult_ = proto.S2CFightReport.Result.Win;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int grade = 2
    private int grade_ = 0;
    public int grade {
        get { return grade_; }
        set { grade_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_grade() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_grade() {
        grade_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int destroyratio = 3
    private int destroyratio_ = 0;
    public int destroyratio {
        get { return destroyratio_; }
        set { destroyratio_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_destroyratio() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_destroyratio() {
        destroyratio_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int exp = 4
    private int exp_ = 0;
    public int exp {
        get { return exp_; }
        set { exp_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_exp() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_exp() {
        exp_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int extraexp = 5
    private int extraexp_ = 0;
    public int extraexp {
        get { return extraexp_; }
        set { extraexp_ = value; 
              has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_extraexp() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_extraexp() {
        extraexp_ = 0;
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int mdeal = 6
    private int mdeal_ = 0;
    public int mdeal {
        get { return mdeal_; }
        set { mdeal_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_mdeal() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_mdeal() {
        mdeal_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field int energy = 7
    private int energy_ = 0;
    public int energy {
        get { return energy_; }
        set { energy_ = value; 
              has_flag_0 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_energy() {
        return ( has_flag_0 & 0x40 ) != 0;
    }
    public void clear_energy() {
        energy_ = 0;
        has_flag_0 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    //field int capital = 8
    private int capital_ = 0;
    public int capital {
        get { return capital_; }
        set { capital_ = value; 
              has_flag_0 |= 0x80;
              cached_byte_size = 0; }
    }
    public bool has_capital() {
        return ( has_flag_0 & 0x80 ) != 0;
    }
    public void clear_capital() {
        capital_ = 0;
        has_flag_0 &= 0xffffff7f;
        cached_byte_size = 0;
    }

    //field proto.LostUnitInfo lostunit = 9
    private List<proto.LostUnitInfo> lostunit_ = null;
    public int lostunit_size() { return lostunit_ == null ? 0 : lostunit_.Count; }
    public proto.LostUnitInfo lostunit( int index ) { return lostunit_[index]; }
    public void add_lostunit( proto.LostUnitInfo val ) { 
        if ( lostunit_ == null ) lostunit_ = new List<proto.LostUnitInfo>();
        lostunit_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_lostunit() { 
        if ( lostunit_ != null ) lostunit_.Clear();
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        int list_count = 0;
        if ( has_fightresult() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, (int)fightresult );
        }
        if ( has_grade() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, grade );
        }
        if ( has_destroyratio() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, destroyratio );
        }
        if ( has_exp() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, exp );
        }
        if ( has_extraexp() ) {
            __index += WriteInt( buf, __index, 40 );
            __index += WriteInt( buf, __index, extraexp );
        }
        if ( has_mdeal() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, mdeal );
        }
        if ( has_energy() ) {
            __index += WriteInt( buf, __index, 56 );
            __index += WriteInt( buf, __index, energy );
        }
        if ( has_capital() ) {
            __index += WriteInt( buf, __index, 64 );
            __index += WriteInt( buf, __index, capital );
        }
        list_count = lostunit_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 74 );
            int size = lostunit( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            lostunit( i ).Serialize( buf, __index ); __index += size;
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                fightresult = (proto.S2CFightReport.Result)ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                grade = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 24:
                destroyratio = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                exp = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 40:
                extraexp = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 48:
                mdeal = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 56:
                energy = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 64:
                capital = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 74:
                int lostunit_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( lostunit_size > msg_end-__index ) return false;
                proto.LostUnitInfo lostunit_tmp = new proto.LostUnitInfo();
                add_lostunit( lostunit_tmp );
                if ( lostunit_size > 0 ) {
                    if ( !lostunit_tmp.Parse( buf, __index, lostunit_size ) ) return false;
                    read_len = lostunit_size;
                } else read_len = 0;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.S2CFightReport" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        fightresult_ = proto.S2CFightReport.Result.Win;
        grade_ = 0;
        destroyratio_ = 0;
        exp_ = 0;
        extraexp_ = 0;
        mdeal_ = 0;
        energy_ = 0;
        capital_ = 0;
        if ( lostunit_ != null ) lostunit_.Clear();
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        int list_count = 0;
        if ( has_fightresult() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( (int)fightresult );
        }
        if ( has_grade() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( grade );
        }
        if ( has_destroyratio() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( destroyratio );
        }
        if ( has_exp() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( exp );
        }
        if ( has_extraexp() ) {
            cached_byte_size += WriteIntSize( 40 );
            cached_byte_size += WriteIntSize( extraexp );
        }
        if ( has_mdeal() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( mdeal );
        }
        if ( has_energy() ) {
            cached_byte_size += WriteIntSize( 56 );
            cached_byte_size += WriteIntSize( energy );
        }
        if ( has_capital() ) {
            cached_byte_size += WriteIntSize( 64 );
            cached_byte_size += WriteIntSize( capital );
        }
        list_count = lostunit_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 74 );
            int size = lostunit( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        int list_count = 0;
        Console.Write( "fightresult: " );
        if ( has_fightresult() ) 
            Console.WriteLine( fightresult );
        Console.Write( "grade: " );
        if ( has_grade() ) 
            Console.WriteLine( grade );
        Console.Write( "destroyratio: " );
        if ( has_destroyratio() ) 
            Console.WriteLine( destroyratio );
        Console.Write( "exp: " );
        if ( has_exp() ) 
            Console.WriteLine( exp );
        Console.Write( "extraexp: " );
        if ( has_extraexp() ) 
            Console.WriteLine( extraexp );
        Console.Write( "mdeal: " );
        if ( has_mdeal() ) 
            Console.WriteLine( mdeal );
        Console.Write( "energy: " );
        if ( has_energy() ) 
            Console.WriteLine( energy );
        Console.Write( "capital: " );
        if ( has_capital() ) 
            Console.WriteLine( capital );
        Console.Write( "lostunit: " );
        list_count = lostunit_size();
        for ( int i = 0; i < list_count; i++ )
            lostunit( i ).Print();
        Console.WriteLine();
    }

}
}

public partial class proto {
public partial class S2CLogin: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field proto.S2CLogin.ErrorCode error_code = 1
    private proto.S2CLogin.ErrorCode error_code_ = proto.S2CLogin.ErrorCode.OK;
    public proto.S2CLogin.ErrorCode error_code {
        get { return error_code_; }
        set { error_code_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_error_code() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_error_code() {
        error_code_ = proto.S2CLogin.ErrorCode.OK;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field proto.Player player = 2
    private proto.Player player_ = null;
    public proto.Player player {
        get { return player_; }
        set { player_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_player() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_player() {
        player_ = null;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field uint server_time = 3
    private uint server_time_ = 0;
    public uint server_time {
        get { return server_time_; }
        set { server_time_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_server_time() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_server_time() {
        server_time_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_error_code() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, (int)error_code );
        }
        if ( has_player() ) {
            __index += WriteInt( buf, __index, 18 );
            int size = player.ByteSize();
            __index += WriteInt( buf, __index, size );
            player.Serialize( buf, __index ); __index += size;
        }
        if ( has_server_time() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteUInt( buf, __index, server_time );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                error_code = (proto.S2CLogin.ErrorCode)ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                int player_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( player_size > msg_end-__index ) return false;
                player = new proto.Player();
                if ( player_size > 0 ) {
                    if ( !player.Parse( buf, __index, player_size ) ) return false;
                    read_len = player_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 24:
                server_time = ReadUInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.S2CLogin" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        error_code_ = proto.S2CLogin.ErrorCode.OK;
        player_ = null;
        server_time_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_error_code() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( (int)error_code );
        }
        if ( has_player() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = player.ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_server_time() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteUIntSize( server_time );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "error_code: " );
        if ( has_error_code() ) 
            Console.WriteLine( error_code );
        Console.Write( "player: " );
        if ( has_player() ) 
            player.Print();
        Console.Write( "server_time: " );
        if ( has_server_time() ) 
            Console.WriteLine( server_time );
    }

}
}

public partial class proto {
public partial class S2CSyncUnitAttr: ProtoMessage {
    public int cached_byte_size;

    //field proto.UnitAttri unitattrlist = 1
    private List<proto.UnitAttri> unitattrlist_ = null;
    public int unitattrlist_size() { return unitattrlist_ == null ? 0 : unitattrlist_.Count; }
    public proto.UnitAttri unitattrlist( int index ) { return unitattrlist_[index]; }
    public void add_unitattrlist( proto.UnitAttri val ) { 
        if ( unitattrlist_ == null ) unitattrlist_ = new List<proto.UnitAttri>();
        unitattrlist_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_unitattrlist() { 
        if ( unitattrlist_ != null ) unitattrlist_.Clear();
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        int list_count = 0;
        list_count = unitattrlist_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 10 );
            int size = unitattrlist( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            unitattrlist( i ).Serialize( buf, __index ); __index += size;
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 10:
                int unitattrlist_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( unitattrlist_size > msg_end-__index ) return false;
                proto.UnitAttri unitattrlist_tmp = new proto.UnitAttri();
                add_unitattrlist( unitattrlist_tmp );
                if ( unitattrlist_size > 0 ) {
                    if ( !unitattrlist_tmp.Parse( buf, __index, unitattrlist_size ) ) return false;
                    read_len = unitattrlist_size;
                } else read_len = 0;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.S2CSyncUnitAttr" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        if ( unitattrlist_ != null ) unitattrlist_.Clear();
        cached_byte_size = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        int list_count = 0;
        list_count = unitattrlist_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 10 );
            int size = unitattrlist( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        return true;
    }

    public override void Print() {
        int list_count = 0;
        Console.Write( "unitattrlist: " );
        list_count = unitattrlist_size();
        for ( int i = 0; i < list_count; i++ )
            unitattrlist( i ).Print();
        Console.WriteLine();
    }

}
}

public partial class proto {
public partial class S2CSystem: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field proto.S2CSystem.Type msg_type = 1
    private proto.S2CSystem.Type msg_type_ = proto.S2CSystem.Type.InvalidSession;
    public proto.S2CSystem.Type msg_type {
        get { return msg_type_; }
        set { msg_type_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_msg_type() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_msg_type() {
        msg_type_ = proto.S2CSystem.Type.InvalidSession;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_msg_type() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, (int)msg_type );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                msg_type = (proto.S2CSystem.Type)ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.S2CSystem" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        msg_type_ = proto.S2CSystem.Type.InvalidSession;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_msg_type() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( (int)msg_type );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "msg_type: " );
        if ( has_msg_type() ) 
            Console.WriteLine( msg_type );
    }

}
}

public partial class proto {
public partial class ServerVerify: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field uint ip = 2
    private uint ip_ = 0;
    public uint ip {
        get { return ip_; }
        set { ip_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_ip() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_ip() {
        ip_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int port = 3
    private int port_ = 0;
    public int port {
        get { return port_; }
        set { port_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_port() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_port() {
        port_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_ip() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteUInt( buf, __index, ip );
        }
        if ( has_port() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, port );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                ip = ReadUInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 24:
                port = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.ServerVerify" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        ip_ = 0;
        port_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_ip() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteUIntSize( ip );
        }
        if ( has_port() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( port );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "ip: " );
        if ( has_ip() ) 
            Console.WriteLine( ip );
        Console.Write( "port: " );
        if ( has_port() ) 
            Console.WriteLine( port );
    }

}
}

public partial class proto {
public partial class Ship: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int level = 2
    private int level_ = 0;
    public int level {
        get { return level_; }
        set { level_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_level() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_level() {
        level_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_level() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, level );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                level = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.Ship" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        level_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_level() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( level );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "level: " );
        if ( has_level() ) 
            Console.WriteLine( level );
    }

}
}

public partial class proto {
public partial class ShuttleReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.ShuttleReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
    }

}
}

public partial class proto {
public partial class ShuttleTeamReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.ShuttleTeamReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
    }

}
}

public partial class proto {
public partial class SkillEffect: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int targetid = 1
    private int targetid_ = 0;
    public int targetid {
        get { return targetid_; }
        set { targetid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_targetid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_targetid() {
        targetid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int val = 2
    private int val_ = 0;
    public int val {
        get { return val_; }
        set { val_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_val() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_val() {
        val_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_targetid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, targetid );
        }
        if ( has_val() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, val );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                targetid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                val = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.SkillEffect" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        targetid_ = 0;
        val_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_targetid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( targetid );
        }
        if ( has_val() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( val );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        return true;
    }

    public override void Print() {
        Console.Write( "targetid: " );
        if ( has_targetid() ) 
            Console.WriteLine( targetid );
        Console.Write( "val: " );
        if ( has_val() ) 
            Console.WriteLine( val );
    }

}
}

public partial class proto {
public partial class SkillReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int skill_select_type = 3
    private int skill_select_type_ = 0;
    public int skill_select_type {
        get { return skill_select_type_; }
        set { skill_select_type_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_skill_select_type() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_skill_select_type() {
        skill_select_type_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int cast_target_type = 4
    private int cast_target_type_ = 0;
    public int cast_target_type {
        get { return cast_target_type_; }
        set { cast_target_type_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_cast_target_type() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_cast_target_type() {
        cast_target_type_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int advantage_unitstrait_type = 5
    private int advantage_unitstrait_type_ = 0;
    public int advantage_unitstrait_type {
        get { return advantage_unitstrait_type_; }
        set { advantage_unitstrait_type_ = value; 
              has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_advantage_unitstrait_type() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_advantage_unitstrait_type() {
        advantage_unitstrait_type_ = 0;
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int disadvantage_unitstrait_type = 6
    private int disadvantage_unitstrait_type_ = 0;
    public int disadvantage_unitstrait_type {
        get { return disadvantage_unitstrait_type_; }
        set { disadvantage_unitstrait_type_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_disadvantage_unitstrait_type() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_disadvantage_unitstrait_type() {
        disadvantage_unitstrait_type_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field int cd = 7
    private int cd_ = 0;
    public int cd {
        get { return cd_; }
        set { cd_ = value; 
              has_flag_0 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_cd() {
        return ( has_flag_0 & 0x40 ) != 0;
    }
    public void clear_cd() {
        cd_ = 0;
        has_flag_0 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    //field int skill_effect_type = 8
    private int skill_effect_type_ = 0;
    public int skill_effect_type {
        get { return skill_effect_type_; }
        set { skill_effect_type_ = value; 
              has_flag_0 |= 0x80;
              cached_byte_size = 0; }
    }
    public bool has_skill_effect_type() {
        return ( has_flag_0 & 0x80 ) != 0;
    }
    public void clear_skill_effect_type() {
        skill_effect_type_ = 0;
        has_flag_0 &= 0xffffff7f;
        cached_byte_size = 0;
    }

    //field int effect_val = 9
    private int effect_val_ = 0;
    public int effect_val {
        get { return effect_val_; }
        set { effect_val_ = value; 
              has_flag_0 |= 0x100;
              cached_byte_size = 0; }
    }
    public bool has_effect_val() {
        return ( has_flag_0 & 0x100 ) != 0;
    }
    public void clear_effect_val() {
        effect_val_ = 0;
        has_flag_0 &= 0xfffffeff;
        cached_byte_size = 0;
    }

    //field int give_buff = 10
    private int give_buff_ = 0;
    public int give_buff {
        get { return give_buff_; }
        set { give_buff_ = value; 
              has_flag_0 |= 0x200;
              cached_byte_size = 0; }
    }
    public bool has_give_buff() {
        return ( has_flag_0 & 0x200 ) != 0;
    }
    public void clear_give_buff() {
        give_buff_ = 0;
        has_flag_0 &= 0xfffffdff;
        cached_byte_size = 0;
    }

    //field int ammo_num = 11
    private int ammo_num_ = 0;
    public int ammo_num {
        get { return ammo_num_; }
        set { ammo_num_ = value; 
              has_flag_0 |= 0x400;
              cached_byte_size = 0; }
    }
    public bool has_ammo_num() {
        return ( has_flag_0 & 0x400 ) != 0;
    }
    public void clear_ammo_num() {
        ammo_num_ = 0;
        has_flag_0 &= 0xfffffbff;
        cached_byte_size = 0;
    }

    //field int energy_cost = 12
    private int energy_cost_ = 0;
    public int energy_cost {
        get { return energy_cost_; }
        set { energy_cost_ = value; 
              has_flag_0 |= 0x800;
              cached_byte_size = 0; }
    }
    public bool has_energy_cost() {
        return ( has_flag_0 & 0x800 ) != 0;
    }
    public void clear_energy_cost() {
        energy_cost_ = 0;
        has_flag_0 &= 0xfffff7ff;
        cached_byte_size = 0;
    }

    //field int cast_range = 13
    private int cast_range_ = 0;
    public int cast_range {
        get { return cast_range_; }
        set { cast_range_ = value; 
              has_flag_0 |= 0x1000;
              cached_byte_size = 0; }
    }
    public bool has_cast_range() {
        return ( has_flag_0 & 0x1000 ) != 0;
    }
    public void clear_cast_range() {
        cast_range_ = 0;
        has_flag_0 &= 0xffffefff;
        cached_byte_size = 0;
    }

    //field int cast_angle = 14
    private int cast_angle_ = 0;
    public int cast_angle {
        get { return cast_angle_; }
        set { cast_angle_ = value; 
              has_flag_0 |= 0x2000;
              cached_byte_size = 0; }
    }
    public bool has_cast_angle() {
        return ( has_flag_0 & 0x2000 ) != 0;
    }
    public void clear_cast_angle() {
        cast_angle_ = 0;
        has_flag_0 &= 0xffffdfff;
        cached_byte_size = 0;
    }

    //field int shape_type = 15
    private int shape_type_ = 0;
    public int shape_type {
        get { return shape_type_; }
        set { shape_type_ = value; 
              has_flag_0 |= 0x4000;
              cached_byte_size = 0; }
    }
    public bool has_shape_type() {
        return ( has_flag_0 & 0x4000 ) != 0;
    }
    public void clear_shape_type() {
        shape_type_ = 0;
        has_flag_0 &= 0xffffbfff;
        cached_byte_size = 0;
    }

    //field int aoe_range = 16
    private int aoe_range_ = 0;
    public int aoe_range {
        get { return aoe_range_; }
        set { aoe_range_ = value; 
              has_flag_0 |= 0x8000;
              cached_byte_size = 0; }
    }
    public bool has_aoe_range() {
        return ( has_flag_0 & 0x8000 ) != 0;
    }
    public void clear_aoe_range() {
        aoe_range_ = 0;
        has_flag_0 &= 0xffff7fff;
        cached_byte_size = 0;
    }

    //field int radiate_len = 17
    private int radiate_len_ = 0;
    public int radiate_len {
        get { return radiate_len_; }
        set { radiate_len_ = value; 
              has_flag_0 |= 0x10000;
              cached_byte_size = 0; }
    }
    public bool has_radiate_len() {
        return ( has_flag_0 & 0x10000 ) != 0;
    }
    public void clear_radiate_len() {
        radiate_len_ = 0;
        has_flag_0 &= 0xfffeffff;
        cached_byte_size = 0;
    }

    //field int radiate_wid = 18
    private int radiate_wid_ = 0;
    public int radiate_wid {
        get { return radiate_wid_; }
        set { radiate_wid_ = value; 
              has_flag_0 |= 0x20000;
              cached_byte_size = 0; }
    }
    public bool has_radiate_wid() {
        return ( has_flag_0 & 0x20000 ) != 0;
    }
    public void clear_radiate_wid() {
        radiate_wid_ = 0;
        has_flag_0 &= 0xfffdffff;
        cached_byte_size = 0;
    }

    //field int missle_vel = 19
    private int missle_vel_ = 0;
    public int missle_vel {
        get { return missle_vel_; }
        set { missle_vel_ = value; 
              has_flag_0 |= 0x40000;
              cached_byte_size = 0; }
    }
    public bool has_missle_vel() {
        return ( has_flag_0 & 0x40000 ) != 0;
    }
    public void clear_missle_vel() {
        missle_vel_ = 0;
        has_flag_0 &= 0xfffbffff;
        cached_byte_size = 0;
    }

    //field int continued_time = 20
    private int continued_time_ = 0;
    public int continued_time {
        get { return continued_time_; }
        set { continued_time_ = value; 
              has_flag_0 |= 0x80000;
              cached_byte_size = 0; }
    }
    public bool has_continued_time() {
        return ( has_flag_0 & 0x80000 ) != 0;
    }
    public void clear_continued_time() {
        continued_time_ = 0;
        has_flag_0 &= 0xfff7ffff;
        cached_byte_size = 0;
    }

    //field string cartoon_res = 21
    private string cartoon_res_ = "";
    public string cartoon_res {
        get { return cartoon_res_; }
        set { cartoon_res_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xffefffff;
              else
                  has_flag_0 |= 0x100000;
              cached_byte_size = 0; }
    }
    public bool has_cartoon_res() {
        return ( has_flag_0 & 0x100000 ) != 0;
    }
    public void clear_cartoon_res() {
        cartoon_res_ = "";
        has_flag_0 &= 0xffefffff;
        cached_byte_size = 0;
    }

    //field string enableicon = 22
    private string enableicon_ = "";
    public string enableicon {
        get { return enableicon_; }
        set { enableicon_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xffdfffff;
              else
                  has_flag_0 |= 0x200000;
              cached_byte_size = 0; }
    }
    public bool has_enableicon() {
        return ( has_flag_0 & 0x200000 ) != 0;
    }
    public void clear_enableicon() {
        enableicon_ = "";
        has_flag_0 &= 0xffdfffff;
        cached_byte_size = 0;
    }

    //field string disableicon = 23
    private string disableicon_ = "";
    public string disableicon {
        get { return disableicon_; }
        set { disableicon_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xffbfffff;
              else
                  has_flag_0 |= 0x400000;
              cached_byte_size = 0; }
    }
    public bool has_disableicon() {
        return ( has_flag_0 & 0x400000 ) != 0;
    }
    public void clear_disableicon() {
        disableicon_ = "";
        has_flag_0 &= 0xffbfffff;
        cached_byte_size = 0;
    }

    //field string note = 24
    private string note_ = "";
    public string note {
        get { return note_; }
        set { note_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xff7fffff;
              else
                  has_flag_0 |= 0x800000;
              cached_byte_size = 0; }
    }
    public bool has_note() {
        return ( has_flag_0 & 0x800000 ) != 0;
    }
    public void clear_note() {
        note_ = "";
        has_flag_0 &= 0xff7fffff;
        cached_byte_size = 0;
    }

    //field int skill_lead_time = 25
    private int skill_lead_time_ = 0;
    public int skill_lead_time {
        get { return skill_lead_time_; }
        set { skill_lead_time_ = value; 
              has_flag_0 |= 0x1000000;
              cached_byte_size = 0; }
    }
    public bool has_skill_lead_time() {
        return ( has_flag_0 & 0x1000000 ) != 0;
    }
    public void clear_skill_lead_time() {
        skill_lead_time_ = 0;
        has_flag_0 &= 0xfeffffff;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_skill_select_type() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, skill_select_type );
        }
        if ( has_cast_target_type() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, cast_target_type );
        }
        if ( has_advantage_unitstrait_type() ) {
            __index += WriteInt( buf, __index, 40 );
            __index += WriteInt( buf, __index, advantage_unitstrait_type );
        }
        if ( has_disadvantage_unitstrait_type() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, disadvantage_unitstrait_type );
        }
        if ( has_cd() ) {
            __index += WriteInt( buf, __index, 56 );
            __index += WriteInt( buf, __index, cd );
        }
        if ( has_skill_effect_type() ) {
            __index += WriteInt( buf, __index, 64 );
            __index += WriteInt( buf, __index, skill_effect_type );
        }
        if ( has_effect_val() ) {
            __index += WriteInt( buf, __index, 72 );
            __index += WriteInt( buf, __index, effect_val );
        }
        if ( has_give_buff() ) {
            __index += WriteInt( buf, __index, 80 );
            __index += WriteInt( buf, __index, give_buff );
        }
        if ( has_ammo_num() ) {
            __index += WriteInt( buf, __index, 88 );
            __index += WriteInt( buf, __index, ammo_num );
        }
        if ( has_energy_cost() ) {
            __index += WriteInt( buf, __index, 96 );
            __index += WriteInt( buf, __index, energy_cost );
        }
        if ( has_cast_range() ) {
            __index += WriteInt( buf, __index, 104 );
            __index += WriteInt( buf, __index, cast_range );
        }
        if ( has_cast_angle() ) {
            __index += WriteInt( buf, __index, 112 );
            __index += WriteInt( buf, __index, cast_angle );
        }
        if ( has_shape_type() ) {
            __index += WriteInt( buf, __index, 120 );
            __index += WriteInt( buf, __index, shape_type );
        }
        if ( has_aoe_range() ) {
            __index += WriteInt( buf, __index, 128 );
            __index += WriteInt( buf, __index, aoe_range );
        }
        if ( has_radiate_len() ) {
            __index += WriteInt( buf, __index, 136 );
            __index += WriteInt( buf, __index, radiate_len );
        }
        if ( has_radiate_wid() ) {
            __index += WriteInt( buf, __index, 144 );
            __index += WriteInt( buf, __index, radiate_wid );
        }
        if ( has_missle_vel() ) {
            __index += WriteInt( buf, __index, 152 );
            __index += WriteInt( buf, __index, missle_vel );
        }
        if ( has_continued_time() ) {
            __index += WriteInt( buf, __index, 160 );
            __index += WriteInt( buf, __index, continued_time );
        }
        if ( has_cartoon_res() ) {
            __index += WriteInt( buf, __index, 170 );
            __index += WriteString( buf, buf_size-__index, __index, cartoon_res );
        }
        if ( has_enableicon() ) {
            __index += WriteInt( buf, __index, 178 );
            __index += WriteString( buf, buf_size-__index, __index, enableicon );
        }
        if ( has_disableicon() ) {
            __index += WriteInt( buf, __index, 186 );
            __index += WriteString( buf, buf_size-__index, __index, disableicon );
        }
        if ( has_note() ) {
            __index += WriteInt( buf, __index, 194 );
            __index += WriteString( buf, buf_size-__index, __index, note );
        }
        if ( has_skill_lead_time() ) {
            __index += WriteInt( buf, __index, 200 );
            __index += WriteInt( buf, __index, skill_lead_time );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                skill_select_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                cast_target_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 40:
                advantage_unitstrait_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 48:
                disadvantage_unitstrait_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 56:
                cd = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 64:
                skill_effect_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 72:
                effect_val = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 80:
                give_buff = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 88:
                ammo_num = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 96:
                energy_cost = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 104:
                cast_range = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 112:
                cast_angle = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 120:
                shape_type = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 128:
                aoe_range = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 136:
                radiate_len = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 144:
                radiate_wid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 152:
                missle_vel = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 160:
                continued_time = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 170:
                cartoon_res = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( cartoon_res == null ) return false;
                __index += read_len;
                break;
            case 178:
                enableicon = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( enableicon == null ) return false;
                __index += read_len;
                break;
            case 186:
                disableicon = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( disableicon == null ) return false;
                __index += read_len;
                break;
            case 194:
                note = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( note == null ) return false;
                __index += read_len;
                break;
            case 200:
                skill_lead_time = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.SkillReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        skill_select_type_ = 0;
        cast_target_type_ = 0;
        advantage_unitstrait_type_ = 0;
        disadvantage_unitstrait_type_ = 0;
        cd_ = 0;
        skill_effect_type_ = 0;
        effect_val_ = 0;
        give_buff_ = 0;
        ammo_num_ = 0;
        energy_cost_ = 0;
        cast_range_ = 0;
        cast_angle_ = 0;
        shape_type_ = 0;
        aoe_range_ = 0;
        radiate_len_ = 0;
        radiate_wid_ = 0;
        missle_vel_ = 0;
        continued_time_ = 0;
        cartoon_res_ = "";
        enableicon_ = "";
        disableicon_ = "";
        note_ = "";
        skill_lead_time_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_skill_select_type() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( skill_select_type );
        }
        if ( has_cast_target_type() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( cast_target_type );
        }
        if ( has_advantage_unitstrait_type() ) {
            cached_byte_size += WriteIntSize( 40 );
            cached_byte_size += WriteIntSize( advantage_unitstrait_type );
        }
        if ( has_disadvantage_unitstrait_type() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( disadvantage_unitstrait_type );
        }
        if ( has_cd() ) {
            cached_byte_size += WriteIntSize( 56 );
            cached_byte_size += WriteIntSize( cd );
        }
        if ( has_skill_effect_type() ) {
            cached_byte_size += WriteIntSize( 64 );
            cached_byte_size += WriteIntSize( skill_effect_type );
        }
        if ( has_effect_val() ) {
            cached_byte_size += WriteIntSize( 72 );
            cached_byte_size += WriteIntSize( effect_val );
        }
        if ( has_give_buff() ) {
            cached_byte_size += WriteIntSize( 80 );
            cached_byte_size += WriteIntSize( give_buff );
        }
        if ( has_ammo_num() ) {
            cached_byte_size += WriteIntSize( 88 );
            cached_byte_size += WriteIntSize( ammo_num );
        }
        if ( has_energy_cost() ) {
            cached_byte_size += WriteIntSize( 96 );
            cached_byte_size += WriteIntSize( energy_cost );
        }
        if ( has_cast_range() ) {
            cached_byte_size += WriteIntSize( 104 );
            cached_byte_size += WriteIntSize( cast_range );
        }
        if ( has_cast_angle() ) {
            cached_byte_size += WriteIntSize( 112 );
            cached_byte_size += WriteIntSize( cast_angle );
        }
        if ( has_shape_type() ) {
            cached_byte_size += WriteIntSize( 120 );
            cached_byte_size += WriteIntSize( shape_type );
        }
        if ( has_aoe_range() ) {
            cached_byte_size += WriteIntSize( 128 );
            cached_byte_size += WriteIntSize( aoe_range );
        }
        if ( has_radiate_len() ) {
            cached_byte_size += WriteIntSize( 136 );
            cached_byte_size += WriteIntSize( radiate_len );
        }
        if ( has_radiate_wid() ) {
            cached_byte_size += WriteIntSize( 144 );
            cached_byte_size += WriteIntSize( radiate_wid );
        }
        if ( has_missle_vel() ) {
            cached_byte_size += WriteIntSize( 152 );
            cached_byte_size += WriteIntSize( missle_vel );
        }
        if ( has_continued_time() ) {
            cached_byte_size += WriteIntSize( 160 );
            cached_byte_size += WriteIntSize( continued_time );
        }
        if ( has_cartoon_res() ) {
            cached_byte_size += WriteIntSize( 170 );
            int size = WriteStringSize( cartoon_res );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_enableicon() ) {
            cached_byte_size += WriteIntSize( 178 );
            int size = WriteStringSize( enableicon );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_disableicon() ) {
            cached_byte_size += WriteIntSize( 186 );
            int size = WriteStringSize( disableicon );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_note() ) {
            cached_byte_size += WriteIntSize( 194 );
            int size = WriteStringSize( note );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_skill_lead_time() ) {
            cached_byte_size += WriteIntSize( 200 );
            cached_byte_size += WriteIntSize( skill_lead_time );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "skill_select_type: " );
        if ( has_skill_select_type() ) 
            Console.WriteLine( skill_select_type );
        Console.Write( "cast_target_type: " );
        if ( has_cast_target_type() ) 
            Console.WriteLine( cast_target_type );
        Console.Write( "advantage_unitstrait_type: " );
        if ( has_advantage_unitstrait_type() ) 
            Console.WriteLine( advantage_unitstrait_type );
        Console.Write( "disadvantage_unitstrait_type: " );
        if ( has_disadvantage_unitstrait_type() ) 
            Console.WriteLine( disadvantage_unitstrait_type );
        Console.Write( "cd: " );
        if ( has_cd() ) 
            Console.WriteLine( cd );
        Console.Write( "skill_effect_type: " );
        if ( has_skill_effect_type() ) 
            Console.WriteLine( skill_effect_type );
        Console.Write( "effect_val: " );
        if ( has_effect_val() ) 
            Console.WriteLine( effect_val );
        Console.Write( "give_buff: " );
        if ( has_give_buff() ) 
            Console.WriteLine( give_buff );
        Console.Write( "ammo_num: " );
        if ( has_ammo_num() ) 
            Console.WriteLine( ammo_num );
        Console.Write( "energy_cost: " );
        if ( has_energy_cost() ) 
            Console.WriteLine( energy_cost );
        Console.Write( "cast_range: " );
        if ( has_cast_range() ) 
            Console.WriteLine( cast_range );
        Console.Write( "cast_angle: " );
        if ( has_cast_angle() ) 
            Console.WriteLine( cast_angle );
        Console.Write( "shape_type: " );
        if ( has_shape_type() ) 
            Console.WriteLine( shape_type );
        Console.Write( "aoe_range: " );
        if ( has_aoe_range() ) 
            Console.WriteLine( aoe_range );
        Console.Write( "radiate_len: " );
        if ( has_radiate_len() ) 
            Console.WriteLine( radiate_len );
        Console.Write( "radiate_wid: " );
        if ( has_radiate_wid() ) 
            Console.WriteLine( radiate_wid );
        Console.Write( "missle_vel: " );
        if ( has_missle_vel() ) 
            Console.WriteLine( missle_vel );
        Console.Write( "continued_time: " );
        if ( has_continued_time() ) 
            Console.WriteLine( continued_time );
        Console.Write( "cartoon_res: " );
        if ( has_cartoon_res() ) 
            Console.WriteLine( cartoon_res );
        Console.Write( "enableicon: " );
        if ( has_enableicon() ) 
            Console.WriteLine( enableicon );
        Console.Write( "disableicon: " );
        if ( has_disableicon() ) 
            Console.WriteLine( disableicon );
        Console.Write( "note: " );
        if ( has_note() ) 
            Console.WriteLine( note );
        Console.Write( "skill_lead_time: " );
        if ( has_skill_lead_time() ) 
            Console.WriteLine( skill_lead_time );
    }

}
}

public partial class proto {
public partial class UnderAttackInfo: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int unitid = 1
    private int unitid_ = 0;
    public int unitid {
        get { return unitid_; }
        set { unitid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_unitid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_unitid() {
        unitid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int durablility = 2
    private int durablility_ = 0;
    public int durablility {
        get { return durablility_; }
        set { durablility_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_durablility() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_durablility() {
        durablility_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int armor = 3
    private int armor_ = 0;
    public int armor {
        get { return armor_; }
        set { armor_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_armor() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_armor() {
        armor_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int energy = 4
    private int energy_ = 0;
    public int energy {
        get { return energy_; }
        set { energy_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_energy() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_energy() {
        energy_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_unitid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, unitid );
        }
        if ( has_durablility() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, durablility );
        }
        if ( has_armor() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, armor );
        }
        if ( has_energy() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, energy );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                unitid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                durablility = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 24:
                armor = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                energy = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.UnderAttackInfo" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        unitid_ = 0;
        durablility_ = 0;
        armor_ = 0;
        energy_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_unitid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( unitid );
        }
        if ( has_durablility() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( durablility );
        }
        if ( has_armor() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( armor );
        }
        if ( has_energy() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( energy );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "unitid: " );
        if ( has_unitid() ) 
            Console.WriteLine( unitid );
        Console.Write( "durablility: " );
        if ( has_durablility() ) 
            Console.WriteLine( durablility );
        Console.Write( "armor: " );
        if ( has_armor() ) 
            Console.WriteLine( armor );
        Console.Write( "energy: " );
        if ( has_energy() ) 
            Console.WriteLine( energy );
    }

}
}

public partial class proto {
public partial class UnitAttri: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int unitid = 1
    private int unitid_ = 0;
    public int unitid {
        get { return unitid_; }
        set { unitid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_unitid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_unitid() {
        unitid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int durablility = 2
    private int durablility_ = 0;
    public int durablility {
        get { return durablility_; }
        set { durablility_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_durablility() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_durablility() {
        durablility_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int armor = 3
    private int armor_ = 0;
    public int armor {
        get { return armor_; }
        set { armor_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_armor() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_armor() {
        armor_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int energy = 4
    private int energy_ = 0;
    public int energy {
        get { return energy_; }
        set { energy_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_energy() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_energy() {
        energy_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_unitid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, unitid );
        }
        if ( has_durablility() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, durablility );
        }
        if ( has_armor() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, armor );
        }
        if ( has_energy() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, energy );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                unitid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                durablility = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 24:
                armor = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                energy = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.UnitAttri" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        unitid_ = 0;
        durablility_ = 0;
        armor_ = 0;
        energy_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_unitid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( unitid );
        }
        if ( has_durablility() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( durablility );
        }
        if ( has_armor() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( armor );
        }
        if ( has_energy() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( energy );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "unitid: " );
        if ( has_unitid() ) 
            Console.WriteLine( unitid );
        Console.Write( "durablility: " );
        if ( has_durablility() ) 
            Console.WriteLine( durablility );
        Console.Write( "armor: " );
        if ( has_armor() ) 
            Console.WriteLine( armor );
        Console.Write( "energy: " );
        if ( has_energy() ) 
            Console.WriteLine( energy );
    }

}
}

public partial class proto {
public partial class UnitBehavior: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int unitid = 1
    private int unitid_ = 0;
    public int unitid {
        get { return unitid_; }
        set { unitid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_unitid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_unitid() {
        unitid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field proto.UnitPos position = 2
    private proto.UnitPos position_ = null;
    public proto.UnitPos position {
        get { return position_; }
        set { position_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_position() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_position() {
        position_ = null;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field proto.PartFireEvent partfireevent = 3
    private List<proto.PartFireEvent> partfireevent_ = null;
    public int partfireevent_size() { return partfireevent_ == null ? 0 : partfireevent_.Count; }
    public proto.PartFireEvent partfireevent( int index ) { return partfireevent_[index]; }
    public void add_partfireevent( proto.PartFireEvent val ) { 
        if ( partfireevent_ == null ) partfireevent_ = new List<proto.PartFireEvent>();
        partfireevent_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_partfireevent() { 
        if ( partfireevent_ != null ) partfireevent_.Clear();
        cached_byte_size = 0;
    }

    //field bool breakaway = 4
    private bool breakaway_ = false;
    public bool breakaway {
        get { return breakaway_; }
        set { breakaway_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_breakaway() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_breakaway() {
        breakaway_ = false;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        int list_count = 0;
        if ( has_unitid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, unitid );
        }
        if ( has_position() ) {
            __index += WriteInt( buf, __index, 18 );
            int size = position.ByteSize();
            __index += WriteInt( buf, __index, size );
            position.Serialize( buf, __index ); __index += size;
        }
        list_count = partfireevent_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 26 );
            int size = partfireevent( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            partfireevent( i ).Serialize( buf, __index ); __index += size;
        }
        if ( has_breakaway() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, breakaway ? 1 : 0 );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                unitid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                int position_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( position_size > msg_end-__index ) return false;
                position = new proto.UnitPos();
                if ( position_size > 0 ) {
                    if ( !position.Parse( buf, __index, position_size ) ) return false;
                    read_len = position_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 26:
                int partfireevent_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( partfireevent_size > msg_end-__index ) return false;
                proto.PartFireEvent partfireevent_tmp = new proto.PartFireEvent();
                add_partfireevent( partfireevent_tmp );
                if ( partfireevent_size > 0 ) {
                    if ( !partfireevent_tmp.Parse( buf, __index, partfireevent_size ) ) return false;
                    read_len = partfireevent_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 32:
                breakaway = ReadInt( buf, __index, ref read_len ) == 1;
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.UnitBehavior" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        unitid_ = 0;
        position_ = null;
        if ( partfireevent_ != null ) partfireevent_.Clear();
        breakaway_ = false;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        int list_count = 0;
        if ( has_unitid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( unitid );
        }
        if ( has_position() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = position.ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        list_count = partfireevent_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 26 );
            int size = partfireevent( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_breakaway() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( breakaway ? 1 : 0 );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x1 ) != 0x1 ) return false;
        return true;
    }

    public override void Print() {
        int list_count = 0;
        Console.Write( "unitid: " );
        if ( has_unitid() ) 
            Console.WriteLine( unitid );
        Console.Write( "position: " );
        if ( has_position() ) 
            position.Print();
        Console.Write( "partfireevent: " );
        list_count = partfireevent_size();
        for ( int i = 0; i < list_count; i++ )
            partfireevent( i ).Print();
        Console.WriteLine();
        Console.Write( "breakaway: " );
        if ( has_breakaway() ) 
            Console.WriteLine( breakaway );
    }

}
}

public partial class proto {
public partial class UnitPos: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int posx = 1
    private int posx_ = 0;
    public int posx {
        get { return posx_; }
        set { posx_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_posx() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_posx() {
        posx_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int posz = 2
    private int posz_ = 0;
    public int posz {
        get { return posz_; }
        set { posz_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_posz() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_posz() {
        posz_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int movetype = 3
    private int movetype_ = 0;
    public int movetype {
        get { return movetype_; }
        set { movetype_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_movetype() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_movetype() {
        movetype_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_posx() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, posx );
        }
        if ( has_posz() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, posz );
        }
        if ( has_movetype() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, movetype );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                posx = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                posz = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 24:
                movetype = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.UnitPos" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        posx_ = 0;
        posz_ = 0;
        movetype_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_posx() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( posx );
        }
        if ( has_posz() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( posz );
        }
        if ( has_movetype() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( movetype );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "posx: " );
        if ( has_posx() ) 
            Console.WriteLine( posx );
        Console.Write( "posz: " );
        if ( has_posz() ) 
            Console.WriteLine( posz );
        Console.Write( "movetype: " );
        if ( has_movetype() ) 
            Console.WriteLine( movetype );
    }

}
}

public partial class proto {
public partial class UnitReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;
    private uint has_flag_1;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field string unitclass = 3
    private string unitclass_ = "";
    public string unitclass {
        get { return unitclass_; }
        set { unitclass_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffb;
              else
                  has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_unitclass() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_unitclass() {
        unitclass_ = "";
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int lv_max = 4
    private int lv_max_ = 0;
    public int lv_max {
        get { return lv_max_; }
        set { lv_max_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_lv_max() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_lv_max() {
        lv_max_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int unitstrait = 5
    private int unitstrait_ = 0;
    public int unitstrait {
        get { return unitstrait_; }
        set { unitstrait_ = value; 
              has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_unitstrait() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_unitstrait() {
        unitstrait_ = 0;
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int unitsextendtrait = 6
    private int unitsextendtrait_ = 0;
    public int unitsextendtrait {
        get { return unitsextendtrait_; }
        set { unitsextendtrait_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_unitsextendtrait() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_unitsextendtrait() {
        unitsextendtrait_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field int unitstype = 7
    private int unitstype_ = 0;
    public int unitstype {
        get { return unitstype_; }
        set { unitstype_ = value; 
              has_flag_0 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_unitstype() {
        return ( has_flag_0 & 0x40 ) != 0;
    }
    public void clear_unitstype() {
        unitstype_ = 0;
        has_flag_0 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    //field int vol = 8
    private int vol_ = 0;
    public int vol {
        get { return vol_; }
        set { vol_ = value; 
              has_flag_0 |= 0x80;
              cached_byte_size = 0; }
    }
    public bool has_vol() {
        return ( has_flag_0 & 0x80 ) != 0;
    }
    public void clear_vol() {
        vol_ = 0;
        has_flag_0 &= 0xffffff7f;
        cached_byte_size = 0;
    }

    //field int hit_envelope_len = 9
    private int hit_envelope_len_ = 0;
    public int hit_envelope_len {
        get { return hit_envelope_len_; }
        set { hit_envelope_len_ = value; 
              has_flag_0 |= 0x100;
              cached_byte_size = 0; }
    }
    public bool has_hit_envelope_len() {
        return ( has_flag_0 & 0x100 ) != 0;
    }
    public void clear_hit_envelope_len() {
        hit_envelope_len_ = 0;
        has_flag_0 &= 0xfffffeff;
        cached_byte_size = 0;
    }

    //field int hit_envelope_wid = 10
    private int hit_envelope_wid_ = 0;
    public int hit_envelope_wid {
        get { return hit_envelope_wid_; }
        set { hit_envelope_wid_ = value; 
              has_flag_0 |= 0x200;
              cached_byte_size = 0; }
    }
    public bool has_hit_envelope_wid() {
        return ( has_flag_0 & 0x200 ) != 0;
    }
    public void clear_hit_envelope_wid() {
        hit_envelope_wid_ = 0;
        has_flag_0 &= 0xfffffdff;
        cached_byte_size = 0;
    }

    //field bool stack = 11
    private bool stack_ = false;
    public bool stack {
        get { return stack_; }
        set { stack_ = value; 
              has_flag_0 |= 0x400;
              cached_byte_size = 0; }
    }
    public bool has_stack() {
        return ( has_flag_0 & 0x400 ) != 0;
    }
    public void clear_stack() {
        stack_ = false;
        has_flag_0 &= 0xfffffbff;
        cached_byte_size = 0;
    }

    //field int stack_num = 12
    private int stack_num_ = 0;
    public int stack_num {
        get { return stack_num_; }
        set { stack_num_ = value; 
              has_flag_0 |= 0x800;
              cached_byte_size = 0; }
    }
    public bool has_stack_num() {
        return ( has_flag_0 & 0x800 ) != 0;
    }
    public void clear_stack_num() {
        stack_num_ = 0;
        has_flag_0 &= 0xfffff7ff;
        cached_byte_size = 0;
    }

    //field int warp_cost = 13
    private int warp_cost_ = 0;
    public int warp_cost {
        get { return warp_cost_; }
        set { warp_cost_ = value; 
              has_flag_0 |= 0x1000;
              cached_byte_size = 0; }
    }
    public bool has_warp_cost() {
        return ( has_flag_0 & 0x1000 ) != 0;
    }
    public void clear_warp_cost() {
        warp_cost_ = 0;
        has_flag_0 &= 0xffffefff;
        cached_byte_size = 0;
    }

    //field int durability = 14
    private int durability_ = 0;
    public int durability {
        get { return durability_; }
        set { durability_ = value; 
              has_flag_0 |= 0x2000;
              cached_byte_size = 0; }
    }
    public bool has_durability() {
        return ( has_flag_0 & 0x2000 ) != 0;
    }
    public void clear_durability() {
        durability_ = 0;
        has_flag_0 &= 0xffffdfff;
        cached_byte_size = 0;
    }

    //field int durability_growthrate = 15
    private int durability_growthrate_ = 0;
    public int durability_growthrate {
        get { return durability_growthrate_; }
        set { durability_growthrate_ = value; 
              has_flag_0 |= 0x4000;
              cached_byte_size = 0; }
    }
    public bool has_durability_growthrate() {
        return ( has_flag_0 & 0x4000 ) != 0;
    }
    public void clear_durability_growthrate() {
        durability_growthrate_ = 0;
        has_flag_0 &= 0xffffbfff;
        cached_byte_size = 0;
    }

    //field int armor = 16
    private int armor_ = 0;
    public int armor {
        get { return armor_; }
        set { armor_ = value; 
              has_flag_0 |= 0x8000;
              cached_byte_size = 0; }
    }
    public bool has_armor() {
        return ( has_flag_0 & 0x8000 ) != 0;
    }
    public void clear_armor() {
        armor_ = 0;
        has_flag_0 &= 0xffff7fff;
        cached_byte_size = 0;
    }

    //field int armory_growthrate = 17
    private int armory_growthrate_ = 0;
    public int armory_growthrate {
        get { return armory_growthrate_; }
        set { armory_growthrate_ = value; 
              has_flag_0 |= 0x10000;
              cached_byte_size = 0; }
    }
    public bool has_armory_growthrate() {
        return ( has_flag_0 & 0x10000 ) != 0;
    }
    public void clear_armory_growthrate() {
        armory_growthrate_ = 0;
        has_flag_0 &= 0xfffeffff;
        cached_byte_size = 0;
    }

    //field int energy = 18
    private int energy_ = 0;
    public int energy {
        get { return energy_; }
        set { energy_ = value; 
              has_flag_0 |= 0x20000;
              cached_byte_size = 0; }
    }
    public bool has_energy() {
        return ( has_flag_0 & 0x20000 ) != 0;
    }
    public void clear_energy() {
        energy_ = 0;
        has_flag_0 &= 0xfffdffff;
        cached_byte_size = 0;
    }

    //field int energy_growthrate = 19
    private int energy_growthrate_ = 0;
    public int energy_growthrate {
        get { return energy_growthrate_; }
        set { energy_growthrate_ = value; 
              has_flag_0 |= 0x40000;
              cached_byte_size = 0; }
    }
    public bool has_energy_growthrate() {
        return ( has_flag_0 & 0x40000 ) != 0;
    }
    public void clear_energy_growthrate() {
        energy_growthrate_ = 0;
        has_flag_0 &= 0xfffbffff;
        cached_byte_size = 0;
    }

    //field int shield_conversion_val = 20
    private int shield_conversion_val_ = 0;
    public int shield_conversion_val {
        get { return shield_conversion_val_; }
        set { shield_conversion_val_ = value; 
              has_flag_0 |= 0x80000;
              cached_byte_size = 0; }
    }
    public bool has_shield_conversion_val() {
        return ( has_flag_0 & 0x80000 ) != 0;
    }
    public void clear_shield_conversion_val() {
        shield_conversion_val_ = 0;
        has_flag_0 &= 0xfff7ffff;
        cached_byte_size = 0;
    }

    //field int shield_conversion_val_growthrate = 21
    private int shield_conversion_val_growthrate_ = 0;
    public int shield_conversion_val_growthrate {
        get { return shield_conversion_val_growthrate_; }
        set { shield_conversion_val_growthrate_ = value; 
              has_flag_0 |= 0x100000;
              cached_byte_size = 0; }
    }
    public bool has_shield_conversion_val_growthrate() {
        return ( has_flag_0 & 0x100000 ) != 0;
    }
    public void clear_shield_conversion_val_growthrate() {
        shield_conversion_val_growthrate_ = 0;
        has_flag_0 &= 0xffefffff;
        cached_byte_size = 0;
    }

    //field int shuttle_team = 22
    private int shuttle_team_ = 0;
    public int shuttle_team {
        get { return shuttle_team_; }
        set { shuttle_team_ = value; 
              has_flag_0 |= 0x200000;
              cached_byte_size = 0; }
    }
    public bool has_shuttle_team() {
        return ( has_flag_0 & 0x200000 ) != 0;
    }
    public void clear_shuttle_team() {
        shuttle_team_ = 0;
        has_flag_0 &= 0xffdfffff;
        cached_byte_size = 0;
    }

    //field int speed_max = 23
    private int speed_max_ = 0;
    public int speed_max {
        get { return speed_max_; }
        set { speed_max_ = value; 
              has_flag_0 |= 0x400000;
              cached_byte_size = 0; }
    }
    public bool has_speed_max() {
        return ( has_flag_0 & 0x400000 ) != 0;
    }
    public void clear_speed_max() {
        speed_max_ = 0;
        has_flag_0 &= 0xffbfffff;
        cached_byte_size = 0;
    }

    //field int acc_speed = 24
    private int acc_speed_ = 0;
    public int acc_speed {
        get { return acc_speed_; }
        set { acc_speed_ = value; 
              has_flag_0 |= 0x800000;
              cached_byte_size = 0; }
    }
    public bool has_acc_speed() {
        return ( has_flag_0 & 0x800000 ) != 0;
    }
    public void clear_acc_speed() {
        acc_speed_ = 0;
        has_flag_0 &= 0xff7fffff;
        cached_byte_size = 0;
    }

    //field int sw_speed = 25
    private int sw_speed_ = 0;
    public int sw_speed {
        get { return sw_speed_; }
        set { sw_speed_ = value; 
              has_flag_0 |= 0x1000000;
              cached_byte_size = 0; }
    }
    public bool has_sw_speed() {
        return ( has_flag_0 & 0x1000000 ) != 0;
    }
    public void clear_sw_speed() {
        sw_speed_ = 0;
        has_flag_0 &= 0xfeffffff;
        cached_byte_size = 0;
    }

    //field int sw_priority_unitstrait = 26
    private int sw_priority_unitstrait_ = 0;
    public int sw_priority_unitstrait {
        get { return sw_priority_unitstrait_; }
        set { sw_priority_unitstrait_ = value; 
              has_flag_0 |= 0x2000000;
              cached_byte_size = 0; }
    }
    public bool has_sw_priority_unitstrait() {
        return ( has_flag_0 & 0x2000000 ) != 0;
    }
    public void clear_sw_priority_unitstrait() {
        sw_priority_unitstrait_ = 0;
        has_flag_0 &= 0xfdffffff;
        cached_byte_size = 0;
    }

    //field int sw_targetdecision_range = 27
    private int sw_targetdecision_range_ = 0;
    public int sw_targetdecision_range {
        get { return sw_targetdecision_range_; }
        set { sw_targetdecision_range_ = value; 
              has_flag_0 |= 0x4000000;
              cached_byte_size = 0; }
    }
    public bool has_sw_targetdecision_range() {
        return ( has_flag_0 & 0x4000000 ) != 0;
    }
    public void clear_sw_targetdecision_range() {
        sw_targetdecision_range_ = 0;
        has_flag_0 &= 0xfbffffff;
        cached_byte_size = 0;
    }

    //field bool parts_hide = 28
    private bool parts_hide_ = false;
    public bool parts_hide {
        get { return parts_hide_; }
        set { parts_hide_ = value; 
              has_flag_0 |= 0x8000000;
              cached_byte_size = 0; }
    }
    public bool has_parts_hide() {
        return ( has_flag_0 & 0x8000000 ) != 0;
    }
    public void clear_parts_hide() {
        parts_hide_ = false;
        has_flag_0 &= 0xf7ffffff;
        cached_byte_size = 0;
    }

    //field int parts_1 = 29
    private int parts_1_ = 0;
    public int parts_1 {
        get { return parts_1_; }
        set { parts_1_ = value; 
              has_flag_0 |= 0x10000000;
              cached_byte_size = 0; }
    }
    public bool has_parts_1() {
        return ( has_flag_0 & 0x10000000 ) != 0;
    }
    public void clear_parts_1() {
        parts_1_ = 0;
        has_flag_0 &= 0xefffffff;
        cached_byte_size = 0;
    }

    //field int parts_1_rate = 30
    private int parts_1_rate_ = 0;
    public int parts_1_rate {
        get { return parts_1_rate_; }
        set { parts_1_rate_ = value; 
              has_flag_0 |= 0x20000000;
              cached_byte_size = 0; }
    }
    public bool has_parts_1_rate() {
        return ( has_flag_0 & 0x20000000 ) != 0;
    }
    public void clear_parts_1_rate() {
        parts_1_rate_ = 0;
        has_flag_0 &= 0xdfffffff;
        cached_byte_size = 0;
    }

    //field int parts_1_replace_team = 31
    private int parts_1_replace_team_ = 0;
    public int parts_1_replace_team {
        get { return parts_1_replace_team_; }
        set { parts_1_replace_team_ = value; 
              has_flag_0 |= 0x40000000;
              cached_byte_size = 0; }
    }
    public bool has_parts_1_replace_team() {
        return ( has_flag_0 & 0x40000000 ) != 0;
    }
    public void clear_parts_1_replace_team() {
        parts_1_replace_team_ = 0;
        has_flag_0 &= 0xbfffffff;
        cached_byte_size = 0;
    }

    //field int parts_2 = 32
    private int parts_2_ = 0;
    public int parts_2 {
        get { return parts_2_; }
        set { parts_2_ = value; 
              has_flag_0 |= 0x80000000;
              cached_byte_size = 0; }
    }
    public bool has_parts_2() {
        return ( has_flag_0 & 0x80000000 ) != 0;
    }
    public void clear_parts_2() {
        parts_2_ = 0;
        has_flag_0 &= 0x7fffffff;
        cached_byte_size = 0;
    }

    //field int parts_2_rate = 33
    private int parts_2_rate_ = 0;
    public int parts_2_rate {
        get { return parts_2_rate_; }
        set { parts_2_rate_ = value; 
              has_flag_1 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_parts_2_rate() {
        return ( has_flag_1 & 0x1 ) != 0;
    }
    public void clear_parts_2_rate() {
        parts_2_rate_ = 0;
        has_flag_1 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int parts_2_replace_team = 34
    private int parts_2_replace_team_ = 0;
    public int parts_2_replace_team {
        get { return parts_2_replace_team_; }
        set { parts_2_replace_team_ = value; 
              has_flag_1 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_parts_2_replace_team() {
        return ( has_flag_1 & 0x2 ) != 0;
    }
    public void clear_parts_2_replace_team() {
        parts_2_replace_team_ = 0;
        has_flag_1 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int parts_3 = 35
    private int parts_3_ = 0;
    public int parts_3 {
        get { return parts_3_; }
        set { parts_3_ = value; 
              has_flag_1 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_parts_3() {
        return ( has_flag_1 & 0x4 ) != 0;
    }
    public void clear_parts_3() {
        parts_3_ = 0;
        has_flag_1 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int parts_3_rate = 36
    private int parts_3_rate_ = 0;
    public int parts_3_rate {
        get { return parts_3_rate_; }
        set { parts_3_rate_ = value; 
              has_flag_1 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_parts_3_rate() {
        return ( has_flag_1 & 0x8 ) != 0;
    }
    public void clear_parts_3_rate() {
        parts_3_rate_ = 0;
        has_flag_1 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int parts_3_replace_team = 37
    private int parts_3_replace_team_ = 0;
    public int parts_3_replace_team {
        get { return parts_3_replace_team_; }
        set { parts_3_replace_team_ = value; 
              has_flag_1 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_parts_3_replace_team() {
        return ( has_flag_1 & 0x10 ) != 0;
    }
    public void clear_parts_3_replace_team() {
        parts_3_replace_team_ = 0;
        has_flag_1 &= 0xffffffef;
        cached_byte_size = 0;
    }

    //field int parts_4 = 38
    private int parts_4_ = 0;
    public int parts_4 {
        get { return parts_4_; }
        set { parts_4_ = value; 
              has_flag_1 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_parts_4() {
        return ( has_flag_1 & 0x20 ) != 0;
    }
    public void clear_parts_4() {
        parts_4_ = 0;
        has_flag_1 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field int parts_4_rate = 39
    private int parts_4_rate_ = 0;
    public int parts_4_rate {
        get { return parts_4_rate_; }
        set { parts_4_rate_ = value; 
              has_flag_1 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_parts_4_rate() {
        return ( has_flag_1 & 0x40 ) != 0;
    }
    public void clear_parts_4_rate() {
        parts_4_rate_ = 0;
        has_flag_1 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    //field int parts_4_replace_team = 40
    private int parts_4_replace_team_ = 0;
    public int parts_4_replace_team {
        get { return parts_4_replace_team_; }
        set { parts_4_replace_team_ = value; 
              has_flag_1 |= 0x80;
              cached_byte_size = 0; }
    }
    public bool has_parts_4_replace_team() {
        return ( has_flag_1 & 0x80 ) != 0;
    }
    public void clear_parts_4_replace_team() {
        parts_4_replace_team_ = 0;
        has_flag_1 &= 0xffffff7f;
        cached_byte_size = 0;
    }

    //field int parts_5 = 41
    private int parts_5_ = 0;
    public int parts_5 {
        get { return parts_5_; }
        set { parts_5_ = value; 
              has_flag_1 |= 0x100;
              cached_byte_size = 0; }
    }
    public bool has_parts_5() {
        return ( has_flag_1 & 0x100 ) != 0;
    }
    public void clear_parts_5() {
        parts_5_ = 0;
        has_flag_1 &= 0xfffffeff;
        cached_byte_size = 0;
    }

    //field int parts_5_rate = 42
    private int parts_5_rate_ = 0;
    public int parts_5_rate {
        get { return parts_5_rate_; }
        set { parts_5_rate_ = value; 
              has_flag_1 |= 0x200;
              cached_byte_size = 0; }
    }
    public bool has_parts_5_rate() {
        return ( has_flag_1 & 0x200 ) != 0;
    }
    public void clear_parts_5_rate() {
        parts_5_rate_ = 0;
        has_flag_1 &= 0xfffffdff;
        cached_byte_size = 0;
    }

    //field int parts_5_replace_team = 43
    private int parts_5_replace_team_ = 0;
    public int parts_5_replace_team {
        get { return parts_5_replace_team_; }
        set { parts_5_replace_team_ = value; 
              has_flag_1 |= 0x400;
              cached_byte_size = 0; }
    }
    public bool has_parts_5_replace_team() {
        return ( has_flag_1 & 0x400 ) != 0;
    }
    public void clear_parts_5_replace_team() {
        parts_5_replace_team_ = 0;
        has_flag_1 &= 0xfffffbff;
        cached_byte_size = 0;
    }

    //field int parts_6 = 44
    private int parts_6_ = 0;
    public int parts_6 {
        get { return parts_6_; }
        set { parts_6_ = value; 
              has_flag_1 |= 0x800;
              cached_byte_size = 0; }
    }
    public bool has_parts_6() {
        return ( has_flag_1 & 0x800 ) != 0;
    }
    public void clear_parts_6() {
        parts_6_ = 0;
        has_flag_1 &= 0xfffff7ff;
        cached_byte_size = 0;
    }

    //field int parts_6_rate = 45
    private int parts_6_rate_ = 0;
    public int parts_6_rate {
        get { return parts_6_rate_; }
        set { parts_6_rate_ = value; 
              has_flag_1 |= 0x1000;
              cached_byte_size = 0; }
    }
    public bool has_parts_6_rate() {
        return ( has_flag_1 & 0x1000 ) != 0;
    }
    public void clear_parts_6_rate() {
        parts_6_rate_ = 0;
        has_flag_1 &= 0xffffefff;
        cached_byte_size = 0;
    }

    //field int parts_6_replace_team = 46
    private int parts_6_replace_team_ = 0;
    public int parts_6_replace_team {
        get { return parts_6_replace_team_; }
        set { parts_6_replace_team_ = value; 
              has_flag_1 |= 0x2000;
              cached_byte_size = 0; }
    }
    public bool has_parts_6_replace_team() {
        return ( has_flag_1 & 0x2000 ) != 0;
    }
    public void clear_parts_6_replace_team() {
        parts_6_replace_team_ = 0;
        has_flag_1 &= 0xffffdfff;
        cached_byte_size = 0;
    }

    //field string model_res = 47
    private string model_res_ = "";
    public string model_res {
        get { return model_res_; }
        set { model_res_ = value; 
              if ( value == null )
                  has_flag_1 &= 0xffffbfff;
              else
                  has_flag_1 |= 0x4000;
              cached_byte_size = 0; }
    }
    public bool has_model_res() {
        return ( has_flag_1 & 0x4000 ) != 0;
    }
    public void clear_model_res() {
        model_res_ = "";
        has_flag_1 &= 0xffffbfff;
        cached_byte_size = 0;
    }

    //field string iconfile = 48
    private string iconfile_ = "";
    public string iconfile {
        get { return iconfile_; }
        set { iconfile_ = value; 
              if ( value == null )
                  has_flag_1 &= 0xffff7fff;
              else
                  has_flag_1 |= 0x8000;
              cached_byte_size = 0; }
    }
    public bool has_iconfile() {
        return ( has_flag_1 & 0x8000 ) != 0;
    }
    public void clear_iconfile() {
        iconfile_ = "";
        has_flag_1 &= 0xffff7fff;
        cached_byte_size = 0;
    }

    //field string note = 49
    private string note_ = "";
    public string note {
        get { return note_; }
        set { note_ = value; 
              if ( value == null )
                  has_flag_1 &= 0xfffeffff;
              else
                  has_flag_1 |= 0x10000;
              cached_byte_size = 0; }
    }
    public bool has_note() {
        return ( has_flag_1 & 0x10000 ) != 0;
    }
    public void clear_note() {
        note_ = "";
        has_flag_1 &= 0xfffeffff;
        cached_byte_size = 0;
    }

    //field int dc_initial = 50
    private int dc_initial_ = 0;
    public int dc_initial {
        get { return dc_initial_; }
        set { dc_initial_ = value; 
              has_flag_1 |= 0x20000;
              cached_byte_size = 0; }
    }
    public bool has_dc_initial() {
        return ( has_flag_1 & 0x20000 ) != 0;
    }
    public void clear_dc_initial() {
        dc_initial_ = 0;
        has_flag_1 &= 0xfffdffff;
        cached_byte_size = 0;
    }

    //field int sw_priority_unitstrait_extend = 51
    private int sw_priority_unitstrait_extend_ = 0;
    public int sw_priority_unitstrait_extend {
        get { return sw_priority_unitstrait_extend_; }
        set { sw_priority_unitstrait_extend_ = value; 
              has_flag_1 |= 0x40000;
              cached_byte_size = 0; }
    }
    public bool has_sw_priority_unitstrait_extend() {
        return ( has_flag_1 & 0x40000 ) != 0;
    }
    public void clear_sw_priority_unitstrait_extend() {
        sw_priority_unitstrait_extend_ = 0;
        has_flag_1 &= 0xfffbffff;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_unitclass() ) {
            __index += WriteInt( buf, __index, 26 );
            __index += WriteString( buf, buf_size-__index, __index, unitclass );
        }
        if ( has_lv_max() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, lv_max );
        }
        if ( has_unitstrait() ) {
            __index += WriteInt( buf, __index, 40 );
            __index += WriteInt( buf, __index, unitstrait );
        }
        if ( has_unitsextendtrait() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, unitsextendtrait );
        }
        if ( has_unitstype() ) {
            __index += WriteInt( buf, __index, 56 );
            __index += WriteInt( buf, __index, unitstype );
        }
        if ( has_vol() ) {
            __index += WriteInt( buf, __index, 64 );
            __index += WriteInt( buf, __index, vol );
        }
        if ( has_hit_envelope_len() ) {
            __index += WriteInt( buf, __index, 72 );
            __index += WriteInt( buf, __index, hit_envelope_len );
        }
        if ( has_hit_envelope_wid() ) {
            __index += WriteInt( buf, __index, 80 );
            __index += WriteInt( buf, __index, hit_envelope_wid );
        }
        if ( has_stack() ) {
            __index += WriteInt( buf, __index, 88 );
            __index += WriteInt( buf, __index, stack ? 1 : 0 );
        }
        if ( has_stack_num() ) {
            __index += WriteInt( buf, __index, 96 );
            __index += WriteInt( buf, __index, stack_num );
        }
        if ( has_warp_cost() ) {
            __index += WriteInt( buf, __index, 104 );
            __index += WriteInt( buf, __index, warp_cost );
        }
        if ( has_durability() ) {
            __index += WriteInt( buf, __index, 112 );
            __index += WriteInt( buf, __index, durability );
        }
        if ( has_durability_growthrate() ) {
            __index += WriteInt( buf, __index, 120 );
            __index += WriteInt( buf, __index, durability_growthrate );
        }
        if ( has_armor() ) {
            __index += WriteInt( buf, __index, 128 );
            __index += WriteInt( buf, __index, armor );
        }
        if ( has_armory_growthrate() ) {
            __index += WriteInt( buf, __index, 136 );
            __index += WriteInt( buf, __index, armory_growthrate );
        }
        if ( has_energy() ) {
            __index += WriteInt( buf, __index, 144 );
            __index += WriteInt( buf, __index, energy );
        }
        if ( has_energy_growthrate() ) {
            __index += WriteInt( buf, __index, 152 );
            __index += WriteInt( buf, __index, energy_growthrate );
        }
        if ( has_shield_conversion_val() ) {
            __index += WriteInt( buf, __index, 160 );
            __index += WriteInt( buf, __index, shield_conversion_val );
        }
        if ( has_shield_conversion_val_growthrate() ) {
            __index += WriteInt( buf, __index, 168 );
            __index += WriteInt( buf, __index, shield_conversion_val_growthrate );
        }
        if ( has_shuttle_team() ) {
            __index += WriteInt( buf, __index, 176 );
            __index += WriteInt( buf, __index, shuttle_team );
        }
        if ( has_speed_max() ) {
            __index += WriteInt( buf, __index, 184 );
            __index += WriteInt( buf, __index, speed_max );
        }
        if ( has_acc_speed() ) {
            __index += WriteInt( buf, __index, 192 );
            __index += WriteInt( buf, __index, acc_speed );
        }
        if ( has_sw_speed() ) {
            __index += WriteInt( buf, __index, 200 );
            __index += WriteInt( buf, __index, sw_speed );
        }
        if ( has_sw_priority_unitstrait() ) {
            __index += WriteInt( buf, __index, 208 );
            __index += WriteInt( buf, __index, sw_priority_unitstrait );
        }
        if ( has_sw_targetdecision_range() ) {
            __index += WriteInt( buf, __index, 216 );
            __index += WriteInt( buf, __index, sw_targetdecision_range );
        }
        if ( has_parts_hide() ) {
            __index += WriteInt( buf, __index, 224 );
            __index += WriteInt( buf, __index, parts_hide ? 1 : 0 );
        }
        if ( has_parts_1() ) {
            __index += WriteInt( buf, __index, 232 );
            __index += WriteInt( buf, __index, parts_1 );
        }
        if ( has_parts_1_rate() ) {
            __index += WriteInt( buf, __index, 240 );
            __index += WriteInt( buf, __index, parts_1_rate );
        }
        if ( has_parts_1_replace_team() ) {
            __index += WriteInt( buf, __index, 248 );
            __index += WriteInt( buf, __index, parts_1_replace_team );
        }
        if ( has_parts_2() ) {
            __index += WriteInt( buf, __index, 256 );
            __index += WriteInt( buf, __index, parts_2 );
        }
        if ( has_parts_2_rate() ) {
            __index += WriteInt( buf, __index, 264 );
            __index += WriteInt( buf, __index, parts_2_rate );
        }
        if ( has_parts_2_replace_team() ) {
            __index += WriteInt( buf, __index, 272 );
            __index += WriteInt( buf, __index, parts_2_replace_team );
        }
        if ( has_parts_3() ) {
            __index += WriteInt( buf, __index, 280 );
            __index += WriteInt( buf, __index, parts_3 );
        }
        if ( has_parts_3_rate() ) {
            __index += WriteInt( buf, __index, 288 );
            __index += WriteInt( buf, __index, parts_3_rate );
        }
        if ( has_parts_3_replace_team() ) {
            __index += WriteInt( buf, __index, 296 );
            __index += WriteInt( buf, __index, parts_3_replace_team );
        }
        if ( has_parts_4() ) {
            __index += WriteInt( buf, __index, 304 );
            __index += WriteInt( buf, __index, parts_4 );
        }
        if ( has_parts_4_rate() ) {
            __index += WriteInt( buf, __index, 312 );
            __index += WriteInt( buf, __index, parts_4_rate );
        }
        if ( has_parts_4_replace_team() ) {
            __index += WriteInt( buf, __index, 320 );
            __index += WriteInt( buf, __index, parts_4_replace_team );
        }
        if ( has_parts_5() ) {
            __index += WriteInt( buf, __index, 328 );
            __index += WriteInt( buf, __index, parts_5 );
        }
        if ( has_parts_5_rate() ) {
            __index += WriteInt( buf, __index, 336 );
            __index += WriteInt( buf, __index, parts_5_rate );
        }
        if ( has_parts_5_replace_team() ) {
            __index += WriteInt( buf, __index, 344 );
            __index += WriteInt( buf, __index, parts_5_replace_team );
        }
        if ( has_parts_6() ) {
            __index += WriteInt( buf, __index, 352 );
            __index += WriteInt( buf, __index, parts_6 );
        }
        if ( has_parts_6_rate() ) {
            __index += WriteInt( buf, __index, 360 );
            __index += WriteInt( buf, __index, parts_6_rate );
        }
        if ( has_parts_6_replace_team() ) {
            __index += WriteInt( buf, __index, 368 );
            __index += WriteInt( buf, __index, parts_6_replace_team );
        }
        if ( has_model_res() ) {
            __index += WriteInt( buf, __index, 378 );
            __index += WriteString( buf, buf_size-__index, __index, model_res );
        }
        if ( has_iconfile() ) {
            __index += WriteInt( buf, __index, 386 );
            __index += WriteString( buf, buf_size-__index, __index, iconfile );
        }
        if ( has_note() ) {
            __index += WriteInt( buf, __index, 394 );
            __index += WriteString( buf, buf_size-__index, __index, note );
        }
        if ( has_dc_initial() ) {
            __index += WriteInt( buf, __index, 400 );
            __index += WriteInt( buf, __index, dc_initial );
        }
        if ( has_sw_priority_unitstrait_extend() ) {
            __index += WriteInt( buf, __index, 408 );
            __index += WriteInt( buf, __index, sw_priority_unitstrait_extend );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 26:
                unitclass = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( unitclass == null ) return false;
                __index += read_len;
                break;
            case 32:
                lv_max = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 40:
                unitstrait = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 48:
                unitsextendtrait = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 56:
                unitstype = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 64:
                vol = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 72:
                hit_envelope_len = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 80:
                hit_envelope_wid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 88:
                stack = ReadInt( buf, __index, ref read_len ) == 1;
                __index += read_len;
                break;
            case 96:
                stack_num = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 104:
                warp_cost = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 112:
                durability = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 120:
                durability_growthrate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 128:
                armor = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 136:
                armory_growthrate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 144:
                energy = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 152:
                energy_growthrate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 160:
                shield_conversion_val = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 168:
                shield_conversion_val_growthrate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 176:
                shuttle_team = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 184:
                speed_max = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 192:
                acc_speed = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 200:
                sw_speed = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 208:
                sw_priority_unitstrait = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 216:
                sw_targetdecision_range = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 224:
                parts_hide = ReadInt( buf, __index, ref read_len ) == 1;
                __index += read_len;
                break;
            case 232:
                parts_1 = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 240:
                parts_1_rate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 248:
                parts_1_replace_team = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 256:
                parts_2 = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 264:
                parts_2_rate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 272:
                parts_2_replace_team = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 280:
                parts_3 = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 288:
                parts_3_rate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 296:
                parts_3_replace_team = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 304:
                parts_4 = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 312:
                parts_4_rate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 320:
                parts_4_replace_team = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 328:
                parts_5 = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 336:
                parts_5_rate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 344:
                parts_5_replace_team = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 352:
                parts_6 = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 360:
                parts_6_rate = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 368:
                parts_6_replace_team = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 378:
                model_res = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( model_res == null ) return false;
                __index += read_len;
                break;
            case 386:
                iconfile = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( iconfile == null ) return false;
                __index += read_len;
                break;
            case 394:
                note = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( note == null ) return false;
                __index += read_len;
                break;
            case 400:
                dc_initial = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 408:
                sw_priority_unitstrait_extend = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.UnitReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        unitclass_ = "";
        lv_max_ = 0;
        unitstrait_ = 0;
        unitsextendtrait_ = 0;
        unitstype_ = 0;
        vol_ = 0;
        hit_envelope_len_ = 0;
        hit_envelope_wid_ = 0;
        stack_ = false;
        stack_num_ = 0;
        warp_cost_ = 0;
        durability_ = 0;
        durability_growthrate_ = 0;
        armor_ = 0;
        armory_growthrate_ = 0;
        energy_ = 0;
        energy_growthrate_ = 0;
        shield_conversion_val_ = 0;
        shield_conversion_val_growthrate_ = 0;
        shuttle_team_ = 0;
        speed_max_ = 0;
        acc_speed_ = 0;
        sw_speed_ = 0;
        sw_priority_unitstrait_ = 0;
        sw_targetdecision_range_ = 0;
        parts_hide_ = false;
        parts_1_ = 0;
        parts_1_rate_ = 0;
        parts_1_replace_team_ = 0;
        parts_2_ = 0;
        parts_2_rate_ = 0;
        parts_2_replace_team_ = 0;
        parts_3_ = 0;
        parts_3_rate_ = 0;
        parts_3_replace_team_ = 0;
        parts_4_ = 0;
        parts_4_rate_ = 0;
        parts_4_replace_team_ = 0;
        parts_5_ = 0;
        parts_5_rate_ = 0;
        parts_5_replace_team_ = 0;
        parts_6_ = 0;
        parts_6_rate_ = 0;
        parts_6_replace_team_ = 0;
        model_res_ = "";
        iconfile_ = "";
        note_ = "";
        dc_initial_ = 0;
        sw_priority_unitstrait_extend_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
        has_flag_1 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_unitclass() ) {
            cached_byte_size += WriteIntSize( 26 );
            int size = WriteStringSize( unitclass );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_lv_max() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( lv_max );
        }
        if ( has_unitstrait() ) {
            cached_byte_size += WriteIntSize( 40 );
            cached_byte_size += WriteIntSize( unitstrait );
        }
        if ( has_unitsextendtrait() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( unitsextendtrait );
        }
        if ( has_unitstype() ) {
            cached_byte_size += WriteIntSize( 56 );
            cached_byte_size += WriteIntSize( unitstype );
        }
        if ( has_vol() ) {
            cached_byte_size += WriteIntSize( 64 );
            cached_byte_size += WriteIntSize( vol );
        }
        if ( has_hit_envelope_len() ) {
            cached_byte_size += WriteIntSize( 72 );
            cached_byte_size += WriteIntSize( hit_envelope_len );
        }
        if ( has_hit_envelope_wid() ) {
            cached_byte_size += WriteIntSize( 80 );
            cached_byte_size += WriteIntSize( hit_envelope_wid );
        }
        if ( has_stack() ) {
            cached_byte_size += WriteIntSize( 88 );
            cached_byte_size += WriteIntSize( stack ? 1 : 0 );
        }
        if ( has_stack_num() ) {
            cached_byte_size += WriteIntSize( 96 );
            cached_byte_size += WriteIntSize( stack_num );
        }
        if ( has_warp_cost() ) {
            cached_byte_size += WriteIntSize( 104 );
            cached_byte_size += WriteIntSize( warp_cost );
        }
        if ( has_durability() ) {
            cached_byte_size += WriteIntSize( 112 );
            cached_byte_size += WriteIntSize( durability );
        }
        if ( has_durability_growthrate() ) {
            cached_byte_size += WriteIntSize( 120 );
            cached_byte_size += WriteIntSize( durability_growthrate );
        }
        if ( has_armor() ) {
            cached_byte_size += WriteIntSize( 128 );
            cached_byte_size += WriteIntSize( armor );
        }
        if ( has_armory_growthrate() ) {
            cached_byte_size += WriteIntSize( 136 );
            cached_byte_size += WriteIntSize( armory_growthrate );
        }
        if ( has_energy() ) {
            cached_byte_size += WriteIntSize( 144 );
            cached_byte_size += WriteIntSize( energy );
        }
        if ( has_energy_growthrate() ) {
            cached_byte_size += WriteIntSize( 152 );
            cached_byte_size += WriteIntSize( energy_growthrate );
        }
        if ( has_shield_conversion_val() ) {
            cached_byte_size += WriteIntSize( 160 );
            cached_byte_size += WriteIntSize( shield_conversion_val );
        }
        if ( has_shield_conversion_val_growthrate() ) {
            cached_byte_size += WriteIntSize( 168 );
            cached_byte_size += WriteIntSize( shield_conversion_val_growthrate );
        }
        if ( has_shuttle_team() ) {
            cached_byte_size += WriteIntSize( 176 );
            cached_byte_size += WriteIntSize( shuttle_team );
        }
        if ( has_speed_max() ) {
            cached_byte_size += WriteIntSize( 184 );
            cached_byte_size += WriteIntSize( speed_max );
        }
        if ( has_acc_speed() ) {
            cached_byte_size += WriteIntSize( 192 );
            cached_byte_size += WriteIntSize( acc_speed );
        }
        if ( has_sw_speed() ) {
            cached_byte_size += WriteIntSize( 200 );
            cached_byte_size += WriteIntSize( sw_speed );
        }
        if ( has_sw_priority_unitstrait() ) {
            cached_byte_size += WriteIntSize( 208 );
            cached_byte_size += WriteIntSize( sw_priority_unitstrait );
        }
        if ( has_sw_targetdecision_range() ) {
            cached_byte_size += WriteIntSize( 216 );
            cached_byte_size += WriteIntSize( sw_targetdecision_range );
        }
        if ( has_parts_hide() ) {
            cached_byte_size += WriteIntSize( 224 );
            cached_byte_size += WriteIntSize( parts_hide ? 1 : 0 );
        }
        if ( has_parts_1() ) {
            cached_byte_size += WriteIntSize( 232 );
            cached_byte_size += WriteIntSize( parts_1 );
        }
        if ( has_parts_1_rate() ) {
            cached_byte_size += WriteIntSize( 240 );
            cached_byte_size += WriteIntSize( parts_1_rate );
        }
        if ( has_parts_1_replace_team() ) {
            cached_byte_size += WriteIntSize( 248 );
            cached_byte_size += WriteIntSize( parts_1_replace_team );
        }
        if ( has_parts_2() ) {
            cached_byte_size += WriteIntSize( 256 );
            cached_byte_size += WriteIntSize( parts_2 );
        }
        if ( has_parts_2_rate() ) {
            cached_byte_size += WriteIntSize( 264 );
            cached_byte_size += WriteIntSize( parts_2_rate );
        }
        if ( has_parts_2_replace_team() ) {
            cached_byte_size += WriteIntSize( 272 );
            cached_byte_size += WriteIntSize( parts_2_replace_team );
        }
        if ( has_parts_3() ) {
            cached_byte_size += WriteIntSize( 280 );
            cached_byte_size += WriteIntSize( parts_3 );
        }
        if ( has_parts_3_rate() ) {
            cached_byte_size += WriteIntSize( 288 );
            cached_byte_size += WriteIntSize( parts_3_rate );
        }
        if ( has_parts_3_replace_team() ) {
            cached_byte_size += WriteIntSize( 296 );
            cached_byte_size += WriteIntSize( parts_3_replace_team );
        }
        if ( has_parts_4() ) {
            cached_byte_size += WriteIntSize( 304 );
            cached_byte_size += WriteIntSize( parts_4 );
        }
        if ( has_parts_4_rate() ) {
            cached_byte_size += WriteIntSize( 312 );
            cached_byte_size += WriteIntSize( parts_4_rate );
        }
        if ( has_parts_4_replace_team() ) {
            cached_byte_size += WriteIntSize( 320 );
            cached_byte_size += WriteIntSize( parts_4_replace_team );
        }
        if ( has_parts_5() ) {
            cached_byte_size += WriteIntSize( 328 );
            cached_byte_size += WriteIntSize( parts_5 );
        }
        if ( has_parts_5_rate() ) {
            cached_byte_size += WriteIntSize( 336 );
            cached_byte_size += WriteIntSize( parts_5_rate );
        }
        if ( has_parts_5_replace_team() ) {
            cached_byte_size += WriteIntSize( 344 );
            cached_byte_size += WriteIntSize( parts_5_replace_team );
        }
        if ( has_parts_6() ) {
            cached_byte_size += WriteIntSize( 352 );
            cached_byte_size += WriteIntSize( parts_6 );
        }
        if ( has_parts_6_rate() ) {
            cached_byte_size += WriteIntSize( 360 );
            cached_byte_size += WriteIntSize( parts_6_rate );
        }
        if ( has_parts_6_replace_team() ) {
            cached_byte_size += WriteIntSize( 368 );
            cached_byte_size += WriteIntSize( parts_6_replace_team );
        }
        if ( has_model_res() ) {
            cached_byte_size += WriteIntSize( 378 );
            int size = WriteStringSize( model_res );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_iconfile() ) {
            cached_byte_size += WriteIntSize( 386 );
            int size = WriteStringSize( iconfile );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_note() ) {
            cached_byte_size += WriteIntSize( 394 );
            int size = WriteStringSize( note );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_dc_initial() ) {
            cached_byte_size += WriteIntSize( 400 );
            cached_byte_size += WriteIntSize( dc_initial );
        }
        if ( has_sw_priority_unitstrait_extend() ) {
            cached_byte_size += WriteIntSize( 408 );
            cached_byte_size += WriteIntSize( sw_priority_unitstrait_extend );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "unitclass: " );
        if ( has_unitclass() ) 
            Console.WriteLine( unitclass );
        Console.Write( "lv_max: " );
        if ( has_lv_max() ) 
            Console.WriteLine( lv_max );
        Console.Write( "unitstrait: " );
        if ( has_unitstrait() ) 
            Console.WriteLine( unitstrait );
        Console.Write( "unitsextendtrait: " );
        if ( has_unitsextendtrait() ) 
            Console.WriteLine( unitsextendtrait );
        Console.Write( "unitstype: " );
        if ( has_unitstype() ) 
            Console.WriteLine( unitstype );
        Console.Write( "vol: " );
        if ( has_vol() ) 
            Console.WriteLine( vol );
        Console.Write( "hit_envelope_len: " );
        if ( has_hit_envelope_len() ) 
            Console.WriteLine( hit_envelope_len );
        Console.Write( "hit_envelope_wid: " );
        if ( has_hit_envelope_wid() ) 
            Console.WriteLine( hit_envelope_wid );
        Console.Write( "stack: " );
        if ( has_stack() ) 
            Console.WriteLine( stack );
        Console.Write( "stack_num: " );
        if ( has_stack_num() ) 
            Console.WriteLine( stack_num );
        Console.Write( "warp_cost: " );
        if ( has_warp_cost() ) 
            Console.WriteLine( warp_cost );
        Console.Write( "durability: " );
        if ( has_durability() ) 
            Console.WriteLine( durability );
        Console.Write( "durability_growthrate: " );
        if ( has_durability_growthrate() ) 
            Console.WriteLine( durability_growthrate );
        Console.Write( "armor: " );
        if ( has_armor() ) 
            Console.WriteLine( armor );
        Console.Write( "armory_growthrate: " );
        if ( has_armory_growthrate() ) 
            Console.WriteLine( armory_growthrate );
        Console.Write( "energy: " );
        if ( has_energy() ) 
            Console.WriteLine( energy );
        Console.Write( "energy_growthrate: " );
        if ( has_energy_growthrate() ) 
            Console.WriteLine( energy_growthrate );
        Console.Write( "shield_conversion_val: " );
        if ( has_shield_conversion_val() ) 
            Console.WriteLine( shield_conversion_val );
        Console.Write( "shield_conversion_val_growthrate: " );
        if ( has_shield_conversion_val_growthrate() ) 
            Console.WriteLine( shield_conversion_val_growthrate );
        Console.Write( "shuttle_team: " );
        if ( has_shuttle_team() ) 
            Console.WriteLine( shuttle_team );
        Console.Write( "speed_max: " );
        if ( has_speed_max() ) 
            Console.WriteLine( speed_max );
        Console.Write( "acc_speed: " );
        if ( has_acc_speed() ) 
            Console.WriteLine( acc_speed );
        Console.Write( "sw_speed: " );
        if ( has_sw_speed() ) 
            Console.WriteLine( sw_speed );
        Console.Write( "sw_priority_unitstrait: " );
        if ( has_sw_priority_unitstrait() ) 
            Console.WriteLine( sw_priority_unitstrait );
        Console.Write( "sw_targetdecision_range: " );
        if ( has_sw_targetdecision_range() ) 
            Console.WriteLine( sw_targetdecision_range );
        Console.Write( "parts_hide: " );
        if ( has_parts_hide() ) 
            Console.WriteLine( parts_hide );
        Console.Write( "parts_1: " );
        if ( has_parts_1() ) 
            Console.WriteLine( parts_1 );
        Console.Write( "parts_1_rate: " );
        if ( has_parts_1_rate() ) 
            Console.WriteLine( parts_1_rate );
        Console.Write( "parts_1_replace_team: " );
        if ( has_parts_1_replace_team() ) 
            Console.WriteLine( parts_1_replace_team );
        Console.Write( "parts_2: " );
        if ( has_parts_2() ) 
            Console.WriteLine( parts_2 );
        Console.Write( "parts_2_rate: " );
        if ( has_parts_2_rate() ) 
            Console.WriteLine( parts_2_rate );
        Console.Write( "parts_2_replace_team: " );
        if ( has_parts_2_replace_team() ) 
            Console.WriteLine( parts_2_replace_team );
        Console.Write( "parts_3: " );
        if ( has_parts_3() ) 
            Console.WriteLine( parts_3 );
        Console.Write( "parts_3_rate: " );
        if ( has_parts_3_rate() ) 
            Console.WriteLine( parts_3_rate );
        Console.Write( "parts_3_replace_team: " );
        if ( has_parts_3_replace_team() ) 
            Console.WriteLine( parts_3_replace_team );
        Console.Write( "parts_4: " );
        if ( has_parts_4() ) 
            Console.WriteLine( parts_4 );
        Console.Write( "parts_4_rate: " );
        if ( has_parts_4_rate() ) 
            Console.WriteLine( parts_4_rate );
        Console.Write( "parts_4_replace_team: " );
        if ( has_parts_4_replace_team() ) 
            Console.WriteLine( parts_4_replace_team );
        Console.Write( "parts_5: " );
        if ( has_parts_5() ) 
            Console.WriteLine( parts_5 );
        Console.Write( "parts_5_rate: " );
        if ( has_parts_5_rate() ) 
            Console.WriteLine( parts_5_rate );
        Console.Write( "parts_5_replace_team: " );
        if ( has_parts_5_replace_team() ) 
            Console.WriteLine( parts_5_replace_team );
        Console.Write( "parts_6: " );
        if ( has_parts_6() ) 
            Console.WriteLine( parts_6 );
        Console.Write( "parts_6_rate: " );
        if ( has_parts_6_rate() ) 
            Console.WriteLine( parts_6_rate );
        Console.Write( "parts_6_replace_team: " );
        if ( has_parts_6_replace_team() ) 
            Console.WriteLine( parts_6_replace_team );
        Console.Write( "model_res: " );
        if ( has_model_res() ) 
            Console.WriteLine( model_res );
        Console.Write( "iconfile: " );
        if ( has_iconfile() ) 
            Console.WriteLine( iconfile );
        Console.Write( "note: " );
        if ( has_note() ) 
            Console.WriteLine( note );
        Console.Write( "dc_initial: " );
        if ( has_dc_initial() ) 
            Console.WriteLine( dc_initial );
        Console.Write( "sw_priority_unitstrait_extend: " );
        if ( has_sw_priority_unitstrait_extend() ) 
            Console.WriteLine( sw_priority_unitstrait_extend );
    }

}
}

public partial class proto {
public partial class UseSkill: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int unitid = 1
    private int unitid_ = 0;
    public int unitid {
        get { return unitid_; }
        set { unitid_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_unitid() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_unitid() {
        unitid_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field int skillid = 2
    private int skillid_ = 0;
    public int skillid {
        get { return skillid_; }
        set { skillid_ = value; 
              has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_skillid() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_skillid() {
        skillid_ = 0;
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int partid = 3
    private int partid_ = 0;
    public int partid {
        get { return partid_; }
        set { partid_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_partid() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_partid() {
        partid_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int skillstage = 4
    private int skillstage_ = 0;
    public int skillstage {
        get { return skillstage_; }
        set { skillstage_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_skillstage() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_skillstage() {
        skillstage_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field proto.PartFireInfo fireinfolist = 5
    private List<proto.PartFireInfo> fireinfolist_ = null;
    public int fireinfolist_size() { return fireinfolist_ == null ? 0 : fireinfolist_.Count; }
    public proto.PartFireInfo fireinfolist( int index ) { return fireinfolist_[index]; }
    public void add_fireinfolist( proto.PartFireInfo val ) { 
        if ( fireinfolist_ == null ) fireinfolist_ = new List<proto.PartFireInfo>();
        fireinfolist_.Add( val );
        cached_byte_size = 0;
    }
    public void clear_fireinfolist() { 
        if ( fireinfolist_ != null ) fireinfolist_.Clear();
        cached_byte_size = 0;
    }

    //field int posx = 6
    private int posx_ = 0;
    public int posx {
        get { return posx_; }
        set { posx_ = value; 
              has_flag_0 |= 0x20;
              cached_byte_size = 0; }
    }
    public bool has_posx() {
        return ( has_flag_0 & 0x20 ) != 0;
    }
    public void clear_posx() {
        posx_ = 0;
        has_flag_0 &= 0xffffffdf;
        cached_byte_size = 0;
    }

    //field int posy = 7
    private int posy_ = 0;
    public int posy {
        get { return posy_; }
        set { posy_ = value; 
              has_flag_0 |= 0x40;
              cached_byte_size = 0; }
    }
    public bool has_posy() {
        return ( has_flag_0 & 0x40 ) != 0;
    }
    public void clear_posy() {
        posy_ = 0;
        has_flag_0 &= 0xffffffbf;
        cached_byte_size = 0;
    }

    //field int posz = 8
    private int posz_ = 0;
    public int posz {
        get { return posz_; }
        set { posz_ = value; 
              has_flag_0 |= 0x80;
              cached_byte_size = 0; }
    }
    public bool has_posz() {
        return ( has_flag_0 & 0x80 ) != 0;
    }
    public void clear_posz() {
        posz_ = 0;
        has_flag_0 &= 0xffffff7f;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        int list_count = 0;
        if ( has_unitid() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, unitid );
        }
        if ( has_skillid() ) {
            __index += WriteInt( buf, __index, 16 );
            __index += WriteInt( buf, __index, skillid );
        }
        if ( has_partid() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, partid );
        }
        if ( has_skillstage() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, skillstage );
        }
        list_count = fireinfolist_size();
        for ( int i = 0; i < list_count; i++ ) {
            __index += WriteInt( buf, __index, 42 );
            int size = fireinfolist( i ).ByteSize();
            __index += WriteInt( buf, __index, size );
            fireinfolist( i ).Serialize( buf, __index ); __index += size;
        }
        if ( has_posx() ) {
            __index += WriteInt( buf, __index, 48 );
            __index += WriteInt( buf, __index, posx );
        }
        if ( has_posy() ) {
            __index += WriteInt( buf, __index, 56 );
            __index += WriteInt( buf, __index, posy );
        }
        if ( has_posz() ) {
            __index += WriteInt( buf, __index, 64 );
            __index += WriteInt( buf, __index, posz );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                unitid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 16:
                skillid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 24:
                partid = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                skillstage = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 42:
                int fireinfolist_size = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                if ( fireinfolist_size > msg_end-__index ) return false;
                proto.PartFireInfo fireinfolist_tmp = new proto.PartFireInfo();
                add_fireinfolist( fireinfolist_tmp );
                if ( fireinfolist_size > 0 ) {
                    if ( !fireinfolist_tmp.Parse( buf, __index, fireinfolist_size ) ) return false;
                    read_len = fireinfolist_size;
                } else read_len = 0;
                __index += read_len;
                break;
            case 48:
                posx = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 56:
                posy = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 64:
                posz = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.UseSkill" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        unitid_ = 0;
        skillid_ = 0;
        partid_ = 0;
        skillstage_ = 0;
        if ( fireinfolist_ != null ) fireinfolist_.Clear();
        posx_ = 0;
        posy_ = 0;
        posz_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        int list_count = 0;
        if ( has_unitid() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( unitid );
        }
        if ( has_skillid() ) {
            cached_byte_size += WriteIntSize( 16 );
            cached_byte_size += WriteIntSize( skillid );
        }
        if ( has_partid() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( partid );
        }
        if ( has_skillstage() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( skillstage );
        }
        list_count = fireinfolist_size();
        for ( int i = 0; i < list_count; i++ ) {
            cached_byte_size += WriteIntSize( 42 );
            int size = fireinfolist( i ).ByteSize();
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_posx() ) {
            cached_byte_size += WriteIntSize( 48 );
            cached_byte_size += WriteIntSize( posx );
        }
        if ( has_posy() ) {
            cached_byte_size += WriteIntSize( 56 );
            cached_byte_size += WriteIntSize( posy );
        }
        if ( has_posz() ) {
            cached_byte_size += WriteIntSize( 64 );
            cached_byte_size += WriteIntSize( posz );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0xf ) != 0xf ) return false;
        return true;
    }

    public override void Print() {
        int list_count = 0;
        Console.Write( "unitid: " );
        if ( has_unitid() ) 
            Console.WriteLine( unitid );
        Console.Write( "skillid: " );
        if ( has_skillid() ) 
            Console.WriteLine( skillid );
        Console.Write( "partid: " );
        if ( has_partid() ) 
            Console.WriteLine( partid );
        Console.Write( "skillstage: " );
        if ( has_skillstage() ) 
            Console.WriteLine( skillstage );
        Console.Write( "fireinfolist: " );
        list_count = fireinfolist_size();
        for ( int i = 0; i < list_count; i++ )
            fireinfolist( i ).Print();
        Console.WriteLine();
        Console.Write( "posx: " );
        if ( has_posx() ) 
            Console.WriteLine( posx );
        Console.Write( "posy: " );
        if ( has_posy() ) 
            Console.WriteLine( posy );
        Console.Write( "posz: " );
        if ( has_posz() ) 
            Console.WriteLine( posz );
    }

}
}

public partial class proto {
public partial class VictoryPointReference: ProtoMessage {
    public int cached_byte_size;
    private uint has_flag_0;

    //field int id = 1
    private int id_ = 0;
    public int id {
        get { return id_; }
        set { id_ = value; 
              has_flag_0 |= 0x1;
              cached_byte_size = 0; }
    }
    public bool has_id() {
        return ( has_flag_0 & 0x1 ) != 0;
    }
    public void clear_id() {
        id_ = 0;
        has_flag_0 &= 0xfffffffe;
        cached_byte_size = 0;
    }

    //field string name = 2
    private string name_ = "";
    public string name {
        get { return name_; }
        set { name_ = value; 
              if ( value == null )
                  has_flag_0 &= 0xfffffffd;
              else
                  has_flag_0 |= 0x2;
              cached_byte_size = 0; }
    }
    public bool has_name() {
        return ( has_flag_0 & 0x2 ) != 0;
    }
    public void clear_name() {
        name_ = "";
        has_flag_0 &= 0xfffffffd;
        cached_byte_size = 0;
    }

    //field int units_res_id = 3
    private int units_res_id_ = 0;
    public int units_res_id {
        get { return units_res_id_; }
        set { units_res_id_ = value; 
              has_flag_0 |= 0x4;
              cached_byte_size = 0; }
    }
    public bool has_units_res_id() {
        return ( has_flag_0 & 0x4 ) != 0;
    }
    public void clear_units_res_id() {
        units_res_id_ = 0;
        has_flag_0 &= 0xfffffffb;
        cached_byte_size = 0;
    }

    //field int defeat_points = 4
    private int defeat_points_ = 0;
    public int defeat_points {
        get { return defeat_points_; }
        set { defeat_points_ = value; 
              has_flag_0 |= 0x8;
              cached_byte_size = 0; }
    }
    public bool has_defeat_points() {
        return ( has_flag_0 & 0x8 ) != 0;
    }
    public void clear_defeat_points() {
        defeat_points_ = 0;
        has_flag_0 &= 0xfffffff7;
        cached_byte_size = 0;
    }

    //field int lose_points = 5
    private int lose_points_ = 0;
    public int lose_points {
        get { return lose_points_; }
        set { lose_points_ = value; 
              has_flag_0 |= 0x10;
              cached_byte_size = 0; }
    }
    public bool has_lose_points() {
        return ( has_flag_0 & 0x10 ) != 0;
    }
    public void clear_lose_points() {
        lose_points_ = 0;
        has_flag_0 &= 0xffffffef;
        cached_byte_size = 0;
    }

    public override bool Serialize( byte[] buf, int __index ) {
        int buf_size = buf.Length;
        int byte_size = ByteSize();
        if ( byte_size > buf_size ) {
            Console.WriteLine( "Serialize error, byte_size > buf_size " );
            return false;
        }

        if ( has_id() ) {
            __index += WriteInt( buf, __index, 8 );
            __index += WriteInt( buf, __index, id );
        }
        if ( has_name() ) {
            __index += WriteInt( buf, __index, 18 );
            __index += WriteString( buf, buf_size-__index, __index, name );
        }
        if ( has_units_res_id() ) {
            __index += WriteInt( buf, __index, 24 );
            __index += WriteInt( buf, __index, units_res_id );
        }
        if ( has_defeat_points() ) {
            __index += WriteInt( buf, __index, 32 );
            __index += WriteInt( buf, __index, defeat_points );
        }
        if ( has_lose_points() ) {
            __index += WriteInt( buf, __index, 40 );
            __index += WriteInt( buf, __index, lose_points );
        }
        return true;
    }

    public override bool Parse( byte[] buf, int __index, int msg_size ) {
        int msg_end = __index + msg_size;
        Clear();
        while ( __index < msg_end ) {
            int read_len = 0;
            int wire_tag = ReadInt( buf, __index, ref read_len ); __index += read_len;
            switch ( wire_tag ) {
            case 8:
                id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 18:
                name = ReadString( buf, msg_end-__index, __index, ref read_len );
                if ( name == null ) return false;
                __index += read_len;
                break;
            case 24:
                units_res_id = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 32:
                defeat_points = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            case 40:
                lose_points = ReadInt( buf, __index, ref read_len );
                __index += read_len;
                break;
            default: __index += GetUnknowFieldValueSize( buf, __index, wire_tag ); break;
            }
        }

        if ( !IsInitialized() ) {
            Console.WriteLine( "{0} parse error, miss required field", "proto.VictoryPointReference" );
            return false;
        }
        return true;
    }

    public override void Clear() {
        id_ = 0;
        name_ = "";
        units_res_id_ = 0;
        defeat_points_ = 0;
        lose_points_ = 0;
        cached_byte_size = 0;
        has_flag_0 = 0;
    }

    public override int ByteSize() {
        if ( cached_byte_size != 0 ) return cached_byte_size;
        if ( has_id() ) {
            cached_byte_size += WriteIntSize( 8 );
            cached_byte_size += WriteIntSize( id );
        }
        if ( has_name() ) {
            cached_byte_size += WriteIntSize( 18 );
            int size = WriteStringSize( name );
            cached_byte_size += WriteIntSize( size );
            cached_byte_size += size;
        }
        if ( has_units_res_id() ) {
            cached_byte_size += WriteIntSize( 24 );
            cached_byte_size += WriteIntSize( units_res_id );
        }
        if ( has_defeat_points() ) {
            cached_byte_size += WriteIntSize( 32 );
            cached_byte_size += WriteIntSize( defeat_points );
        }
        if ( has_lose_points() ) {
            cached_byte_size += WriteIntSize( 40 );
            cached_byte_size += WriteIntSize( lose_points );
        }
        return cached_byte_size;
    }

    public override bool IsInitialized() {
        if ( ( has_flag_0 & 0x3 ) != 0x3 ) return false;
        return true;
    }

    public override void Print() {
        Console.Write( "id: " );
        if ( has_id() ) 
            Console.WriteLine( id );
        Console.Write( "name: " );
        if ( has_name() ) 
            Console.WriteLine( name );
        Console.Write( "units_res_id: " );
        if ( has_units_res_id() ) 
            Console.WriteLine( units_res_id );
        Console.Write( "defeat_points: " );
        if ( has_defeat_points() ) 
            Console.WriteLine( defeat_points );
        Console.Write( "lose_points: " );
        if ( has_lose_points() ) 
            Console.WriteLine( lose_points );
    }

}
}

public partial class proto {
public partial class S2CFightReport {
public enum Result {
    Win = 1,
    Fail = 2,
    COMPLETE = 3,
}
}
}

public partial class proto {
public partial class S2CLogin {
public enum ErrorCode {
    OK = 1,
    InvalidAccountOrPassword = 2,
    LoginServerException = 3,
    TimeOut = 4,
    InvalidVersion = 5,
    Blocked = 6,
}
}
}

public partial class proto {
public partial class S2CSystem {
public enum Type {
    InvalidSession = 1,
    NotInGame = 2,
}
}
}

