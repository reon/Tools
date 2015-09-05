// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.IO;
using System.Text;

namespace DataExtractor.Reader
{
    public static class Extensions
    {
        // Create only one service
        static PluralizationService pluralService = PluralizationService.CreateService(new CultureInfo("en-US"));

        public static string Pluralize(this string s)
        {
            var pluralized = pluralService.Pluralize(s);

            // Handle some language exceptions
            if (pluralized.EndsWith("Datas"))
                return pluralized.Remove(pluralized.Length - 1, 1);
            else if (pluralized.EndsWith("Infoes"))
                return pluralized.Remove(pluralized.Length - 2, 2);

            return pluralized;
        }
        public static Dictionary<Type, Func<BinaryReader, object>> ReadValue = new Dictionary<Type, Func<BinaryReader, object>>()
        {
            { typeof(bool),   br => br.ReadBoolean() },
            { typeof(sbyte),  br => br.ReadSByte()   },
            { typeof(byte),   br => br.ReadByte()    },
            { typeof(char),   br => br.ReadChar()    },
            { typeof(short),  br => br.ReadInt16()   },
            { typeof(ushort), br => br.ReadUInt16()  },
            { typeof(int),    br => br.ReadInt32()   },
            { typeof(uint),   br => br.ReadUInt32()  },
            { typeof(float),  br => br.ReadSingle()  },
            { typeof(long),   br => br.ReadInt64()   },
            { typeof(ulong),  br => br.ReadUInt64()  },
            { typeof(double), br => br.ReadDouble()  },
        };

        public static T Read<T>(this BinaryReader br)
        {
            var type = typeof(T).IsEnum ? typeof(T).GetEnumUnderlyingType() : typeof(T);

            return (T)ReadValue[type](br);
        }
        public static sbyte[] ReadSByte(this BinaryReader br, int count)
        {
            var arr = new sbyte[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadSByte();

            return arr;
        }

        public static byte[] ReadByte(this BinaryReader br, int count)
        {
            var arr = new byte[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadByte();

            return arr;
        }

        public static int[] ReadInt32(this BinaryReader br, int count)
        {
            var arr = new int[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadInt32();

            return arr;
        }

        public static uint[] ReadUInt32(this BinaryReader br, int count)
        {
            var arr = new uint[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadUInt32();

            return arr;
        }

        public static float[] ReadSingle(this BinaryReader br, int count)
        {
            var arr = new float[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadSingle();

            return arr;
        }

        public static long[] ReadInt64(this BinaryReader br, int count)
        {
            var arr = new long[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadInt64();

            return arr;
        }

        public static ulong[] ReadUInt64(this BinaryReader br, int count)
        {
            var arr = new ulong[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadUInt64();

            return arr;
        }

        public static string ReadCString(this BinaryReader br)
        {
            StringBuilder tmpString = new StringBuilder();
            char tmpChar = br.ReadChar();
            char tmpEndChar = Convert.ToChar(Encoding.UTF8.GetString(new byte[] { 0 }));

            while (tmpChar != tmpEndChar)
            {
                tmpString.Append(tmpChar);
                tmpChar = br.ReadChar();
            }

            return tmpString.ToString();
        }

        public static string ReadString(this BinaryReader br, int count)
        {
            byte[] stringArray = br.ReadBytes(count);
            return Encoding.ASCII.GetString(stringArray);
        }

        public static void WriteString(this BinaryWriter bw, string data, bool reverse = false)
        {
            var bytes = Encoding.ASCII.GetBytes(data);

            bw.Write(bytes);
        }
    }
}
