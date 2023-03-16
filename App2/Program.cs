using System;
using System.Threading.Tasks;
using Couchbase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;


await new App2().Main();

class App2
{

    public async Task Main()
    {
        var password = Environment.GetEnvironmentVariable("cb_password");

        var clusterOptions = new ClusterOptions{
            ForceIpAsTargetHost = true
        }
        .WithConnectionString("couchbases://cb.vginy-kxbifuq8dn.cloud.couchbase.com")
        .WithCredentials(username: "launchpad", password: password)
        .WithLogging(LoggerFactory.Create(builder => { builder.AddFilter("Couchbase", LogLevel.Debug).AddConsole(); }));

        var cluster = await Couchbase.Cluster.ConnectAsync(
            "couchbases://cb.vginy-kxbifuq8dn.cloud.couchbase.com", 
            clusterOptions)
        .ConfigureAwait(false);

        // bucket: logs
        {
        var bucketName = "logs";
        Console.WriteLine("inline:" + bucketName);
        var bucket = await cluster.BucketAsync(bucketName).ConfigureAwait(false);
        var scope = await bucket.ScopeAsync("_default").ConfigureAwait(false);
        var collection = await scope.CollectionAsync("_default").ConfigureAwait(false);
    
        var upsertResult = await collection.UpsertAsync("last_successful_timestamp", new { Name = "UTC", Time = DateTime.UtcNow });
        var getResult = await collection.GetAsync("last_successful_timestamp");
        Console.WriteLine("inline result for " + bucketName + "..."+ getResult.ContentAs<dynamic>());
        }

        {
        // bucket: render
        var bucketName = "render";
        Console.WriteLine("inline:" + bucketName);
        var bucket = await cluster.BucketAsync(bucketName).ConfigureAwait(false);
        var scope = await bucket.ScopeAsync("_default").ConfigureAwait(false);
        var collection = await scope.CollectionAsync("_default").ConfigureAwait(false);
    
        var upsertResult = await collection.UpsertAsync("last_successful_timestamp", new { Name = "UTC", Time = DateTime.UtcNow });
        var getResult = await collection.GetAsync("last_successful_timestamp");
        Console.WriteLine("inline result for " + bucketName + "..."+ getResult.ContentAs<dynamic>());
        }

        {
        // bucket: renderaudio
        var bucketName = "renderaudio";
        Console.WriteLine("inline:" + bucketName);
        var bucket = await cluster.BucketAsync(bucketName).ConfigureAwait(false);
        var scope = await bucket.ScopeAsync("_default").ConfigureAwait(false);
        var collection = await scope.CollectionAsync("_default").ConfigureAwait(false);
    
        var upsertResult = await collection.UpsertAsync("last_successful_timestamp", new { Name = "UTC", Time = DateTime.UtcNow });
        var getResult = await collection.GetAsync("last_successful_timestamp");
        Console.WriteLine("inline result for " + bucketName + "..."+ getResult.ContentAs<dynamic>());
        }

    }

}