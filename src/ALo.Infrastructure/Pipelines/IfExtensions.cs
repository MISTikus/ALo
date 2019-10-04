using System;

namespace ALo.Infrastructure.Pipelines
{
    public static class IfExtensions
    {
        public static T If<T>(this T obj, bool condition, Action<T> action)
        {
            if (condition)
                action(obj);
            return obj;
        }
        public static T If<T>(this T obj, Func<T, bool> condition, Action<T> action)
        {
            if (condition(obj))
                action(obj);
            return obj;
        }
    }
}
