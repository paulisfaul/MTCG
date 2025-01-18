using System;
using System.Net.Sockets;
using System.Text;

namespace MonsterTradingCardsGame.Helper.HttpServer
{
    public class HttpSvrEventArgs : EventArgs
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected members                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP client.</summary>
        protected TcpClient _Client;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="client">TCP client.</param>
        /// <param name="plainMessage">Plain HTTP message.</param>
        public HttpSvrEventArgs(TcpClient client, string plainMessage)
        {
            _Client = client;

            PlainMessage = plainMessage;
            Payload = string.Empty;

            string[] lines = plainMessage.Replace("\r\n", "\n").Split('\n');
            bool inheaders = true;
            List<HttpHeader> headers = new();

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    string[] inc = lines[0].Split(' ');
                    Method = inc[0];
                    Path = inc[1];
                    RemainingPath = inc[1];
                    continue;
                }

                if (inheaders)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        inheaders = false;
                    }
                    else { headers.Add(new(lines[i])); }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Payload)) { Payload += "\r\n"; }
                    Payload += lines[i];
                }
            }

            Headers = headers.ToArray();
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the plain message.</summary>
        public string PlainMessage
        {
            get; protected set;
        } = string.Empty;


        /// <summary>Gets the HTTP method.</summary>
        public virtual string Method
        {
            get; protected set;
        } = string.Empty;


        /// <summary>Gets the HTTP path.</summary>
        public virtual string Path
        {
            get; protected set;
        } = string.Empty;

        /// <summary>Gets the remaining HTTP path.</summary>
        public virtual string RemainingPath
        {
            get;
            set;
        } = string.Empty;


        /// <summary>Gets the HTTP headers.</summary>
        public virtual HttpHeader[] Headers
        {
            get; protected set;
        } = Array.Empty<HttpHeader>();


        /// <summary>Gets the payload.</summary>
        public virtual string Payload
        {
            get; protected set;
        } = string.Empty;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Replies the request</summary>
        /// <param name="status">HTTP Status code.</param>
        /// <param name="msg">Reply body.</param>
        public void Reply(int status, string? body = null)
        {
            string data;

            switch (status)
            {
                case HttpStatusCode.OK:
                    data = "HTTP/1.1 200 OK\n"; break;
                case HttpStatusCode.CREATED:
                    data = "HTTP/1.1 201 Created\n"; break;
                case HttpStatusCode.BAD_REQUEST:
                    data = "HTTP/1.1 400 Bad Request\n"; break;
                case HttpStatusCode.UNAUTHORIZED:
                    data = "HTTP/1.1 401 Unauthorized\n"; break;
                case HttpStatusCode.NOT_FOUND:
                    data = "HTTP/1.1 404 Not Found\n"; break;
                case HttpStatusCode.METHOD_NOT_ALLOWED:
                    data = "HTTP/1.1 405 Method Not Allowed\n"; break;
                case HttpStatusCode.CONFLICT:
                    data = "HTTP/1.1 409 Conflict\n"; break;
                default:
                    data = $"HTTP/1.1 {status} Status Unknown\n"; break;
            }

            if (string.IsNullOrEmpty(body))
            {
                data += "Content-Length: 0\n";
            }
            data += "Content-Type: text/plain\n\n";
            if (!string.IsNullOrEmpty(body)) { data += body; }

            byte[] buf = Encoding.ASCII.GetBytes(data);
            _Client.GetStream().Write(buf, 0, buf.Length);
            _Client.Close();
            _Client.Dispose();
        }
    }

}
