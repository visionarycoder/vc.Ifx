using System.Collections.Immutable;
using System.Reflection;
using OpenFeature.Model;

namespace Eyefinity.Utilities.FeatureFlagging.Helpers
{
    public static class DictionaryExtensions
    {
        public static T ToClass<T>(this IImmutableDictionary<string, Value> asDictionary) where T : new()
        {
            var type = typeof(T);
            var properties = type.GetProperties();

            var obj = new T();

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                if (asDictionary is null)
                {
                    return obj;
                }

                asDictionary.TryGetValue(propertyName, out var propertyValue);
               
                var setMethod = property.GetSetMethod();
                if (setMethod is null)
                {
                    continue;
                }

                SetPropertyValue(propertyValue, setMethod);
            }

            return obj;

            void SetPropertyValue(Value? propertyValue, MethodBase setMethod)
            {
                switch (propertyValue)
                {
                    case { AsBoolean: not null }:
                        setMethod.Invoke(obj, new object?[] { propertyValue.AsBoolean });
                        break;
                    case { AsInteger: not null }:
                        setMethod.Invoke(obj, new object?[] { propertyValue.AsInteger });
                        break;
                    case { AsDouble: not null }:
                        setMethod.Invoke(obj, new object?[] { propertyValue.AsDouble });
                        break;
                    case { AsString: not null }:
                        setMethod.Invoke(obj, new object?[] { propertyValue.AsString });
                        break;
                    case { AsDateTime: not null }:
                        setMethod.Invoke(obj, new object?[] { propertyValue.AsDateTime });
                        break;
                    case { AsList: not null }:
                        setMethod.Invoke(obj, new object?[] { propertyValue.AsList });
                        break;
                    case { AsStructure: not null }:
                        setMethod.Invoke(obj, new object?[] { propertyValue.AsStructure });
                        break;
                    case { AsObject: not null }:
                        setMethod.Invoke(obj, new object?[] { propertyValue.AsObject });
                        break;
                    default:
                        return;
                }
            }
        }
    }
}