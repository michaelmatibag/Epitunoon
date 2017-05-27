﻿using RestSharp;
using Services.Infrastructure.RestConfiguration;

namespace Services.Integration.RestExecutor
{
    public class RestExecutor : IRestExecutor
    {
        IRestConfiguration _restConfiguration;
        RestClient _client;
        RestRequest _request;

        public RestExecutor(IRestConfiguration restConfiguration)
        {
            _restConfiguration = restConfiguration;
        }

        public string Execute(string request)
        {
            _client = new RestClient(_restConfiguration.RestApi);
            _request = new RestRequest(request);

            var response = _client.Execute(_request);

            return response.Content;
        }
    }
}