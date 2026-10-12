using System;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class UniqueErrorNumberOrStateRule : AbstractRule
    {
        private sealed class ErrInfo : IEquatable<ErrInfo>
        {
            public ErrInfo() { }

            public ErrInfo(int number, int state)
            {
                ErrorNumber = number;
                ErrorState = state;
            }

            public int ErrorNumber { get; set; }

            public int ErrorState { get; set; }

            public bool Equals(ErrInfo other)
            {
                if (other is null)
                {
                    return false;
                }

                return other.ErrorNumber == this.ErrorNumber
                    && other.ErrorState == this.ErrorState;
            }

            public override bool Equals(object obj)
            {
                if (obj is ErrInfo other)
                {
                    return other.Equals(this);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return (ErrorState * 1000) + ErrorNumber;
            }
        }
    }
}
