using System.Diagnostics;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Routines
{
    public sealed class NumeronimBuilder
    {
        private readonly string delim;
        private readonly int delimLength;
        private readonly int maxValuesUntilNumeronim;
        private readonly int minStartValuesIfNumeronim;

        public NumeronimBuilder(string delim, int maxValuesUntilNumeronim, int minStartValuesIfNumeronim)
        {
            this.delim = delim;
            this.delimLength = delim.Length;
            this.maxValuesUntilNumeronim = maxValuesUntilNumeronim;
            this.minStartValuesIfNumeronim = minStartValuesIfNumeronim;
        }

        public string Build(string[] items)
        {
            var result = ObjectPools.StringBuilderPool.Get();
            int totalItemCount = items.Length;
            int n = totalItemCount > maxValuesUntilNumeronim ? minStartValuesIfNumeronim : totalItemCount;
            int includedItemCount = 0;
            int numeronimLength = 0;

            foreach (var item in items)
            {
                includedItemCount++;
                Debug.Assert(item != "", "item is empty");

                if (includedItemCount <= n)
                {
                    // starting with full names
                    result
                        .Append(delim)
                        .Append(item);
                }
                else if (includedItemCount == n + 1)
                {
                    // from the first item in numeronim we take the first char
                    result
                        .Append(delim)
                        .Append(item[0]);

                    numeronimLength += item.Length - 1;
                }
                else if (includedItemCount < totalItemCount)
                {
                    // counting numeronim length
                    numeronimLength += delimLength + item.Length;
                }
                else
                {
                    // from the last item in numeronim we take the last char
                    numeronimLength += delimLength + item.Length - 1;
                    result
                        .Append(numeronimLength)
                        .Append(item[item.Length - 1]);
                }
            }

            var res = result.ToString();
            ObjectPools.StringBuilderPool.Return(result);
            return res;
        }
    }
}
