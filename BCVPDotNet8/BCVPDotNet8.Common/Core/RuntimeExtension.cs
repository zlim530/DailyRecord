﻿using Microsoft.Extensions.DependencyModel;
using Serilog;
using System.Reflection;
using System.Runtime.Loader;

namespace BCVPDotNet8.Common.Core
{
    public static class RuntimeExtension
    {
        /// <summary>
        /// 获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包
        /// </summary>
        /// <returns></returns>
        public static IList<Assembly> GetAssembylies()
        { 
            var list = new List<Assembly>();
            var deps = DependencyContext.Default;
            // 只加载项目中的程序集
            var libs = deps.CompileLibraries.Where(lib => !lib.Serviceable && lib.Type == "project");
            foreach (var lib in libs)
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
                    list.Add(assembly);
                }
                catch (Exception e)
                {
                    Log.Debug(e, "GetAllAssembies Exception:{ex}",e.Message);
                }
            }

            return list;
        }


        public static Assembly GetAssembly(string assemblyName)
        {
            return GetAssembylies().FirstOrDefault(assembly => assembly.FullName.Contains(assemblyName));
        }

        public static IList<Type> GetAllTypes()
        {
            var list = new List<Type>();
            foreach (var assembly in GetAssembylies())
            {
                var typeInfos = assembly.DefinedTypes;
                foreach (var typeInfo in typeInfos)
                {
                    list.Add(typeInfo.AsType());
                }
            }

            return list;
        }

        public static IList<Type> GetTypesByAssembly(string assemblyName)
        {
            var list = new List<Type>();
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
            var typeInfos = assembly.DefinedTypes;
            foreach (var typeInfo in typeInfos)
            {
                list.Add(typeInfo.AsType());
            }

            return list;
        }

        public static Type GetImplementType(string typeName, Type baseInterfaceType)
        {
            return GetAllTypes().FirstOrDefault(t =>
            {
                if (t.Name == typeName &&
                    t.GetTypeInfo().GetInterfaces().Any(b => b.Name == baseInterfaceType.Name))
                {
                    var typeInfo = t.GetTypeInfo();
                    return typeInfo.IsClass && !typeInfo.IsAbstract && !typeInfo.IsGenericType;
                }

                return false;
            });
        }

    }
}
