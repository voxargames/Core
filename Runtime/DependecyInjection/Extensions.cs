using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VoxarGames.DependecyInjection
{
    public static class Extensions
    {
        public static void InjectServices(this IServiceProvider serviceProvider, object obj)
        {
            foreach (var property in GetPropertiesToInject(obj))
            {
                var service = serviceProvider.GetService(property.PropertyType);
                property.SetValue(obj, service);
            }

            foreach (var field in GetFieldsToInject(obj))
            {
                var service = serviceProvider.GetService(field.FieldType);
                field.SetValue(obj, service);
            }
        }

        public static void InjectServices(this object obj, IServiceProvider serviceProvider)
        {
            serviceProvider.InjectServices(obj);
        }

        public static T CreateInstance<T>(IServiceProvider serviceProvider, params object[] arg)
        {
            var type = typeof(T);
            return (T)ActivatorUtilities.CreateInstance(serviceProvider, type, arg);
        }

        public static object CreateInstance(IServiceProvider serviceProvider, Type type, params object[] arg)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, type, arg);
        }

        private static List<PropertyInfo> GetPropertiesToInject(object obj)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();

            Type objectType = obj.GetType();
            var props = objectType.GetRuntimeProperties();

            foreach (var property in props)
            {
                var attributes = property.GetCustomAttributes<InjectAttribute>();

                bool hasInjectAttribute = attributes != null && attributes.Any() ? true : false;

                if (hasInjectAttribute)
                {
                    properties.Add(property);
                }
            }

            return properties;
        }

        private static List<FieldInfo> GetFieldsToInject(object obj)
        {
            List<FieldInfo> fieldsIfo = new List<FieldInfo>();

            Type objectType = obj.GetType();
            var fields = objectType.GetRuntimeFields();

            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes<InjectAttribute>();
                bool hasInjectAttribute = attributes != null && attributes.Any() ? true : false;

                if (hasInjectAttribute)
                {
                    fieldsIfo.Add(field);
                }
            }

            return fieldsIfo;
        }

    }
}
