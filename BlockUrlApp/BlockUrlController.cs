using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;

namespace BlockUrlApp
{
   public class BlockUrlController: IDisposable
   {
      private readonly ProxyServer proxyServer;
      private readonly int _proxyPort = 8000;
      private readonly List<string> BlockedUrl = new List<string>{ "facebook.com", "twitter.com", "youtube.com" };
      public BlockUrlController() {

         proxyServer = new ProxyServer("cert.pfx", "Name", true, true, true)
         {
            ExceptionFunc = ProxyException,

            TcpTimeWaitSeconds = 10,
            ConnectionTimeOutSeconds = 15,
            ReuseSocket = false,
            ForwardToUpstreamGateway = true
         };
         proxyServer.CertificateManager.PfxPassword = "pass@123";
         proxyServer.CertificateManager.SaveFakeCertificates = true;
         proxyServer.CheckCertificateRevocation = X509RevocationMode.NoCheck;
         proxyServer.NoDelay = true;
         proxyServer.EnableConnectionPool = true;
         proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount;
         proxyServer.MaxCachedConnections = 2;
      }

      private void ProxyException(Exception exception)
      {
         Console.WriteLine(exception.ToString());
      }

      public void Start()
      {
         proxyServer.BeforeRequest += OnRequestHandler;

         var explicitEndPointV4 = new ExplicitProxyEndPoint(IPAddress.Any, _proxyPort);

         proxyServer.AddEndPoint(explicitEndPointV4);
         proxyServer.Start();

         if (RunTime.IsWindows)
         {
            proxyServer.SetAsSystemProxy(explicitEndPointV4, ProxyProtocolType.AllHttp);
         }
      }

      private Task OnRequestHandler(object sender, SessionEventArgs evt)
      {
         try
         {
            _ = Task.Run(() => ProcessRequest(evt));
         }
         catch (Exception ex)
         {
            Console.WriteLine("Some error has occurred in receiving request. The error is " + ex.Message);
         }
         return Task.CompletedTask;
      }

      private void ProcessRequest(SessionEventArgs evt)
      {
         try
         {
            string absoluteUri = evt.HttpClient.Request.RequestUri.AbsoluteUri;

            HeaderCollection headers = evt.HttpClient.Request.Headers;

            if(BlockedUrl.AsEnumerable().Any(x => absoluteUri.Contains(x)))
            {
               evt.Ok("<!DOCTYPE html><html><body style='text-align: center;'><h1>Website Blocked</h1><p>This site has been blocked by Admin. Please contact Admin for more info.</p></body></html>");
            }
         }
         catch (Exception exception)
         {
            Console.WriteLine($"[ERROR]: OnRequestHandler: on URL: {evt.HttpClient.Request.RequestUri.AbsoluteUri} Exception Message: {exception}");
         }
      }

      public void Stop()
      {
         proxyServer.BeforeRequest -= OnRequestHandler;
         proxyServer.Stop();
         proxyServer.CertificateManager.ClearRootCertificate();
         proxyServer.RestoreOriginalProxySettings();
         proxyServer.Dispose();
      }

      public void Dispose()
      {
         proxyServer?.Dispose();
      }
   }
}