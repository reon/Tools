using Db4objects.Db4o;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.Internal.Convert;
using Db4objects.Db4o.Linq;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Arctium_Database_Editor.Functions
{
    public class Connection
    {
        public IObjectContainer Database;

        public Connection(string host, int port, string user, string password)
        {
            Database = Db4oClientServer.OpenClient(host, port, user, password);
        }

        public List<T> Select<T>()
        {
            var sObject = from T o in Database select o;

            List<T> list = new List<T>();
            foreach (T obj in sObject)
                list.Add(obj);

            return list;
        }

        public Array Select(Type type)
        {
            var sObject = from object o in Database select o;

            ArrayList list = new ArrayList();
            foreach (var obj in sObject)
                if (obj.GetType() == type)
                {
                    list.Add(obj);
                    break;
                }

            return list.ToArray(type);
        }
    }
}
