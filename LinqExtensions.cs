using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public static class Extensions
{
    public static IEnumerable<IList<T>> ChunkOn<T>(this IEnumerable<T> source, Func<T, bool> startChunk)
    {
        List<T> list = new List<T>();

        foreach (var item in source)
        {
            if(startChunk(item) && list.Count > 0)
            {
                yield return list;
                list = new List<T>();
            }

            list.Add(item);
        }

        if(list.Count > 0)
        {
            yield return list;
        }
    }
      public static IEnumerable<IGrouping<TKey, TSource>> GroupAdjacent<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        TKey last = default(TKey);
        bool haveLast = false;
        List<TSource> list = new List<TSource>();
        foreach (TSource s in source)
        {
            TKey k = keySelector(s);
            if (haveLast)
            {
                if (!k.Equals(last))
                {
                    yield return new GroupOfAdjacent<TSource, TKey>(list, last);
                    list = new List<TSource>();
                    list.Add(s);
                    last = k;
                }
                else
                {
                    list.Add(s);
                    last = k;
                }
            }
            else
            {
                list.Add(s);
                last = k;
                haveLast = true;
            }
        }
        if (haveLast)
            yield return new GroupOfAdjacent<TSource, TKey>(list, last);
    }
    public class GroupOfAdjacent<TSource, TKey> : IEnumerable<TSource>, IGrouping<TKey, TSource>
{
    public TKey Key { get; set; }
    private List<TSource> GroupList { get; set; }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return ((System.Collections.Generic.IEnumerable<TSource>)this).GetEnumerator();
    }
    System.Collections.Generic.IEnumerator<TSource> System.Collections.Generic.IEnumerable<TSource>.GetEnumerator()
    {
        foreach (var s in GroupList)
            yield return s;
    }
    public GroupOfAdjacent(List<TSource> source, TKey key)
    {
        GroupList = source;
        Key = key;
    }
}
}
