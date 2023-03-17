using Couchbase.Extensions.DependencyInjection;
// implement Couchbase interface for DI
public interface IRenderBucketProvider : INamedBucketProvider {}
public interface IRenderAudioBucketProvider : INamedBucketProvider {}
public interface ILogsBucketProvider : INamedBucketProvider {}


public interface ICouchbaseSDKClient
{
    void testLogsBucket(string value);
    void testRenderBucket(string value);
    void testRenderAudioBucket(string value);
}

public class Client1 : ICouchbaseSDKClient
{
    private ILogsBucketProvider logsBucketProvider;
    private IRenderBucketProvider renderBucketProvider;
    private IRenderAudioBucketProvider renderAudioBucketProvider;


    public Client1(ILogsBucketProvider logsBucketProvider, IRenderBucketProvider renderBucketProvider, IRenderAudioBucketProvider renderAudioBucketProvider) {
        this.logsBucketProvider = logsBucketProvider;
        this.renderBucketProvider = renderBucketProvider;
        this.renderAudioBucketProvider = renderAudioBucketProvider;
    }
    public void testLogsBucket(string value)
    {
        Console.WriteLine("testLogsBucket:" + value);
    }
    public void testRenderBucket(string value)
    {
        Console.WriteLine("testRenderBucket:" + value);

    }
    public void testRenderAudioBucket(string value)
    {
        Console.WriteLine("testRenderAudioBucket:" + value);

    }
}