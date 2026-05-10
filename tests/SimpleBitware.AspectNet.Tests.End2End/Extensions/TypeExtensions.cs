using System.Reflection;

namespace SimpleBitware.AspectNet.Tests.End2End.Extensions;

public static class TypeExtensions
{
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    
    public static Dictionary<string, object?> GetMethodParameters(this Type type, string methodName)
    {
        var method = type.GetMethod(methodName, Flags);
        if (method is not null)
        {
            return method.GetParameters()
                .Select(x=>x.Name)
                .Where(x => x is not null)
                .ToDictionary(x => x!, object? (_) => null);
        }

        var constructor = type.GetConstructor(Flags, []);
        if (constructor is not null)
        {
            return constructor.GetParameters()
                .Select(x=>x.Name)
                .Where(x => x is not null)
                .ToDictionary(x => x!, object? (_) => null);
        }

        var propertyMethod = type.GetProperty(methodName, Flags);
        if (propertyMethod is not null)
        {
            var getter = propertyMethod.GetGetMethod(true);
            var setter = propertyMethod.GetSetMethod(true);

            if (getter is not null)
                return new Dictionary<string, object?>();
            
            if(setter is not null)
                return setter.GetParameters()
                    .Select(x=>x.Name)
                    .Where(x => x is not null)
                    .ToDictionary(x => x!, object? (_) => null);
        }
        
        return new Dictionary<string, object?>();
    }
}
