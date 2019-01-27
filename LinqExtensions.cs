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
}
