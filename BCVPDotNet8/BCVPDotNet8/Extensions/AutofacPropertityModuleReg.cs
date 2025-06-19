﻿using Autofac;
using Microsoft.AspNetCore.Mvc;

namespace BCVPDotNet8.Extensions
{
    public class AutofacPropertityModuleReg : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var controllerBaseType = typeof(ControllerBase);
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
               .Where(t => controllerBaseType.IsAssignableFrom(t) && t != controllerBaseType)
               .PropertiesAutowired();// 属性注入
        }
    }
}
