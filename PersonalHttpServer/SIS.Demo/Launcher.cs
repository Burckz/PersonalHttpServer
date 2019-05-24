using SIS.WebServer;
using SIS.WebServer.Routing;
using SIS.WebServer.Routing.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Demo
{
    public class Launcher
    {
        public static void Main(string[] args)
        {
            IServerRoutingTable serverRoutingTable = new ServerRoutingTable();

            serverRoutingTable.Add(HTTP.Enums.HttpRequestMethod.Get,
                "/",
                request => new HomeController().Index(request));

            Server server = new Server(8000, serverRoutingTable);

            server.Run();
        }
    }
}
