using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib
{
    public static class Util
    {
        public static readonly Random rand = new Random();

        public static Ann CloneAnn(Ann ann)
        {
            return ObjectCloner<Ann>.CloneObject(ann);
        }

        private static class ObjectCloner<T>
        {
            public static T CloneObject(T objectToClone)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(memoryStream, objectToClone);
                    memoryStream.Position = 0;

                    return (T)formatter.Deserialize(memoryStream);
                }
            }
        }
    }
}