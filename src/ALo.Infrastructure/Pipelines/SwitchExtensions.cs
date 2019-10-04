using System;
using System.Linq;

namespace ALo.Infrastructure.Pipelines
{
    public static class SwitchExtensions
    {
        public static void Switch<TSource, TKey>(this TSource source, TKey key, params (TKey key, Action<TSource> action)[] actions)
        {
            var dictionary = actions.ToDictionary(k => k.key, v => v.action);
            dictionary[key](source);
        }
    }
}
