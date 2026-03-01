using System.Collections.Concurrent;
using System.Reflection;

namespace Cits;

public static class DtoEntityComparer
{
    // 缓存类型属性信息
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    public static bool HasChanges<T>(T source, object? target)
    {
        if (source == null || target == null)
            return false;

        var sourceType = typeof(T);
        var targetType = target.GetType();

        // 获取源类型的所有可读属性
        var sourceProperties = PropertyCache.GetOrAdd(sourceType, t =>
            t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead).ToArray());

        foreach (var prop in sourceProperties)
        {
            // 查找目标对象的同名属性
            var targetProp = targetType.GetProperty(prop.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (targetProp == null || !targetProp.CanRead) continue;

            // 获取属性值并比较
            var val1 = prop.GetValue(source);
            var val2 = targetProp.GetValue(target);

            if (!AreEqual(val1, val2))
                return true;
        }

        return false;
    }

    private static bool AreEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Equals(b);
    }
}