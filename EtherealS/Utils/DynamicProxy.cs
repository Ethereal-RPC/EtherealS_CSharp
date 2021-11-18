﻿using EtherealS.Core.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Utils
{
    public class DynamicProxy
    {
        //代码参考 https://gitee.com/code2roc/FastIOC/blob/master/FastIOC/Proxy/DynamictProxy.cs
        public static T CreateRequestProxy<T>() where T:Request.Abstract.Request
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
                Request.Attribute.RequestMethod attribute = method.GetCustomAttribute<Request.Attribute.RequestMethod>();
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

                #region  - before -
                var parameters = ilOfMethod.DeclareLocal(typeof(object[]));
                ilOfMethod.Emit(OpCodes.Ldc_I4, methodParameterTypes.Length);
                ilOfMethod.Emit(OpCodes.Newarr, typeof(object));
                ilOfMethod.Emit(OpCodes.Stloc, parameters);
                for (var j = 0; j < methodParameterTypes.Length; j++)
                {
                    ilOfMethod.Emit(OpCodes.Ldloc, parameters);
                    ilOfMethod.Emit(OpCodes.Ldc_I4, j);
                    ilOfMethod.Emit(OpCodes.Ldarg, j + 1);
                    ilOfMethod.Emit(OpCodes.Stelem_Ref);
                }
                //调用Before
                ilOfMethod.Emit(OpCodes.Ldarg_0);
                ilOfMethod.Emit(OpCodes.Ldstr, attribute.Mapping);
                ilOfMethod.Emit(OpCodes.Ldloc, parameters);
                //调用拦截方法
                ilOfMethod.Emit(OpCodes.Callvirt,ImpType.GetMethod("Invoke",new Type[] {typeof(string),typeof(object[])}));
                // pop the stack if return void
                if (method.ReturnType == typeof(void))
                {
                    ilOfMethod.Emit(OpCodes.Pop);
                }
                else
                {
                    if (method.ReturnType.IsValueType)
                    {
                        ilOfMethod.Emit(OpCodes.Unbox_Any, method.ReturnType);
                    }
                    else
                    {
                        ilOfMethod.Emit(OpCodes.Castclass, method.ReturnType);
                    }
                }
                ilOfMethod.Emit(OpCodes.Ret);
                #endregion
            }
        }
    }
}
