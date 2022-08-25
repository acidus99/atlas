using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using atlas.Contexts;
using atlas.Protocols;

namespace atlas
{
    public static class Server
    {
        public static Socket GeminiSocket { get; set; }
        public static Socket SpartanSocket { get; set; }
        public static Configuration Config { get; set; }
        public static SslServerAuthenticationOptions TlsOptions { get; set; }

        static Server()
        {
            TlsOptions = new SslServerAuthenticationOptions
            {
                EnabledSslProtocols = SslProtocols.Tls13,
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                ClientCertificateRequired = true,
            };
            TlsOptions.ServerCertificateSelectionCallback += (object _, string host) =>
            {
                if (Config.Capsules.TryGetValue(host, out var capsule))
                    return X509Certificate.CreateFromCertFile(capsule.AbsoluteTlsCertPath);
                return null;
            };

            TlsOptions.RemoteCertificateValidationCallback += (object _, X509Certificate _, X509Chain _, SslPolicyErrors _) => true; ;
        }

        public static void Start()
        {
            GeminiSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            GeminiSocket.Bind(new IPEndPoint(IPAddress.Any, Config.Port));
            GeminiSocket.Listen();

            SpartanSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SpartanSocket.Bind(new IPEndPoint(IPAddress.Any, 3000));//Config.SpartanPort
            SpartanSocket.Listen();

            Task.Run(async () =>
            {
                while (true)
                {
                    Console.WriteLine("[GEMINI] Waiting for connection...");
                    var clientSocket = await GeminiSocket.AcceptAsync();

            var ctx = new GeminiCtx()
            {
                Socket = clientSocket,
                Stream = new SslStream(new NetworkStream(clientSocket), false)
            };

                    var success = await Gemini.HandShake(ctx);
                    try
                    {
                        if (!success)
                            continue;

                        await Gemini.ReceiveHeader(ctx);

                        if (ctx.IsUpload)
                            await Titan.HandleRequest(ctx);
                        else
                            await Gemini.HandleRequest(ctx);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        ctx.CloseConnection();
                    }
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    Console.WriteLine("[SPARTAN] Waiting for connection...");
                    var clientSocket = await SpartanSocket.AcceptAsync();

                    var ctx = new SpartanCtx()
                    {
                        Socket = clientSocket,
                        Stream = new NetworkStream(clientSocket)
                    };

                    try
                    {
                        await Spartan.ReceiveHeader(ctx);

                        if (ctx.IsUpload)
                            await Spartan.POST(ctx);
                        else
                            await Spartan.GET(ctx);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        ctx.CloseConnection();
                    }
                }
            });
        }
    }
}