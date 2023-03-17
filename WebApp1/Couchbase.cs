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

    private  IBucket logsBucket;
    private  IBucket renderBucket;


    public  Client1(ILogsBucketProvider logsBucketProvider, IRenderBucketProvider renderBucketProvider) {
        this.logsBucketProvider = logsBucketProvider;
        this.renderBucketProvider = renderBucketProvider;
    }
    public async Task testLogsBucket(string value)
    {
        Console.WriteLine("testLogsBucket:" + value);
        if (logsBucket == null) {
            logsBucket =  await logsBucketProvider.GetBucketAsync().ConfigureAwait(false);
        }
        await accessbucket(logsBucket);
    }
    public async Task testRenderBucket(string value)
    {
        Console.WriteLine("testRenderBucket:" + value);
        if (renderBucket == null) {
            renderBucket =  await renderBucketProvider.GetBucketAsync().ConfigureAwait(false);
        }        
        await accessbucket(renderBucket);

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