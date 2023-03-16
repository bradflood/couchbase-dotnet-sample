# Simple couchbase code to troubleshoot issues with dotnet SDK and Capella

All Apps are dotnet console apps, using dotnet 6 and couchbase sdk 3.4.4.

In the target capella database, there are three buckets: render, renderaudio and logs


## App1
App1 upserts a single document entitled "last_successful_timestamp" into all three buckets, via a method call. The file run.out contains debug level output. 

App1 works as expected - the documents are inserted into each of the three buckets, and the UTC timestamp is updated upon successive executions.


## App2
App2 executes the same logic as App1, but does it inline in the Main method.

App2 works as expected - the documents are inserted into each of the three buckets, and the UTC timestamp is updated upon successive executions.




