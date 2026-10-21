// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Reflection;

namespace Stride.Core.Reflection;

public static class CustomAttributeExtensions
{
    public static T? GetCustomAttributeEx<T>(this Assembly assembly) where T : Attribute
    {
        return assembly.GetCustomAttribute<T>();
    }

    public static Attribute? GetCustomAttributeEx(this Assembly assembly, Type attributeType)
    {
        return assembly.GetCustomAttribute(attributeType);
    }

    public static IEnumerable<Attribute> GetCustomAttributesEx(this Assembly assembly, Type attributeType)
    {
        return assembly.GetCustomAttributes(attributeType);
    }

    public static IEnumerable<T> GetCustomAttributesEx<T>(this Assembly assembly) where T : Attribute
    {
        return assembly.GetCustomAttributes<T>();
    }
}
