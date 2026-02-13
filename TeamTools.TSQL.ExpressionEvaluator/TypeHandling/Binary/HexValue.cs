using System;
using System.Globalization;
using System.Numerics;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // Length limit should be managed by SqlBinaryTypeValue/Handler
    public class HexValue : IComparable<HexValue>, IEquatable<HexValue>
    {
        private const string HexPrefix = "0x";
        private BigInteger asNumber;
        private string asString;

        public HexValue(int value, int minBytes = 0)
        {
            MinBytes = minBytes;
            AsNumber = value;
        }

        public HexValue(BigInteger value, int minBytes = 0)
        {
            MinBytes = minBytes;
            AsNumber = value;
        }

        public HexValue(string value, int minBytes = 0)
        {
            MinBytes = minBytes;
            AsString = value;
        }

        public BigInteger AsNumber
        {
            get
            {
                return asNumber;
            }

            set
            {
                asNumber = value;

                // Trimming because ToString sometimes prepends value with unexpected extra 0
                asString = value.ToString("X").TrimStart('0');

                // Prepending result with zeroes if MinLength is defined and the result is shorter
                if (MinLength > asString.Length)
                {
                    // FIXME: In case of BINARY(MAX) a huge string will be generated.
                    // A different approach for such cases is preferred.
                    asString = asString.PadLeft(MinLength, '0');
                }

                if (asString.Length % 2 != 0)
                {
                    // in SQL SERVER every byte is always represented by 2 symbols
                    asString = "0" + asString;
                }
            }
        }

        public string AsString
        {
            get
            {
                return asString;
            }

            set
            {
                if (value.StartsWith(HexPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (value.Length > 2)
                    {
                        value = value.Substring(2);
                    }
                    else
                    {
                        // there is only 0x prefix, no hex numbers afterwards
                        AsNumber = 0;
                        return;
                    }
                }

                // Leading zero to avoid -1 for 0xFF
                if (!BigInteger.TryParse("0" + value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var val))
                {
                    asString = default;
                    asNumber = default;
                }
                else
                {
                    // TODO : not sure if this is a correct behavior
                    // to keep leading zeroes if any
                    int providedBytes = (value.Length + 1) / 2;
                    if (MinBytes < providedBytes)
                    {
                        MinBytes = providedBytes;
                    }

                    // It will set AsString as well. With expected formatting.
                    AsNumber = val;
                }
            }
        }

        public int MinBytes { get; private set; }

        private int MinLength => MinBytes * 2;

        public static bool operator ==(HexValue left, HexValue right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(HexValue left, HexValue right)
        {
            return !(left == right);
        }

        public static bool operator <(HexValue left, HexValue right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(HexValue left, HexValue right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(HexValue left, HexValue right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(HexValue left, HexValue right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }

        public static HexValue operator +(HexValue left, HexValue right)
        {
            return new HexValue(left.AsString + right.AsString);
        }

        public static bool TryConvert(string src, out HexValue hex)
        {
            hex = new HexValue(src);
            return !string.IsNullOrEmpty(hex.AsString);
        }

        public int CompareTo(HexValue other) => this.asNumber.CompareTo(other?.asNumber);

        public bool Equals(HexValue other) => this.asNumber.Equals(other?.asNumber);

        public override string ToString() => HexPrefix + AsString;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj is HexValue hex)
            {
                return this.Equals(hex);
            }

            return false;
        }

        public override int GetHashCode() => asNumber.GetHashCode();
    }
}
