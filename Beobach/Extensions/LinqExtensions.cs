using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beobach.Extensions
{
    public static class LinqExtensions
    {
        public static IList<TResult> FullOuterGroupJoin<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> selectKeyLeft,
            Func<TRight, TKey> selectKeyRight,
            Func<IEnumerable<TLeft>, IEnumerable<TRight>, TKey, TResult> projection,
            IEqualityComparer<TKey> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;
            var alookup = left.ToLookup(selectKeyLeft, comparer);
            var blookup = right.ToLookup(selectKeyRight, comparer);

            var keys = new HashSet<TKey>(alookup.Select(p => p.Key), comparer);
            keys.UnionWith(blookup.Select(p => p.Key));

            var join = from key in keys
                let xa = alookup[key]
                let xb = blookup[key]
                select projection(xa, xb, key);

            return join.ToList();
        }

        public static IList<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> selectKeyLeft,
            Func<TRight, TKey> selectKeyRight,
            Func<TLeft, TRight, TKey, TResult> projection,
            TLeft defaultLeft = default(TLeft),
            TRight defaultRight = default(TRight),
            IEqualityComparer<TKey> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;
            var alookup = left.ToLookup(selectKeyLeft, comparer);
            var blookup = right.ToLookup(selectKeyRight, comparer);

            var keys = new HashSet<TKey>(alookup.Select(p => p.Key), comparer);
            keys.UnionWith(blookup.Select(p => p.Key));

            var join = from key in keys
                from xa in alookup[key].DefaultIfEmpty(defaultLeft)
                from xb in blookup[key].DefaultIfEmpty(defaultRight)
                select projection(xa, xb, key);

            return join.ToList();
        }
    }
}