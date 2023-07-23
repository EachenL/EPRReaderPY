using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;
using EasyPathology.Core.Extensions;

namespace Server;

public static class Converter {
    private static bool HasAttribute<T>(this PropertyInfo property) where T : Attribute {
        return property.GetCustomAttributes(typeof(T), true).Length > 0;
    }

    private static readonly Dictionary<Type, Type> TypeCache = new();

    /// <summary>
    /// 根据原始类型找pb类型，如果原始类型是pb类型则返回原始类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static Type? GetCorrespondingType(Type type) {
        if (type.Namespace == null) return null;
        if (TypeCache.TryGetValue(type, out var other)) return other;

        var ns = type.Namespace.Split('.');
        if (ns.Length < 2) return null;

        switch (ns[0]) {
            case "EasyPathologyPb": {
                ns[0] = "EasyPathology";
                other = ns[1] switch {
                    "Definitions" => typeof(EasyPathology.Definitions.DataTypes.Point2D).Assembly.GetType(
                        string.Join('.', ns) + "." + type.Name),
                    "Record" => typeof(EasyPathology.Record.EasyPathologyRecord).Assembly.GetType(
                        string.Join('.', ns) + "." + type.Name),
                    _ => other
                };
                break;
            }
            case "EasyPathology": {
                ns[0] = "EasyPathologyPb";
                other = typeof(EasyPathologyPb.Record.EasyPathologyRecord).Assembly.GetType(
                    string.Join('.', ns) + "." + type.Name);
                break;
            }
        }

        if (other != null) TypeCache.Add(type, other);
        return other;
    }

    private static List<ValueTuple<PropertyInfo, PropertyInfo>> MakeMapList(Type fromType, Type toType) {
        return fromType.GetProperties().Where(
                p => p.CanRead && !p.HasAttribute<JsonIgnoreAttribute>())
            .Select(p => new ValueTuple<PropertyInfo, PropertyInfo?>(
                p, toType.GetProperty(p.Name)))
            .Where(p => p.Item2 is { CanWrite: true })
            .Cast<ValueTuple<PropertyInfo, PropertyInfo>>().ToList();
    }

    private static readonly Dictionary<Type, List<ValueTuple<PropertyInfo, PropertyInfo>>> TypeListCache = new();

    private static bool IsIListType(Type type) {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>);
    }

    private static Type GetGenericIListType(Type type) {
        return type.GetGenericArguments()[0];
    }

    private static void CopyTo(object src, object dst) {
        var srcType = src.GetType();
        var dstType = dst.GetType();
        if (IsIListType(srcType)) {  // 拷贝列表
            var srcGenericType = GetGenericIListType(srcType);
            var dstGenericType = GetGenericIListType(dstType);
            if (srcGenericType != dstGenericType) return;
                
            var clearMethod = dstType.GetMethod("Clear").NotNull();
            clearMethod.Invoke(dst, null);

            var addMethod = dstType.GetMethod("Add").NotNull();
            foreach (var item in (IEnumerable)src) {
                addMethod.Invoke(dst, new[] { item });
            }

            return;
        }

        if (!TypeListCache.TryGetValue(srcType, out var mapList)) {
            mapList = MakeMapList(srcType, dst.GetType());
            TypeListCache.Add(srcType, mapList);
        }

        foreach (var (srcProp, dstProp) in mapList) {
            if (srcProp.PropertyType == dstProp.PropertyType) {
                dstProp.SetValue(dst, srcProp.GetValue(src));
            } else {
                // 两者类型不同，说明一个是pb类型，一个是原始类型
                var srcValue = srcProp.GetValue(src);
                var dstValue = dstProp.GetValue(dst);
                if (srcValue == null) {
                    if (dstValue != null) {
                        dstProp.SetValue(dst, null);
                    }

                    continue;
                }

                if (dstValue == null) {
                    dstValue = Activator.CreateInstance(dstProp.PropertyType).NotNull();
                    dstProp.SetValue(dst, dstValue);
                }

                CopyTo(srcValue, dstValue);
            }
        }
    }

    public static object? Convert(this object record) {
        var other = GetCorrespondingType(record.GetType());
        if (other == null) return null;
        CopyTo(record, other);
        return other;
    }
}