using System;

namespace ALo.Infrastructure.Pipelines
{
    public static class DoExtensions
    {
        public static T Do<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }
    }
}
