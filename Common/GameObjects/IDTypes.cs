using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    [Serializable]
    public struct MapIDType : IConvertible
    {
        private int _id;

        public int GetValue()
        {
            return _id;
        }

        public static explicit operator MapIDType(int i)
        {
            return new MapIDType() { _id = i };
        }

        public static explicit operator int(MapIDType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (int)(MapIDType)obj;
        }
        public static bool operator ==(MapIDType a, MapIDType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(MapIDType a, MapIDType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _id;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }
    [Serializable]
    public struct TerritoryIDType : IConvertible
    {
        private int _id;

        public int GetValue()
        {
            return _id;
        }

        public static explicit operator TerritoryIDType(int i)
        {
            return new TerritoryIDType() { _id = i };
        }

        public static explicit operator int(TerritoryIDType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (int)(TerritoryIDType)obj;
        }
        public static bool operator ==(TerritoryIDType a, TerritoryIDType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(TerritoryIDType a, TerritoryIDType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _id;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }
    [Serializable]
    public struct PlayerIDType : IConvertible
    {
        private int _id;

        public int GetValue()
        {
            return _id;
        }

        public static explicit operator PlayerIDType(int i)
        {
            return new PlayerIDType() { _id = i };
        }

        public static explicit operator int(PlayerIDType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (int)(PlayerIDType)obj;
        }
        public static bool operator ==(PlayerIDType a, PlayerIDType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(PlayerIDType a, PlayerIDType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _id;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }
    [Serializable]
    public struct GameIDType : IConvertible
    {
        private int _id;

        public int GetValue()
        {
            return _id;
        }

        public static explicit operator GameIDType(int i)
        {
            return new GameIDType() { _id = i };
        }

        public static explicit operator int(GameIDType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (int)(GameIDType)obj;
        }
        public static bool operator ==(GameIDType a, GameIDType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(GameIDType a, GameIDType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _id;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }
    [Serializable]
    public struct DistributionIDType : IConvertible
    {
        private int _id;

        public int GetValue()
        {
            return _id;
        }

        public static explicit operator DistributionIDType(int i)
        {
            return new DistributionIDType() { _id = i };
        }

        public static explicit operator int(DistributionIDType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (int)(DistributionIDType)obj;
        }
        public static bool operator ==(DistributionIDType a, DistributionIDType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(DistributionIDType a, DistributionIDType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _id;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }
    [Serializable]
    public struct BonusIDType : IConvertible
    {
        private int _id;

        public int GetValue()
        {
            return _id;
        }

        public static explicit operator BonusIDType(int i)
        {
            return new BonusIDType() { _id = i };
        }

        public static explicit operator int(BonusIDType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (int)(BonusIDType)obj;
        }
        public static bool operator ==(BonusIDType a, BonusIDType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(BonusIDType a, BonusIDType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _id;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }
    [Serializable]
    public struct SlotType : IConvertible
    {
        private int _id;

        public int GetValue()
        {
            return _id;
        }

        public static explicit operator SlotType(int i)
        {
            return new SlotType() { _id = i };
        }

        public static explicit operator int(SlotType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (int)(SlotType)obj;
        }
        public static bool operator ==(SlotType a, SlotType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(SlotType a, SlotType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _id;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }

    [Serializable]
    public struct TeamIDType : IConvertible
    {
        private int _id;

        public int GetValue()
        {
            return _id;
        }

        public static explicit operator TeamIDType(int i)
        {
            return new TeamIDType() { _id = i };
        }

        public static explicit operator int(TeamIDType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (int)(TeamIDType)obj;
        }
        public static bool operator ==(TeamIDType a, TeamIDType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(TeamIDType a, TeamIDType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _id;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }
    [Serializable]
    public struct CardInstanceIDType : IConvertible
    {
        private Guid _id;

        public Guid GetValue()
        {
            return _id;
        }

        public static explicit operator CardInstanceIDType(Guid i)
        {
            return new CardInstanceIDType() { _id = i };
        }

        public static explicit operator Guid(CardInstanceIDType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (Guid)(CardInstanceIDType)obj;
        }
        public static bool operator ==(CardInstanceIDType a, CardInstanceIDType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(CardInstanceIDType a, CardInstanceIDType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
            ;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }
    [Serializable]
    public struct CardIDType : IConvertible
    {
        private int _id;

        public int GetValue()
        {
            return _id;
        }

        public static explicit operator CardIDType(int i)
        {
            return new CardIDType() { _id = i };
        }

        public static explicit operator int(CardIDType a)
        {
            return a._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _id == (int)(CardIDType)obj;
        }
        public static bool operator ==(CardIDType a, CardIDType b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(CardIDType a, CardIDType b)
        {
            return a._id != b._id;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _id;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return _id;
        }
    }

}
