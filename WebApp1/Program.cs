
using Couchbase.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();


var connection_string = Environment.GetEnvironmentVariable("cb_connection_string");
var userid = Environment.GetEnvironmentVariable("cb_userid");
var password = Environment.GetEnvironmentVariable("cb_password");

if (connection_string == null || userid ==null ||password == null) {
    throw new Exception("couchbase connection information not provided");
}
Console.WriteLine("Couchbase connection string: " + connection_string +".. userid: " + userid);
builder.Services.AddCouchbase(clusterOptions =>
    {
        clusterOptions.QueryTimeout = TimeSpan.FromSeconds(100);
        clusterOptions.EnableTls = true;
        // clusterOptions.IgnoreRemoteCertificateNameMismatch = true;
        clusterOptions.ForceIpAsTargetHost = true;
        clusterOptions.KvIgnoreRemoteCertificateNameMismatch = true;
        clusterOptions.HttpIgnoreRemoteCertificateMismatch = true;
        clusterOptions.WithConnectionString("couchbases://"+connection_string)
            .WithCredentials(username: userid, password: password)
            .WithLogging(LoggerFactory.Create(builder => { builder.AddFilter("Couchbase", LogLevel.Information).AddConsole(); }));
        Console.WriteLine(clusterOptions.ConnectionString);
    });

builder.Services.AddCouchbaseBucket<ILogsBucketProvider>("logs");
builder.Services.AddCouchbaseBucket<IRenderBucketProvider>("render"); 

builder.Services.AddScoped<ICouchbaseSDKClient, Client1>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
