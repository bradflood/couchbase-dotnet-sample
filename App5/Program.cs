using System;
using System.IO;
using System.Threading.Tasks;
using Couchbase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;

await new CloudExample().Main();

class CloudExample
{
    private  HttpClient _httpClient;


    public async Task Main()
    {

        // var connection_string = Environment.GetEnvironmentVariable("cb_connection_string");
        // var userid = Environment.GetEnvironmentVariable("cb_userid");
        // var password = Environment.GetEnvironmentVariable("cb_password");

        // var clusterOptions = new ClusterOptions
        // {
        //     ForceIpAsTargetHost = true,
        //     KvIgnoreRemoteCertificateNameMismatch = true, // development only. do not include in any production configuration
        //     HttpIgnoreRemoteCertificateNameMismatch = true // development only. do not include in any production configuration
        // }
        // .WithConnectionString(connection_string)
        // .WithCredentials(username: userid, password: password)
        // .WithLogging(LoggerFactory.Create(builder => { builder.AddFilter("Couchbase", LogLevel.Debug).AddConsole(); }));

        // var cluster = await Couchbase.Cluster.ConnectAsync(
        //     clusterOptions)
        // .ConfigureAwait(false);

        var sg_connection_string = Environment.GetEnvironmentVariable("sg_connection_string");
        var sg_userid = Environment.GetEnvironmentVariable("sg_userid");
        var sg_password = Environment.GetEnvironmentVariable("sg_password");  
            
        // setup httpclient
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
        };
        _httpClient = new HttpClient(httpClientHandler) { 
            BaseAddress = new Uri(sg_connection_string) 
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "App5");
        var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{sg_userid}:{sg_password}"));
        Console.WriteLine("Base64: " + base64);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);        
        _httpClient.Timeout = TimeSpan.FromSeconds(60);



        // health check
        {
           Console.WriteLine("*** health check ***");
           var response = _httpClient.GetAsync("render/").GetAwaiter().GetResult();
           Console.WriteLine("request: " + response.RequestMessage +".. response " + response);
        }

        addUser() ;

    }

    private async Task addUser()
    {



    }


}


/*
curl --location '52.152.216.148:4985/render/_user/' \
--header 'Content-Type: application/json' \
--header 'Authorization: Basic QWRtaW5pc3RyYXRvcjpDaHJpc3Q0dGhlTmF0aW9ucw==' \
--data '{
    "name": "launchpad",
    "password": "cF7N4MCKTExbDkF!",
    "admin_channels": ["*"],
    "disabled": false
}'
*/