﻿using SIS.HTTP.Enums;
using SIS.HTTP.Exceptions;
using SIS.HTTP.Requests;
using SIS.HTTP.Requests.Contracts;
using SIS.HTTP.Responses.Contracts;
using SIS.WebServer.Results;
using SIS.WebServer.Routing.Contracts;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SIS.WebServer
{
    public class ConnectionHandler
    {
        private readonly Socket client;

        private readonly IServerRoutingTable serverRoutingTable;

        public ConnectionHandler(Socket client, IServerRoutingTable serverRoutingTable)
        {
            this.client = client;
            this.serverRoutingTable = serverRoutingTable;
        }

        public void ProcessRequest()
        {
            try
            {
                var httpRequest = this.ReadRequest();

                if (httpRequest != null)
                {
                    Console.WriteLine($"Processing: {httpRequest.RequestMethod} {httpRequest.Path}...");

                    var httpResponse = this.HandleRequest(httpRequest);

                    this.PrepareResponse(httpResponse);
                }
            }
            catch (BadRequestException e)
            {
                this.PrepareResponse(new TextResult(e.ToString(), HttpResponseStatusCode.BadRequest));
            }
            catch(Exception e)
            {
                this.PrepareResponse(new TextResult(e.ToString(), HttpResponseStatusCode.InternalServerError));
            }

            this.client.Shutdown(SocketShutdown.Both);
        }

        private IHttpRequest ReadRequest()
        {
            var result = new StringBuilder();
            var data = new ArraySegment<byte>(new byte[1024]);

            while (true)
            {
                int numberOfBytesRead = this.client.Receive(data.Array, SocketFlags.None);
                if(numberOfBytesRead == 0)
                {
                    break;
                }

                var bytesAsString = Encoding.UTF8.GetString(data.Array, 0, numberOfBytesRead);
                result.Append(bytesAsString);

                if(numberOfBytesRead < 1023)
                {
                    break;
                }
            }

            if(result.Length == 0)
            {
                return null;
            }

            return new HttpRequest(result.ToString());
        }

        private IHttpResponse HandleRequest(IHttpRequest httpRequest)
        {
            if(!this.serverRoutingTable.Contains(httpRequest.RequestMethod, httpRequest.Path))
            {
                return new TextResult($"Route with method {httpRequest.RequestMethod} and path \"{httpRequest.Path}\" not found.", HttpResponseStatusCode.NotFound);
            }

            return this.serverRoutingTable.Get(httpRequest.RequestMethod, httpRequest.Path).Invoke(httpRequest);
        }

        private void PrepareResponse(IHttpResponse httpResponse)
        {
            byte[] byteSegments = httpResponse.GetBytes();

            this.client.Send(byteSegments, SocketFlags.None);
        }
    }
}
