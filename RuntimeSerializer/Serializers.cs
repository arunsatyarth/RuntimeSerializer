using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace RuntimeSerializer
{
    public class Serializers
    {
        /// <summary>
        /// This is the implementation of constructor for ISerializable interface.
        /// The actual implementation will call this function
        /// Use reflection to copy all properties and fields from stream and create an object
        /// </summary>
        /// <param name="t"></param>
        /// <param name="inst"></param>
        /// <param name="info"></param>
        public static void CtorImpl(Type t, object inst, SerializationInfo info)
        {
            object valuex = null;
            try
            {
                PropertyInfo[] allProps = t.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (PropertyInfo item in allProps)
                {
                    if (item.CanRead && item.CanWrite)
                    {
                        valuex = info.GetValue(item.Name, item.PropertyType);
                        if (valuex == null)
                            continue;
                        item.SetValue(inst, valuex, null);
                    }
                }
                FieldInfo[] allFields = t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (FieldInfo item in allFields)
                {
                    valuex = info.GetValue(item.Name, item.FieldType);
                    if (valuex == null)
                        continue;
                    item.SetValue(inst, valuex);

                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
        }
        /// <summary>
        /// This is the implementation of GetObjectData for ISerializable interface.
        /// The actual implementation will call this function
        /// Use Reflection to iterate all fields and properties in the object and set the values in the stream
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="inst"></param>
        /// <param name="info"></param>
        public static void GetObject_Implementation(Type t, object inst, SerializationInfo info)
        {
            object valuex = null;
            try
            {
                PropertyInfo[] allProps = t.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (PropertyInfo item in allProps)
                {
                    if (item.CanRead)
                    {
                        valuex = item.GetValue(inst, null);//get value from retval
                        if (valuex == null)
                            continue;
                        info.AddValue(item.Name, valuex, item.PropertyType);
                    }

                }
                FieldInfo[] allFields = t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (FieldInfo item in allFields)
                {
                    valuex = item.GetValue(inst);
                    if (valuex == null)
                        continue;
                    info.AddValue(item.Name, valuex, valuex.GetType());
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ex in  " + ex.ToString());
            }
        }
    }
}
