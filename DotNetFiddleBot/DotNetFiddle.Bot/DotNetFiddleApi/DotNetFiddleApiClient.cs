using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using RestSharp;

namespace DotNetFiddle.Bot.DotNetFiddleApi
{
    public class DotNetFiddleApiClient
    {
        private string ApiRootUrl
        {
            get { return ConfigurationManager.AppSettings["DotNetFiddleApi_Url"]; }
        }


        public async Task<IRestResponse<ExecuteFiddleResponse>> ExecuteFiddleAsync(FiddleExecuteRequest request)
        {
            string apiUrl = ApiRootUrl + "/fiddles";
            var client = new RestClient(apiUrl);

            // execute request through API
            var restRequest = new RestRequest("execute", Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(request);

            var cancellationTokenSource = new CancellationTokenSource();

            IRestResponse<ExecuteFiddleResponse> response = await client.ExecuteTaskAsync<ExecuteFiddleResponse>(restRequest, cancellationTokenSource.Token);
            return response;
        }

        public IRestResponse<ExecuteFiddleResponse> ExecuteFiddle(FiddleExecuteRequest request)
        {
            string apiUrl = ApiRootUrl + "/fiddles";
            var client = new RestClient(apiUrl);

            // execute request through API
            var restRequest = new RestRequest("execute", Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(request);
            
            IRestResponse<ExecuteFiddleResponse> response = client.Execute<ExecuteFiddleResponse>(restRequest);
            return response;
        }

    }
}