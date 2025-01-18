using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MonsterTradingCardsGame.Application.Configurations;

namespace MonsterTradingCardsGame.Helper.HttpServer
{
    public sealed class HttpSvr
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP listener instance.</summary>
        private TcpListener? _Listener;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public events                                                                                                    //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Is raised when incoming data is available.</summary>
        public event HttpSvrEventHandler? Incoming;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets if the server is available.</summary>
        public bool Active
        {
            get; private set;
        } = false;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Runs the server.</summary>
        public void Run()
        {
            if (Active) return;

            Active = true;
            _Listener = new(IPAddress.Parse(ServerConfig.IPAddress), ServerConfig.Port);
            _Listener.Start();

            byte[] buf = new byte[256];

            Console.WriteLine($"Server running on {ServerConfig.IPAddress}:{ServerConfig.Port}");

            while (Active)
            {
                TcpClient client = _Listener.AcceptTcpClient();
                string data = string.Empty;

                while (client.GetStream().DataAvailable || string.IsNullOrWhiteSpace(data))
                {
                    int n = client.GetStream().Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);
                }

                Incoming?.Invoke(this, new(client, data));
            }
        }


        /// <summary>Stops the server.</summary>
        public void Stop()
        {
            Active = false;
        }
    }
}
