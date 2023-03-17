using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp1.Pages;

public class CouchbaseModel : PageModel
{
    private readonly ILogger<CouchbaseModel> _logger;
    private ICouchbaseSDKClient client;

    public CouchbaseModel(ILogger<CouchbaseModel> logger, ICouchbaseSDKClient client)
    {
        _logger = logger;
        this.client = client;
    }

    public void OnGet()
    {
        Console.WriteLine("onGet couchbase");
        client.testLogsBucket("foo") ;
        client.testRenderBucket("foo2") ;
    }
}

