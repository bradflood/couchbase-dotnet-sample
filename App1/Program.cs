using System;
using System.Threading.Tasks;
using Couchbase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;


await new CloudExample().Main();

class CloudExample
{

    public async Task Main()
    {

        var connection_string = Environment.GetEnvironmentVariable("cb_connection_string");
        var userid = Environment.GetEnvironmentVariable("cb_userid");
        var password = Environment.GetEnvironmentVariable("cb_password");

        var clusterOptions = new ClusterOptions{
            ForceIpAsTargetHost = true,
            KvIgnoreRemoteCertificateNameMismatch = true // development only. do not include in any production configuration
        }
        .WithConnectionString("couchbases://"+connection_string)
        .WithCredentials(username: userid, password: password)
        .WithLogging(LoggerFactory.Create(builder => { builder.AddFilter("Couchbase", LogLevel.Debug).AddConsole(); }));

        var cluster = await Couchbase.Cluster.ConnectAsync(
            clusterOptions)
        .ConfigureAwait(false);

        await accessbucket(cluster, "logs");
        await accessbucket(cluster, "render");
        await accessbucket(cluster, "renderaudio");

    }

    private async Task accessbucket(ICluster cluster, string bucketName) {
        Console.WriteLine("access bucket (in method accessbucket):" + bucketName);
        var bucket = await cluster.BucketAsync(bucketName).ConfigureAwait(false);
        var scope = await bucket.ScopeAsync("_default").ConfigureAwait(false);
        var collection = await scope.CollectionAsync("_default").ConfigureAwait(false);
    
        // Upsert Document
        var utcNow = DateTime.UtcNow;        
        var upsertResult = await collection.UpsertAsync("last_successful_timestamp", new { Name = "UTC", Time = utcNow }).ConfigureAwait(false);
        var getResult = await collection.GetAsync("last_successful_timestamp").ConfigureAwait(false);
        Console.WriteLine("method call result for " + bucketName + "..."+ getResult.ContentAs<dynamic>());
    }

}