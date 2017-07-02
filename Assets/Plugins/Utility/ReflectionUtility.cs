using System;
using System.Linq;
using System.Collections.Generic;

public static class ReflectionUtility
{
    public static Type FindType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);

            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    public static List<Type> FindAllConcreteDerivedTypes<T>()
    {
        return (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof(T).IsAssignableFrom(assemblyType) && !assemblyType.IsAbstract
                select assemblyType).ToList();
    }

    public static bool IsConcrete(Type type)
    {
        return type.IsClass && !type.IsAbstract && !type.IsInterface;
    }

    public static bool IsConcreteSubclass(Type type, Type derivedType)
    {
        return IsConcrete(derivedType) && type.IsAssignableFrom(derivedType);
    }
}
