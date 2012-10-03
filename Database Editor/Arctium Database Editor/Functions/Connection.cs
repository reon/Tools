using Db4objects.Db4o;
using Db4objects.Db4o.CS;

namespace Arctium_Database_Editor.Functions
{
    public class Connection
    {
        public IObjectContainer Database;

        public Connection(string host, int port, string user, string password)
        {
            Database = Db4oClientServer.OpenClient(host, port, user, password);
        }
    }
}
