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
    private readonly string audioAttachmentName = "blob%2Fimage";


    public async Task Main()
    {

        var connection_string = Environment.GetEnvironmentVariable("cb_connection_string");
        var userid = Environment.GetEnvironmentVariable("cb_userid");
        var password = Environment.GetEnvironmentVariable("cb_password");

        

        var clusterOptions = new ClusterOptions
        {
            ForceIpAsTargetHost = true
        }
        .WithConnectionString("couchbases://" + connection_string)
        .WithCredentials(username: userid, password: password)
        .WithLogging(LoggerFactory.Create(builder => { builder.AddFilter("Couchbase", LogLevel.Debug).AddConsole(); }));

        var cluster = await Couchbase.Cluster.ConnectAsync(
            clusterOptions)
        .ConfigureAwait(false);

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
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "foozy");
        var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{sg_userid}:{sg_password}"));
        Console.WriteLine("Base64: " + base64);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);        
        _httpClient.Timeout = TimeSpan.FromSeconds(60);

        var response = _httpClient.GetAsync("render/").GetAwaiter().GetResult();
       Console.WriteLine("request: " + response.RequestMessage +".. response " + response);

        await accessbucket(cluster, "renderaudio");

        await addAttachment();

        //retrieveDocument(_httpClient, "test-add-attachment-318e7eb0-ef06-4e0c-8c05-6015d24b90f1", "3-d424456353a5dc887070ea0fb8d96081", "audio");

    }



    private async Task addAttachment()
    {
        // health check
        {
            Console.WriteLine("*** health check ***");
            var path = "renderaudio/";
            var response = _httpClient.GetAsync(path).GetAwaiter().GetResult();
            Console.WriteLine("path: " + path +". response:" + response);
        }

        // Guid guid = Guid.NewGuid();
        string guid = "318e7eb0-ef06-4e0c-8c05-6015d24b90f1";
        var documentKey = "test-add-attachment-"+guid;
        var stringAttachmentName = "blob%2Ftext";
        var inputFilename = "input.opus";
        var outputFilename = "output.opus";

        string revision ;

        // {
        //     Console.WriteLine("*** create new document ***");
        //     var path = $"app_endpoint_renderaudio/{documentKey}";
        //     var body = "{\"Type\": \"test\"}";

        //     var content = new StringContent(body);
        //     var response = _httpClient.PutAsync(path,content).GetAwaiter().GetResult();
        //     Console.WriteLine("path: " + path +". response:" + response);
        //     check("create new document", response);
        //     revision = response.Headers.ETag?.Tag.Trim('"');

        // }

        {
            // get latest revision
            var path = $"renderaudio/{documentKey}?revs_limit=1";
            var response = _httpClient.GetAsync(path).GetAwaiter().GetResult();
            Console.WriteLine("SYNC GATEWAY CALL path: " + path +". response:" + response);
            revision = response.Headers.ETag?.Tag.Trim('"');

        }

        {
            Console.WriteLine("*** add string attachment ***");
            // add attachment
            var path = $"renderaudio/{documentKey}/{stringAttachmentName}?rev="+revision;

            byte[] bytes = Encoding.ASCII.GetBytes("this is a string - " + guid);

            var byteContent = new ByteArrayContent(bytes);
            byteContent.Headers.Add("Content-Type", "text/example");

            var response = _httpClient.PutAsync(path, byteContent).GetAwaiter().GetResult();
            Console.WriteLine("SYNC GATEWAY CALL path: " + path +". byteContent headers: "+byteContent.Headers +". response:" + response);
            revision = response.Headers.ETag?.Tag.Trim('"');            
        }
        {
            Console.WriteLine("*** retrieve document with string attachment ***");
            // add attachment
            var path = $"renderaudio/{documentKey}/{stringAttachmentName}?rev="+revision;

            var response = _httpClient.GetAsync(path).GetAwaiter().GetResult();
            Console.WriteLine("SYNC GATEWAY CALL path: " + path +". response:" + response);
            Console.WriteLine("content: " + response.Content); 
            var attachmentByteArray = response.Content.ReadAsByteArrayAsync().Result; 

            Console.WriteLine("string attachment contents: " + Encoding.UTF8.GetString(attachmentByteArray));

        }    
        {
            Console.WriteLine("*** add audio attachment ***");
            // add attachment
            var path = $"renderaudio/{documentKey}/{audioAttachmentName}?rev="+revision;

            byte[] bytes = File.ReadAllBytes(inputFilename);

            var byteContent = new ByteArrayContent(bytes);
            byteContent.Headers.Add("Content-Type", "audio/opus");

            var response = _httpClient.PutAsync(path, byteContent).GetAwaiter().GetResult();
            Console.WriteLine("SYNC GATEWAY CALL path: " + path +". byteContent headers: "+byteContent.Headers +". response:" + response);
            check("add audio attachment", response);
            revision = response.Headers.ETag?.Tag.Trim('"');            
        }

        {
            Console.WriteLine("*** retrieve document with audio attachment ***");
            // add attachment
            var path = $"renderaudio/{documentKey}/{audioAttachmentName}?rev="+revision;

            var response = _httpClient.GetAsync(path).GetAwaiter().GetResult();
            Console.WriteLine("SYNC GATEWAY CALL path: " + path +". response:" + response);
            Console.WriteLine("content: " + response.Content); 
            // var attachmentByteArray = response.Content.ReadAsStreamAsync().Result; 
            var attachmentByteArray = response.Content.ReadAsByteArrayAsync().Result; 


            // Create the file, or overwrite if the file exists.
            using (FileStream fs = File.Create(outputFilename))
            {
                fs.Write(attachmentByteArray, 0, attachmentByteArray.Length);   
                // File.WriteAllBytes("./attachment.txt", attachmentByteArray);                   
                Console.WriteLine("attachment contents written to file: " + fs.Name);       
            }              
        }

        Console.WriteLine("\n\n *** done ***");
    }


    private async Task accessbucket(ICluster cluster, string bucketName)
    {
        Console.WriteLine("access bucket (in method accessbucket):" + bucketName);
        var bucket = await cluster.BucketAsync(bucketName).ConfigureAwait(false);
        var scope = await bucket.ScopeAsync("_default").ConfigureAwait(false);
        var collection = await scope.CollectionAsync("_default").ConfigureAwait(false);

        // Upsert Document
        var utcNow = DateTime.UtcNow;
        var upsertResult = await collection.UpsertAsync("last_successful_timestamp", new { Name = "UTC", Time = utcNow }).ConfigureAwait(false);
        var getResult = await collection.GetAsync("last_successful_timestamp").ConfigureAwait(false);
        Console.WriteLine("method call result for " + bucketName + "..." + getResult.ContentAs<dynamic>());
    }

    private void check(string action, HttpResponseMessage response) {
       Console.WriteLine("request: " + response.RequestMessage +".. response " + response);
       if (!response.IsSuccessStatusCode){
          Console.WriteLine(action + " failed. exiting");
          Environment.Exit(1);
       }
    }

    private bool retrieveDocument(HttpClient client, string documentKey, string revision, string attachmentName) {
                {
            Console.WriteLine("*** retrieve document with audio attachment ***");
            // add attachment
            var path = $"renderaudio/{documentKey}/{attachmentName}?rev="+revision;

            var response = client.GetAsync(path).GetAwaiter().GetResult();
            Console.WriteLine("path: " + path +". response:" + response);
            Console.WriteLine("content: " + response.Content); 
            // var attachmentByteArray = response.Content.ReadAsStreamAsync().Result; 
            var attachmentByteArray = response.Content.ReadAsByteArrayAsync().Result; 


            // Create the file, or overwrite if the file exists.
            string outputFilename = $"{documentKey}_{revision}.opus";
            using (FileStream fs = File.Create(outputFilename))
            {
                fs.Write(attachmentByteArray, 0, attachmentByteArray.Length);   
                // File.WriteAllBytes("./attachment.txt", attachmentByteArray);                   
                Console.WriteLine("attachment contents written to file: " + fs.Name);       
            }              
        }
        return true ;
    }

}

// https://www.couchbase.com/blog/store-sync-binary-data-attachments-blobs-couchbase-mobile/
//  curl -i -X PUT 
//  'http://sync-gateway-url:4984/dbname/user::jane/blob_%2Fimage?rev=1-ed2d37e7ece0dc5726fecd211433cbba' 
//  -H 'Accept: application/json' 
//  -H 'Authorization: Basic ZGVtbzE6cGFzc3dvcmQ=' 
//  -H 'Content-Type: image/png' 
//  –data-binary "@layered.png”

/*
curl -i -X PUT \
'https://0oytoea0mlnbtooa.apps.cloud.couchbase.com:4984/app_endpoint_renderaudio/audio::e0332496-f776-46d7-ae5c-7de24cb07ed6/blob_%2Fimage?rev=20-76aa1cf6f55f1b6cea4ed38c7eee7223' \
  -H 'Accept: application/json' \
  -H 'Authorization: Basic cmVtb3RlX2RiOlBhc3N3b3JkMTIzIQ=='  \
  -H 'Content-Type: image/png' \
  –data-binary "@audio.opus”;

curl -i -X GET \
'https://0oytoea0mlnbtooa.apps.cloud.couchbase.com:4984/app_endpoint_renderaudio/test-add-attachment/blob%2Fimage?rev=10-924498c7d5b1eb79f82c8633ec0b17f0' \
  -H 'Accept: application/json' \
  -H 'Authorization: Basic cmVtb3RlX2RiOlBhc3N3b3JkMTIzIQ=='  
*/