using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;

namespace RuntimeSerializer
{
    public class RuntimeSerializer
    {
        static string s_new_dllname = "RS_";
        static string s_path = "";
        private static string GetPath()
        {
            if (s_path == "")
                s_path = Directory.GetCurrentDirectory() + "\\";

            return s_path;

        }
        private static void Generate_RuntimeLibrary(Type baseType, string dllpath)
        {
            //Build a New Type using Reflection Emit
            AppDomain appDomain = AppDomain.CurrentDomain;

            AssemblyName asmName = new AssemblyName();
            //set the new name of the assembly as a comnination of the prefix and exiting type
            asmName.Name = s_new_dllname + baseType.ToString();

            AssemblyBuilder assemblyBuilder = appDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Save, s_path);

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("RunTimeModule", s_new_dllname + baseType.ToString() + ".dll");
            //Add a new class in the module
            TypeBuilder classBuilder = moduleBuilder.DefineType("RuntimeDerived" + baseType.ToString(), TypeAttributes.Public);

            //Adding the default ctor
            classBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            //This is the most important. New type should derive from existing type so as to have all its members
            classBuilder.SetParent(baseType);

            //adding Serializable attribute
            ConstructorInfo ci1 = typeof(SerializableAttribute).GetConstructor(new Type[] { });

            CustomAttributeBuilder cb1 = new CustomAttributeBuilder(ci1, new object[] { });

            classBuilder.SetCustomAttribute(cb1);


            //Implement ISerializable and add default  ctor implementation and GetObjectData 
            classBuilder.AddInterfaceImplementation(typeof(ISerializable));
            ConstructorBuilder cbuild = classBuilder.DefineConstructor(MethodAttributes.Public,
                    CallingConventions.Standard, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) });
            //Add the method GetObjectData from ISerializable . ie implementing the interface
            MethodBuilder serMethod = classBuilder.DefineMethod("GetObjectData", MethodAttributes.Public
                        | MethodAttributes.Virtual, typeof(void), new Type[] { typeof(SerializationInfo), typeof(StreamingContext) });

            //The IL code just calls a methid GetObjectImpl which has the implementation of getObject
            //create the IL generator
            ILGenerator gen = serMethod.GetILGenerator();
            //get the class in which we hav to make function call to perform actual action
            Type util = typeof(Serializers);
            //get the function which we wanna call inside GetObjectData
            MethodInfo meth1 = util.GetMethod("GetObject_Implementation", BindingFlags.Static | BindingFlags.Public, Type.DefaultBinder,
                                         new Type[] { typeof(Type), typeof(object), typeof(SerializationInfo) }, null);
            //Now to make the function call we have to add the parameters into stack starting from last parameter
            //param1
            Type param1Type = typeof(Object);
            MethodInfo meth3 = param1Type.GetMethod("GetType", BindingFlags.Public | BindingFlags.Instance);

            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, meth3);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, meth1);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);


            //now we generate the body for ctor call
            gen = cbuild.GetILGenerator();

            //get the method in Serializers.CtorImpl  which will be called from inside ctor
            meth1 = util.GetMethod("CtorImpl", BindingFlags.Static | BindingFlags.Public, Type.DefaultBinder,
                new Type[] { typeof(Type), typeof(object), typeof(SerializationInfo) }, null);
            //create parameters for function call of meth1
            //pram2
            ConstructorInfo cons = baseType.GetConstructor(new Type[] { });
            ConstructorInfo[] waste = baseType.GetConstructors();
            if (cons == null)
            {
                throw new DefaultCtorAbsentException("The specified type does not have a default ctor");
            }


            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, cons);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, meth3);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, meth1);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);



            //saving the class type inside the assembly
            classBuilder.CreateType();
            //saving the assembly into disk 
            assemblyBuilder.Save(s_new_dllname + baseType.ToString() + ".dll");
        }
        public static Object GenerateSerializableObject(object existingControl)
        {

            Type typeOfControl = existingControl.GetType();
            string folderPath = "";
            folderPath = GetPath();

            Assembly asm = null;
            string dllPath;
            try
            {
                dllPath = folderPath + s_new_dllname + typeOfControl + ".dll";
                //Now generate dll
                Generate_RuntimeLibrary(typeOfControl, dllPath);
                asm = Assembly.LoadFile(dllPath);

                if (asm == null)
                {
                    return false;
                }
                Type[] allClasses = asm.GetTypes();
                if (allClasses.Length != 1)
                    return null;
                Type t = allClasses[0];
                object inst = Activator.CreateInstance(t);

                //now copy all fields
                ShallowCopy(t, existingControl, inst);
                //copy base values also
                ShallowCopy(t.BaseType, existingControl, inst);

                return inst;
            }
            catch (DefaultCtorAbsentException)
            {
                Trace.WriteLine("The specified object does not have a parameterless ctor. going to rethrow the exeption");
                throw;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw;
            }

        }
        private static void ShallowCopy(Type t, object source, object dest)
        {
            FieldInfo[] allfields = t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            object value = null;
            foreach (FieldInfo item in allfields)
            {
                value = item.GetValue(source);//get value from retval
                item.SetValue(dest, value);//set value to new obj
                if (value == null)
                    continue;
            }
            PropertyInfo[] allProps = t.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (PropertyInfo item in allProps)
            {
                if (item.CanRead)
                    value = item.GetValue(source, null);//get value from retval
                if (item.CanRead && item.CanWrite)
                {
                    item.SetValue(dest, value, null);//set value to new obj
                    if (value == null)
                        continue;
                }
            }
        }
    }
}
