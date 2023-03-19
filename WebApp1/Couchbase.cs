using Couchbase;
using Couchbase.Extensions.DependencyInjection;

// implement Couchbase interface for DI
public interface ILogsBucketProvider : INamedBucketProvider {}
public interface IRenderBucketProvider : INamedBucketProvider {}


public interface ICouchbaseSDKClient
{
    Task testLogsBucket(string value);
    Task testRenderBucket(string value);
}

public class Client1 : ICouchbaseSDKClient
{
    private ILogsBucketProvider logsBucketProvider;
    private IRenderBucketProvider renderBucketProvider;

    public  Client1(ILogsBucketProvider logsBucketProvider, IRenderBucketProvider renderBucketProvider) {
        this.logsBucketProvider = logsBucketProvider;
        this.renderBucketProvider = renderBucketProvider;
    }
    public async Task testLogsBucket(string value)
    {
        Console.WriteLine("testLogsBucket:" + value + ". retrieving bucket");
        var logsBucket =  await logsBucketProvider.GetBucketAsync().ConfigureAwait(false);
        Console.WriteLine("testLogsBucket. calling accessbucket");
        await accessbucket(logsBucket);
        Console.WriteLine("testLogsBucket. returned from call to accessbucket");
    }
    public async Task testRenderBucket(string value)
    {
        Console.WriteLine("testRenderBucket:" + value + ". retrieving bucket");
        var renderBucket =  await renderBucketProvider.GetBucketAsync().ConfigureAwait(false);
        Console.WriteLine("testRenderBucket. calling accessbucket");
        await accessbucket(renderBucket);
        Console.WriteLine("testRenderBucket. returned from call to accessbucket");

    }
    private async Task accessbucket(IBucket bucket) {
        Console.WriteLine("access bucket (in method accessbucket):" + bucket.Name);
        var scope = await bucket.ScopeAsync("_default").ConfigureAwait(false);
        var collection = await scope.CollectionAsync("_default").ConfigureAwait(false);
    
        // Upsert Document
        var utcNow = DateTime.UtcNow;        
        var upsertResult = await collection.UpsertAsync("last_successful_timestamp", new { Name = "UTC", Time = utcNow }).ConfigureAwait(false);
        var getResult = await collection.GetAsync("last_successful_timestamp").ConfigureAwait(false);
        Console.WriteLine("method call result for " + bucket.Name + "..."+ getResult.ContentAs<dynamic>());
    }


}