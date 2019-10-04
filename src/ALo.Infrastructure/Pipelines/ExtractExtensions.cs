using System;

namespace ALo.Infrastructure.Pipelines
{
    public static class ExtractExtensions
    {
        public static TValue Extract<TSource, TValue>(this TSource source, Func<TSource, TValue> func) => func(source);
    }
}
