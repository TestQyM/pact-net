﻿using System;
using Nancy.Hosting.Self;

namespace PactNet
{
    public class PactProvider : IDisposable
    {
        private NancyHost _host;
        private readonly string _baseUri;
        private string _description;
        private PactProviderRequest _request;
        private PactProviderResponse _response;

        public PactProvider(int port)
        {
            _baseUri = String.Format("http://localhost:{0}", port);
        }

        public PactProvider UponReceiving(string description)
        {
            _description = description;

            return this;
        }

        public PactProvider With(PactProviderRequest request)
        {
            _request = request;
            PactNancyRequestDispatcher.Set(request);

            return this;
        }

        public PactProvider WillRespondWith(PactProviderResponse response)
        {
            _response = response;
            PactNancyRequestDispatcher.Set(response);

            return this;
        }

        internal void Start()
        {
            var hostConfig = new HostConfiguration { UrlReservations = { CreateAutomatically = true }, AllowChunkedEncoding = false };
            _host = new NancyHost(new PactNancyBootstrapper(), hostConfig, new Uri(_baseUri));

            _host.Start();

            PactNancyRequestDispatcher.Reset();
        }

        internal void Stop()
        {
            _host.Stop();

            PactNancyRequestDispatcher.Reset();
        }

        public PactInteraction DescribeInteraction()
        {
            return new PactInteraction
                       {
                           Description = _description,
                           Request = _request,
                           Response = _response
                       };
        }

        public void Dispose()
        {
            if(_host != null)
                _host.Dispose();
        }
    }
}