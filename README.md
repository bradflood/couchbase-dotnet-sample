# Simple couchbase code to troubleshoot issues with dotnet SDK and Capella

All Apps are dotnet console apps, using dotnet 6 and couchbase sdk 3.4.4.

In the target capella database, there are three buckets: render, renderaudio and logs


## App1
App1 upserts a single document entitled "last_successful_timestamp" into all three buckets, via a method call. The file run.out contains debug level output. 

On MacOS and Amazon Linux 2, App1 works as expected - the documents are inserted into each of the three buckets, and the UTC timestamp is updated upon successive executions.

On Windows, App1 works as long as there is no AppService. When an AppService is created, the connections fail with error
Unhandled exception. System.Security.Authentication.AuthenticationException: Authentication failed because the remote party sent a TLS alert: 'InternalError'.
 ---> System.ComponentModel.Win32Exception (0x80090326): The message received was unexpected or badly formatted.
   --- End of inner exception stack trace ---
   at Couchbase.Core.ClusterContext.GetOrCreateBucketAsync(String name)
   at CloudExample.accessbucket(ICluster cluster, String bucketName) in C:\Users\L50\git\couchbase-dotnet-sample\App1\Program.cs:line 41
   at CloudExample.Main() in C:\Users\L50\git\couchbase-dotnet-sample\App1\Program.cs:line 34
   at Program.<Main>$(String[] args) in C:\Users\L50\git\couchbase-dotnet-sample\App1\Program.cs:line 9
   at Program.<Main>(String[] args)




## App2
App2 executes the same logic as App1, but does it inline in the Main method.

App2 works as expected - the documents are inserted into each of the three buckets, and the UTC timestamp is updated upon successive executions.

## App3
App3 is an earlier version of App1, which initially didn't work. This is here for documentation purposes

Note that the calls to accessbucket are not awaited. **There are no compile-time or run-time errors, but the calls do not update the timestamps!**

## App4
App4 builds on App1 and App2 to reproduce a Render use case in simpler form, by adding an attachment to an existing document. 

The attachment is greater than 8k, which requires a WAF rule modification in the associated database, via Couchbase support.
The attachment happens to be an audio file, but I haven't tried to determine if that matters; I believe the 8k limit applies to attachments of any content type.

## App5
App5 represents a simplification of a second Render use case -- adding Sync Gateway users to three buckets

## WebApp1
WebApp1 builds on the success of App1, extending it to include the Couchbase Dependency Injection (DI) extension. This more closely matches Launchpad. 

This introduces the use of ASP.NET and it's dependency injection framework

dotnet new webapp -o WebApp1 --no-https -f net6.0
dotnet watch

WebApp1 works as expected - it updates the document "last_successful_timestamp" in buckets "logs" and "render"


