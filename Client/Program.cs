using RuntimeSerializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class NonSerializedClass2
    {
        int m_val1 { get; set; }
        int m_val2 { get; set; }
        int m_val3 { get; set; }
        int m_val4 { get; set; }
        public NonSerializedClass2()
        {
            m_val2 = 400;
            m_val4 = 500;
        }
    }
    public class NonSerializedClass1
    {
        int m_val1 { get; set; }
        int m_val2 { get; set; }
        int m_val3 { get; set; }
        int m_val4 { get; set; }
        List<NonSerializedClass2> m_list = new List<NonSerializedClass2>();
        NonSerializedClass2 class2 = new NonSerializedClass2();
        public NonSerializedClass1()
        {
            m_val1 = 200;
            m_val3 = 400;
            m_list.Add(class2);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            NonSerializedClass1 nonserialisedobject=null;
            Stream stream = null;
            BinaryFormatter bformatter = null;
            try
            {
                nonserialisedobject = new NonSerializedClass1();
                stream = File.Open("Serializedfile", FileMode.Create);
                bformatter = new BinaryFormatter();

                bformatter.Serialize(stream, nonserialisedobject);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("This results in exception as class is not serializable");
                
            }
            finally
            {
                stream.Close();
            }


            NonSerializedClass1 runtime_serialised_object = (NonSerializedClass1)RuntimeSerializer.RuntimeSerializer.GenerateSerializableObject(nonserialisedobject);

            stream = File.Open("Serializedfile", FileMode.Create);
            bformatter = new BinaryFormatter();

            bformatter.Serialize(stream, runtime_serialised_object);
            stream.Close();


            FileStream fs = new FileStream("Serializedfile",FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();

            NonSerializedClass1 obj5 = (NonSerializedClass1)bf.Deserialize(fs);
            fs.Close();
        }
    }
}
