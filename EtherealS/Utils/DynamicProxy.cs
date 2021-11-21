using EtherealS.Core.Model;
using EtherealS.Request.Attribute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace EtherealS.Utils
{
    public class DynamicProxy
    {
        //代码参考 https://gitee.com/code2roc/FastIOC/blob/master/FastIOC/Proxy/DynamictProxy.cs
        public static T CreateRequestProxy<T>() where T : Request.Abstract.Request
        {
            Type ImpType = typeof(T);
            string nameOfAssembly = ImpType.Name + "ProxyAssembly";
            string nameOfModule = ImpType.Name + "ProxyModule";
            string nameOfType = ImpType.Name + "Proxy";
            var assemblyName = new AssemblyName(nameOfAssembly);
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assembly.DefineDynamicModule(nameOfModule);
            TypeBuilder typeBuilder;
            typeBuilder = moduleBuilder.DefineType(nameOfType, TypeAttributes.Public, ImpType);
            InjectInterceptor(typeBuilder, ImpType);
            Type t = typeBuilder.CreateType();
            return Activator.CreateInstance(t) as T;
        }
        private static void InjectInterceptor(TypeBuilder typeBuilder, Type ImpType)
        {
            // ---- 方法定义 ----
            foreach (var method in ImpType.GetMethods())
            {
                RequestMapping attribute = method.GetCustomAttribute<RequestMapping>();
                if (attribute == null) continue;
                else if (attribute.Mapping == null) throw new TrackException(TrackException.ErrorCode.Runtime, $"{ImpType.FullName}-{method.Name}的Mapping未赋值！");
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                // ---- 定义方法名与参数 ----
                MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final;
                var methodBuilder = typeBuilder.DefineMethod(method.Name, methodAttributes, CallingConventions.Standard, method.ReturnType, methodParameterTypes);
                //如果是泛型方法
                if (method.IsGenericMethod)
                {
                    //获取所有泛型参数类型定义
                    Type[] Args = method.GetGenericArguments();
                    List<string> GenericArgNames = new List<string>();
                    for (int j = 0; j < Args.Length; j++)
                    {
                        GenericArgNames.Add(Args[j].Name);
                    }
                    //代理类方法生成泛型参数定义
                    GenericTypeParameterBuilder[] DGP = methodBuilder.DefineGenericParameters(GenericArgNames.ToArray());
                    //泛型参数约束设置
                    for (int j = 0; j < DGP.Length; j++)
                    {
                        //泛型参数继承约束
                        DGP[j].SetBaseTypeConstraint(Args[j].BaseType);
                        //泛型参数完成接口约束
                        DGP[j].SetInterfaceConstraints(Args[j].GetInterfaces());
                    }
                }

                var ilOfMethod = methodBuilder.GetILGenerator();
                #region - Local -
                var localResult = ilOfMethod.DeclareLocal(typeof(object));
                ilOfMethod.Emit(OpCodes.Ldnull);
                ilOfMethod.Emit(OpCodes.Stloc, localResult);
                if (attribute.InvokeType.HasFlag(RequestMapping.InvokeTypeFlags.Local))
                {
                    //注入参数
                    ilOfMethod.Emit(OpCodes.Ldarg_0);
                    for (var j = 0; j < methodParameterTypes.Length; j++)
                    {
                        ilOfMethod.Emit(OpCodes.Ldarg, j + 1);
                    }
                    //调用本地方法
                    ilOfMethod.Emit(OpCodes.Call, method);
                    if (method.ReturnType != typeof(void))
                    {
                        if (method.ReturnType.IsValueType)
                        {
                            ilOfMethod.Emit(OpCodes.Box, method.ReturnType);
                        }
                        ilOfMethod.Emit(OpCodes.Stloc, localResult);
                    }
                }
                #endregion


                #region  - before -
                //整理参数
                var parameters = ilOfMethod.DeclareLocal(typeof(object[]));
                ilOfMethod.Emit(OpCodes.Ldc_I4, methodParameterTypes.Length);
                ilOfMethod.Emit(OpCodes.Newarr, typeof(object));
                ilOfMethod.Emit(OpCodes.Stloc, parameters);
                for (var j = 0; j < methodParameterTypes.Length; j++)
                {
                    ilOfMethod.Emit(OpCodes.Ldloc, parameters);
                    ilOfMethod.Emit(OpCodes.Ldc_I4, j);
                    ilOfMethod.Emit(OpCodes.Ldarg, j + 1);
                    if (methodParameterTypes[j].IsValueType)
                    {
                        ilOfMethod.Emit(OpCodes.Box, methodParameterTypes[j]);
                    }
                    ilOfMethod.Emit(OpCodes.Stelem_Ref);
                }
                //调用Before
                ilOfMethod.Emit(OpCodes.Ldarg_0);
                ilOfMethod.Emit(OpCodes.Ldstr, attribute.Mapping);
                ilOfMethod.Emit(OpCodes.Ldloc, parameters);
                ilOfMethod.Emit(OpCodes.Ldloc, localResult);
                //调用拦截方法
                ilOfMethod.Emit(OpCodes.Callvirt, ImpType.GetMethod("Invoke", new Type[] { typeof(string), typeof(object[]), typeof(object) }));
                // pop the stack if return void
                if (method.ReturnType == typeof(void))
                {
                    ilOfMethod.Emit(OpCodes.Pop);
                }
                ilOfMethod.Emit(OpCodes.Ret);
                #endregion
            }
        }
    }
}
