# Simple couchbase code to troubleshoot issues with dotnet SDK and Capella

All Apps are dotnet console apps, using dotnet 6 and couchbase sdk 3.4.4.

In the target capella database, there are three buckets: render, renderaudio and logs


## App1
App1 upserts a single document entitled "last_successful_timestamp" into all three buckets, via a method call. The file run.out contains debug level output. 

App1 works as expected - the documents are inserted into each of the three buckets, and the UTC timestamp is updated upon successive executions.


## App2
App2 executes the same logic as App1, but does it inline in the Main method.

App2 works as expected - the documents are inserted into each of the three buckets, and the UTC timestamp is updated upon successive executions.

## App3
App3 is an earlier version of App1, which initially didn't work. This is here for documentation purposes

Note that the calls to accessbucket are not awaited. **There are no compile-time or run-time errors, but the calls do not update the timestamps!**

## WebApp1
WebApp1 builds on the success of App1, extending it to include the Couchbase Dependency Injection (DI) extension. This more closely matches Launchpad. 

This introduces the use of ASP.NET and it's dependency injection framework

dotnet new webapp -o WebApp1 --no-https -f net6.0
dotnet watch
