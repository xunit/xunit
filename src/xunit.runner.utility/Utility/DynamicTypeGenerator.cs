using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Xunit
{
    internal static class DynamicTypeGenerator
    {
        private static readonly ModuleBuilder _dynamicModule = CreateDynamicModule();

        private static ModuleBuilder CreateDynamicModule()
        {
            AssemblyBuilder dynamicAssembly =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName("xunit.runner.utility.{Dynamic}"),
                    AssemblyBuilderAccess.Run,
                    new CustomAttributeBuilder[0]
                );

            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("xunit.runner.utility.{Dynamic}.dll");
            return dynamicModule;
        }

        public static Type GenerateType(string dynamicTypeName, Type baseType, Type interfaceType)
        {
            TypeBuilder newType = _dynamicModule.DefineType(
                "Xunit.{Dynamic}." + dynamicTypeName,
                TypeAttributes.AutoLayout | TypeAttributes.Public | TypeAttributes.Class,
                baseType
            );

            newType.AddInterfaceImplementation(interfaceType);

            foreach (MethodInfo interfaceMethod in interfaceType.GetMethods())
                ImplementInterfaceMethod(newType, interfaceMethod);

            foreach (ConstructorInfo ctor in baseType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                switch (ctor.Attributes & MethodAttributes.MemberAccessMask)
                {
                    case MethodAttributes.Family:
                    case MethodAttributes.Public:
                    case MethodAttributes.FamORAssem:
                        ImplementConstructor(newType, ctor);
                        break;
                }

            return newType.CreateType();
        }

        private static void ImplementConstructor(TypeBuilder newType, ConstructorInfo baseCtor)
        {
            ParameterInfo[] parameters = baseCtor.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            for (int idx = 0; idx < parameters.Length; idx++)
                parameterTypes[idx] = parameters[idx].ParameterType;

            ConstructorBuilder newCtor = newType.DefineConstructor(
                (baseCtor.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public /* force public constructor */,
                baseCtor.CallingConvention, parameterTypes);

            for (int i = 0; i < parameters.Length; i++)
                newCtor.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);

            ILGenerator ilGen = newCtor.GetILGenerator();

            for (int i = 0; i <= parameterTypes.Length; i++)
                ilGen.Emit(OpCodes.Ldarg_S, (byte)i);

            ilGen.Emit(OpCodes.Call, baseCtor);
            ilGen.Emit(OpCodes.Ret);
        }

        private static void ImplementInterfaceMethod(TypeBuilder newType, MethodInfo interfaceMethod)
        {
            ParameterInfo[] parameters = interfaceMethod.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length];
            for (int idx = 0; idx < parameters.Length; idx++)
                parameterTypes[idx] = parameters[idx].ParameterType;

            MethodBuilder newMethod = newType.DefineMethod(
                interfaceMethod.DeclaringType.Name + "." + interfaceMethod.Name,
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                interfaceMethod.ReturnType, parameterTypes
            );

            MethodInfo baseMethod = newType.BaseType.GetMethod(interfaceMethod.Name, parameterTypes);

            for (int i = 0; i < parameters.Length; i++)
                newMethod.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);

            ILGenerator ilGen = newMethod.GetILGenerator();

            for (int i = 0; i <= parameterTypes.Length; i++)
                ilGen.Emit(OpCodes.Ldarg_S, (byte)i);

            ilGen.Emit(OpCodes.Call, baseMethod);
            ilGen.Emit(OpCodes.Ret);

            newType.DefineMethodOverride(newMethod, interfaceMethod);
        }
    }
}
