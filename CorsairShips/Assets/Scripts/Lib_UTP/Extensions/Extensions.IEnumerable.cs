using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static partial class Extensions {
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action) {
        foreach (var item in collection) {
            action(item);
        }

        return collection;
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection) {
        return collection == null || !collection.Any();
    }
}
