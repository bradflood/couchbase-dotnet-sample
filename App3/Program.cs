using System;
using System.Threading.Tasks;
using Couchbase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;


await new CloudExample().Main();

class CloudExample
{

    /*
    Note: 
    App3 is an earlier version of App1, which initially didn't work. This is here for documentation purposes.
    Note that the calls to accessbucket are not awaited. There are no compile-time or run-time errors.
    
    The call to accessBucket returns a Task. that is not awaited, so the code runs off, closes the application and doesn’t wait for them to finish. By simply pausing the main thread, the correct results are outputted:
    */
    public async Task Main()
    {

        var password = Environment.GetEnvironmentVariable("cb_password");

        var clusterOptions = new ClusterOptions{
            ForceIpAsTargetHost = true
        }
        .WithConnectionString("couchbases://cb.vginy-kxbifuq8dn.cloud.couchbase.com")
        .WithCredentials(username: "launchpad", password: password)
        // .WithBuckets(bucketList)
        .WithLogging(LoggerFactory.Create(builder => { builder.AddFilter("Couchbase", LogLevel.Debug).AddConsole(); }));

        var cluster = await Couchbase.Cluster.ConnectAsync(
            "couchbases://cb.vginy-kxbifuq8dn.cloud.couchbase.com", 
            clusterOptions)
        .ConfigureAwait(false);

        var bucketName = "logs";
        Console.WriteLine("access bucket (inline):" + bucketName);
        var bucket = await cluster.BucketAsync(bucketName).ConfigureAwait(false);
        var scope = await bucket.ScopeAsync("_default").ConfigureAwait(false);
        var collection = await scope.CollectionAsync("_default").ConfigureAwait(false);
    
        // // Upsert Document
        var upsertResult = await collection.UpsertAsync("last_successful_timestamp", new { Name = "UTC", Time = DateTime.UtcNow });
        var getResult = await collection.GetAsync("last_successful_timestamp");
        Console.WriteLine("inline: " + getResult.ContentAs<dynamic>());

        // NOTE: by not awaiting the return of these method calls, the expected results are not obtained.
        // the code runs off, closes the application and doesn’t wait for them to finish. 
        accessbucket(cluster, "logs");
        accessbucket(cluster, "render");
        accessbucket(cluster, "renderaudio");

        //pause the main thread for 5s so the operations complete
        //Thread.Sleep(5000);

    }

    private async void accessbucket(ICluster cluster, string bucketName) {
        Console.WriteLine("access bucket (in method accessbucket):" + bucketName);
        var bucket = await cluster.BucketAsync(bucketName).ConfigureAwait(false);
        var scope = await bucket.ScopeAsync("_default").ConfigureAwait(false);
        var collection = await scope.CollectionAsync("_default").ConfigureAwait(false);
    
        // // Upsert Document
        var utcNow = DateTime.UtcNow;        
        var upsertResult = await collection.UpsertAsync("last_successful_timestamp", new { Name = "UTC", Time = utcNow });
        var getResult = await collection.GetAsync("last_successful_timestamp");
        Console.WriteLine("method accessbucket:" + getResult.ContentAs<dynamic>());
    }

}