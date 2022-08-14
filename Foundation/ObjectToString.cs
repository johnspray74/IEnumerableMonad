using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Library;

namespace Foundation
{
    public static partial class FoundationExtensionMethods
    {
        // override ToString for reference objects to return the class's property's values as well as the class name
        public static string ClassToString<T>(this T instance) where T : class
        {
            if (instance == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (instance is ExpandoObject)
            {
                var dic = (IDictionary<string, object>)instance;
                sb.Append(dic.Select(kvp => kvp.Key.ToString() + "=" + kvp.Value.ToString()).Join(", ")); 
            }
            else
            {
                try
                {
                    var type = instance.GetType();

                    var typeName = type.Name;
                    sb.Append(typeName);
                    sb.Append(" ");
                    if (instance.GetType().IsGenericType) sb.Append(" (Generic)");

                    sb.Append("{");

                    var strListType = typeof(List<string>);
                    var strArrType = typeof(string[]);

                    var arrayTypes = new[] { strListType, strArrType };
                    var handledTypes = new[] { typeof(Int32), typeof(String), typeof(bool), typeof(DateTime), typeof(double), typeof(decimal), strListType, strArrType };

                    var propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    bool first = true;
                    foreach (var propertyInfo in propertyInfos)
                    {
                        if (!first) sb.Append(", ");
                        first = false;
                        sb.Append(propertyInfo.Name + "=");
                        try
                        {
                            if (propertyInfo.GetValue(instance, null) == null)
                            {
                                sb.Append("null");
                            }
                            else
                            {
                                var s = arrayTypes.Contains(propertyInfo.PropertyType)
                                        ? string.Join(", ", (IEnumerable<string>)propertyInfo.GetValue(instance, null))
                                        : propertyInfo.GetValue(instance, null);
                                sb.Append(s);
                            }
                        }
                        catch
                        {
                            sb.Append("Exception getting value");
                        }
                    }
                    sb.Append("}");
                }
                catch (Exception e)
                {
                    sb.AppendLine("Exception in ToString<T>() where T : class {e}");
                }
            }
            return sb.ToString();
        }
    }
}
