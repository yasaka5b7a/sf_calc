
/*
 * 参照設定から参照の追加で.NETの「System.Numerics」を追加する。
 * メインフォームのusingに「System.Numerics」を追加する。
 */



namespace System.Numerics

{

    public struct Bigdecimal : IConvertible, IComparable<Bigdecimal>, IEquatable<Bigdecimal>
    {

        #region 定数

        private const int precision = 100;  //  桁数

        #endregion

        #region メンバ変数

        public static readonly Bigdecimal One = new Bigdecimal(BigInteger.One, 0);
        public static readonly Bigdecimal MinusOne = new Bigdecimal(BigInteger.MinusOne, 0);
        public static readonly Bigdecimal Zero = new Bigdecimal(BigInteger.Zero, 0);

        private readonly BigInteger _unscaledValue;
        private readonly int _scale;

        #endregion

        #region コンストラクタ

        public Bigdecimal(double value)
        {
            double x = value;
            int scale = 0;



            if (value == 0.0)
            {
                _unscaledValue = 0;
                _scale = 0;
                return;
            }


            while (Math.Truncate(x) != x)
            {
                x *= 10.0;
                scale++;
            }

            BigInteger unscaledValue = new BigInteger(x);

            _unscaledValue = unscaledValue;
            _scale = scale;
        }

        public Bigdecimal(float value) : this((double)value) { }
        public Bigdecimal(int value) : this(new BigInteger(value), 0) { }
        public Bigdecimal(long value) : this(new BigInteger(value), 0) { }
        public Bigdecimal(uint value) : this(new BigInteger(value), 0) { }
        public Bigdecimal(ulong value) : this(new BigInteger(value), 0) { }

        public Bigdecimal(decimal value)
        {
            var bytes = FromDecimalToBytes(value);
            var unscaledValueBytes = new byte[12];
            Array.Copy(bytes, unscaledValueBytes, unscaledValueBytes.Length);
            var unscaledValue = new BigInteger(unscaledValueBytes);
            var scale = bytes[14];



            if (bytes[15] == 128) unscaledValue *= BigInteger.MinusOne;

            _unscaledValue = unscaledValue;
            _scale = scale;
        }

        public Bigdecimal(BigInteger unscaledValue, int scale)
        {
            _unscaledValue = unscaledValue;
            _scale = scale;
        }

        public Bigdecimal(byte[] value)
        {
            byte[] number = new byte[value.Length - 4];
            byte[] flags = new byte[4];

            Array.Copy(value, 0, number, 0, number.Length);
            Array.Copy(value, value.Length - 4, flags, 0, 4);

            _unscaledValue = new BigInteger(number);
            _scale = BitConverter.ToInt32(flags, 0);
        }

        #endregion

        #region 演算子

        public static bool operator ==(Bigdecimal left, Bigdecimal right)
        {

            return left.Equals(right);

        }

        public static bool operator !=(Bigdecimal left, Bigdecimal right)
        {

            return !left.Equals(right);

        }

        public static bool operator >(Bigdecimal left, Bigdecimal right)
        {

            return left.CompareTo(right) > 0;

        }

        public static bool operator >=(Bigdecimal left, Bigdecimal right)
        {

            return left.CompareTo(right) >= 0;

        }

        public static bool operator <(Bigdecimal left, Bigdecimal right)
        {

            return left.CompareTo(right) < 0;

        }

        public static bool operator <=(Bigdecimal left, Bigdecimal right)
        {

            return left.CompareTo(right) <= 0;

        }

        public static bool operator ==(Bigdecimal left, decimal right)
        {

            return left.Equals(right);

        }

        public static bool operator !=(Bigdecimal left, decimal right)
        {

            return !left.Equals(right);

        }

        public static bool operator >(Bigdecimal left, decimal right)
        {

            return left.CompareTo(right) > 0;

        }

        public static bool operator >=(Bigdecimal left, decimal right)
        {

            return left.CompareTo(right) >= 0;

        }

        public static bool operator <(Bigdecimal left, decimal right)
        {

            return left.CompareTo(right) < 0;

        }

        public static bool operator <=(Bigdecimal left, decimal right)
        {

            return left.CompareTo(right) <= 0;

        }

        public static bool operator ==(decimal left, Bigdecimal right)
        {

            return left.Equals(right);

        }

        public static bool operator !=(decimal left, Bigdecimal right)
        {

            return !left.Equals(right);

        }

        public static bool operator >(decimal left, Bigdecimal right)
        {

            return left.CompareTo(right) > 0;

        }

        public static bool operator >=(decimal left, Bigdecimal right)
        {

            return left.CompareTo(right) >= 0;

        }

        public static bool operator <(decimal left, Bigdecimal right)
        {

            return left.CompareTo(right) < 0;

        }

        public static bool operator <=(decimal left, Bigdecimal right)
        {

            return left.CompareTo(right) <= 0;

        }


        public static Bigdecimal operator +(Bigdecimal left, Bigdecimal right)
        {
            BigInteger unscaledValue;
            int scale = 0;

            if (left._scale == right._scale)
            {
                scale = left._scale;
                unscaledValue = left._unscaledValue + right._unscaledValue;
            }
            else if (left._scale > right._scale)
            {
                scale = left._scale;
                var scaleDivisor = BigInteger.Pow(new BigInteger(10), scale - right._scale);
                unscaledValue = left._unscaledValue + right._unscaledValue * scaleDivisor;
            }
            else
            {
                scale = right._scale;
                var scaleDivisor = BigInteger.Pow(new BigInteger(10), scale - left._scale);
                unscaledValue = left._unscaledValue * scaleDivisor + right._unscaledValue;
            }

            return Rounding(new Bigdecimal(unscaledValue, scale));
        }

        public static Bigdecimal operator -(Bigdecimal left, Bigdecimal right)
        {
            BigInteger unscaledValue;
            int scale;

            if (left._scale == right._scale)
            {
                scale = left._scale;
                unscaledValue = left._unscaledValue - right._unscaledValue;
            }
            else if (left._scale > right._scale)
            {
                scale = left._scale;
                var scaleDivisor = BigInteger.Pow(new BigInteger(10), scale - right._scale);
                unscaledValue = left._unscaledValue - right._unscaledValue * scaleDivisor;
            }
            else
            {
                scale = right._scale;
                var scaleDivisor = BigInteger.Pow(new BigInteger(10), scale - left._scale);
                unscaledValue = left._unscaledValue * scaleDivisor - right._unscaledValue;
            }

            return Rounding(new Bigdecimal(unscaledValue, scale));
        }

        public static Bigdecimal operator *(Bigdecimal left, Bigdecimal right)
        {
            Bigdecimal ret;

            ret = new Bigdecimal(left._unscaledValue * right._unscaledValue, left._scale + right._scale);

            return Rounding(ret);
        }

        public static Bigdecimal operator /(Bigdecimal left, Bigdecimal right)
        {
            Bigdecimal ret;
            int upscale;
            BigInteger op1;

            if (right._scale > left._scale)
            {
                try
                {
                    upscale = right._scale - left._scale + precision;
                    op1 = left._unscaledValue * BigInteger.Pow(10, upscale);
                    ret = new Bigdecimal(op1 / right._unscaledValue, precision);

                    return Rounding(ret);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                try
                {

                    ret = new Bigdecimal(left._unscaledValue * BigInteger.Pow(10, precision) / right._unscaledValue, left._scale - right._scale + precision);

                    return Rounding(ret);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        #endregion

        #region 型変換

        public static explicit operator byte(Bigdecimal value) { return value.ToType<byte>(); }

        public static explicit operator sbyte(Bigdecimal value) { return value.ToType<sbyte>(); }

        public static explicit operator short(Bigdecimal value) { return value.ToType<short>(); }

        public static explicit operator int(Bigdecimal value) { return value.ToType<int>(); }

        public static explicit operator long(Bigdecimal value) { return value.ToType<long>(); }

        public static explicit operator ushort(Bigdecimal value) { return value.ToType<ushort>(); }

        public static explicit operator uint(Bigdecimal value) { return value.ToType<uint>(); }

        public static explicit operator ulong(Bigdecimal value) { return value.ToType<ulong>(); }

        public static explicit operator float(Bigdecimal value) { return value.ToType<float>(); }

        public static explicit operator double(Bigdecimal value) { return value.ToType<double>(); }

        public static explicit operator decimal(Bigdecimal value) { return value.ToType<decimal>(); }

        public static explicit operator BigInteger(Bigdecimal value)
        {

            var scaleDivisor = BigInteger.Pow(new BigInteger(10), value._scale);

            var scaledValue = BigInteger.Divide(value._unscaledValue, scaleDivisor);

            return scaledValue;

        }

        public static implicit operator Bigdecimal(byte value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(sbyte value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(short value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(int value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(long value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(ushort value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(uint value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(ulong value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(float value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(double value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(decimal value) { return new Bigdecimal(value); }

        public static implicit operator Bigdecimal(BigInteger value) { return new Bigdecimal(value, 0); }

        #endregion

        #region publicメソッド

        public bool IsEven { get { return _unscaledValue.IsEven; } }

        public bool IsOne { get { return _unscaledValue.IsOne; } }

        public bool IsPowerOfTwo { get { return _unscaledValue.IsPowerOfTwo; } }

        public bool IsZero { get { return _unscaledValue.IsZero; } }

        public int Sign { get { return _unscaledValue.Sign; } }

        public override string ToString()
        {

            var number = _unscaledValue.ToString("G");
            int keta = number.Length - _scale;
            if (Sign > 0)
            {
                if (_scale > 0)
                {

                    while (!(keta > 0))
                    {
                        keta++;
                        number = number.Insert(0, "0");
                    }
                    number = number.Insert(keta, ".");
                }
            }
            else
            {
                if (_scale > 0)
                {

                    while (!(keta > 1))
                    {
                        keta++;
                        number = number.Insert(1, "0");
                    }
                    number = number.Insert(keta, ".");
                }
            }

            return number;

        }

        public byte[] ToByteArray()
        {

            var unscaledValue = _unscaledValue.ToByteArray();
            var scale = BitConverter.GetBytes(_scale);
            var bytes = new byte[unscaledValue.Length + scale.Length];

            Array.Copy(unscaledValue, 0, bytes, 0, unscaledValue.Length);
            Array.Copy(scale, 0, bytes, unscaledValue.Length, scale.Length);

            return bytes;

        }

        public static Bigdecimal Rounding(Bigdecimal v)
        {

            if (v == Zero) return v;

            int m = (int)BigInteger.Log10(BigInteger.Abs(v._unscaledValue)) - precision;

            if (m > 0)
            {
                Bigdecimal ret = new Bigdecimal(v._unscaledValue / BigInteger.Pow(10, m), v._scale - m);
                return ret;
            }

            return v;

        }

        public T ToType<T>() where T : struct
        {

            return (T)((IConvertible)this).ToType(typeof(T), null);

        }

        public override bool Equals(object obj)
        {

            return (obj is Bigdecimal) && Equals((Bigdecimal)obj);

        }

        public override int GetHashCode()
        {

            return _unscaledValue.GetHashCode() ^ _scale.GetHashCode();

        }

        public static Bigdecimal Sqrt(Bigdecimal v)
        {
            Bigdecimal d = One;
            Bigdecimal ret = Zero;

            for (int i = 0; i < 10000; i++)
            {
                Bigdecimal R2 = ret * ret;
                if (v == R2) break;
                if (v > R2)
                {
                    ret += d;
                }
                else
                {
                    ret -= d;
                    d /= 10.0;
                }
            }
            return ret;
        }


        #endregion

        #region privateメソッド

        private static byte[] FromDecimalToBytes(decimal d)
        {

            byte[] bytes = new byte[16];
            int[] bits = decimal.GetBits(d);
            int low = bits[0];
            int mid = bits[1];
            int high = bits[2];
            int flags = bits[3];

            bytes[0] = (byte)low;
            bytes[1] = (byte)(low >> 8);
            bytes[2] = (byte)(low >> 0x10);
            bytes[3] = (byte)(low >> 0x18);
            bytes[4] = (byte)mid;
            bytes[5] = (byte)(mid >> 8);
            bytes[6] = (byte)(mid >> 0x10);
            bytes[7] = (byte)(mid >> 0x18);
            bytes[8] = (byte)high;
            bytes[9] = (byte)(high >> 8);
            bytes[10] = (byte)(high >> 0x10);
            bytes[11] = (byte)(high >> 0x18);
            bytes[12] = (byte)flags;
            bytes[13] = (byte)(flags >> 8);
            bytes[14] = (byte)(flags >> 0x10);
            bytes[15] = (byte)(flags >> 0x18);

            return bytes;

        }

        #endregion

        #region IConvertible メンバー

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (_scale >= 0)
            {
                var scaleDivisor = BigInteger.Pow(new BigInteger(10), _scale);
                var remainder = BigInteger.Remainder(_unscaledValue, scaleDivisor);
                var scaledValue = BigInteger.Divide(_unscaledValue, scaleDivisor);

                if (BigInteger.Abs(scaledValue) > new BigInteger(decimal.MaxValue))
                {
                    if (BigInteger.Abs(scaledValue) > new BigInteger(double.MaxValue) || conversionType != typeof(double))
                    {
                        throw new ArgumentOutOfRangeException("value", "The value " + _unscaledValue + " can not fit into " + conversionType.Name + ".");
                    }
                    var d_scaledValue = (double)_unscaledValue / (double)scaleDivisor;

                    return Convert.ChangeType(d_scaledValue, conversionType);
                }

                var leftOfDecimal = (decimal)scaledValue;
                var rightOfDecimal = decimal.Zero;

                if (scaleDivisor < new BigInteger(decimal.MaxValue))
                {
                    rightOfDecimal = ((decimal)remainder) / ((decimal)scaleDivisor);

                }
                else if (scaleDivisor < new BigInteger(double.MaxValue))
                {
                    double r1 = (double)remainder;
                    double r2 = (double)scaleDivisor;
                    double r3 = (double)leftOfDecimal;
                    return Convert.ChangeType(r3 + r1 / r2, conversionType);
                }

                var value = leftOfDecimal + rightOfDecimal;

                return Convert.ChangeType(value, conversionType);
            }
            else
            {
                var scaleDivisor = BigInteger.Pow(new BigInteger(10), -_scale);
                var scaledValue = _unscaledValue * scaleDivisor;

                if (scaledValue > new BigInteger(decimal.MaxValue))
                {
                    if (BigInteger.Abs(scaledValue) > new BigInteger(double.MaxValue) || conversionType != typeof(double))
                    {
                        throw new ArgumentOutOfRangeException("value", "The value " + _unscaledValue + " can not fit into " + conversionType.Name + ".");
                    }
                    var d_scaledValue = (double)scaledValue;

                    return Convert.ChangeType(d_scaledValue, conversionType);
                }
                var value2 = (decimal)scaledValue;

                return Convert.ChangeType(value2, conversionType);
            }

        }

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;

        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {

            return Convert.ToBoolean(this);

        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {

            return Convert.ToByte(this);

        }

        char IConvertible.ToChar(IFormatProvider provider)
        {

            throw new InvalidCastException("Can not cast Bigdecimal to Char");

        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {

            throw new InvalidCastException("Can not cast Bigdecimal to DateTime");

        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {

            return Convert.ToDecimal(this);

        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {

            return Convert.ToDouble(this);

        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {

            return Convert.ToInt16(this);

        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {

            return Convert.ToInt32(this);

        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {

            return Convert.ToInt64(this);

        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {

            return Convert.ToSByte(this);

        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {

            return Convert.ToSingle(this);

        }

        string IConvertible.ToString(IFormatProvider provider)
        {

            return Convert.ToString(this);

        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {

            return Convert.ToUInt16(this);

        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {

            return Convert.ToUInt32(this);

        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {

            return Convert.ToUInt64(this);

        }

        #endregion

        #region IComparable<BigDecimal> メンバー

        public int CompareTo(Bigdecimal other)
        {
            BigInteger unscaledValue;
            int scale = _scale;

            if (_scale == other._scale)
            {

                unscaledValue = _unscaledValue - other._unscaledValue;
            }
            else if (_scale > other._scale)
            {

                var scaleDivisor = BigInteger.Pow(new BigInteger(10), scale - other._scale);
                unscaledValue = _unscaledValue - other._unscaledValue * scaleDivisor;
            }
            else
            {
                scale = other._scale;
                var scaleDivisor = BigInteger.Pow(new BigInteger(10), scale - _scale);
                unscaledValue = _unscaledValue * scaleDivisor - other._unscaledValue;
            }
            return unscaledValue.Sign;

            
        }

        #endregion

        #region IEquatable<BigDecimal> メンバー

        public bool Equals(Bigdecimal other)
        {
            BigInteger unscaledValue;
            int scale = _scale;

            if (_scale == other._scale)
            {

                unscaledValue = _unscaledValue - other._unscaledValue;
            }
            else if (_scale > other._scale)
            {

                var scaleDivisor = BigInteger.Pow(new BigInteger(10), scale - other._scale);
                unscaledValue = _unscaledValue - other._unscaledValue * scaleDivisor;
            }
            else
            {
                scale = other._scale;
                var scaleDivisor = BigInteger.Pow(new BigInteger(10), scale - _scale);
                unscaledValue = _unscaledValue * scaleDivisor - other._unscaledValue;
            }

            return unscaledValue == BigInteger.Zero;

        }

        #endregion

    }

}

