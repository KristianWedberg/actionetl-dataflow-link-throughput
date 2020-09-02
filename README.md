﻿# actionETL Dataflow Throughput Benchmark

[**actionETL**](https://envobi.com) is a cross-platform ETL library for easily writing 
high performance, highly productive 
[ETL (Extract, Transform, Load)](https://en.wikipedia.org/wiki/Extract,_transform,_load) 
data processing applications in C#, VB etc. running on Windows and Linux.

This benchmark measures the aggregate link throughput when pumping rows across 1 to 1024
**actionETL** [dataflow links](https://envobi.com/dataflow), up to **340 million rows/s** 
on an old 4-core PC:

<img src="https://envobi.com/wp-content/uploads/2020/08/aggregate-link-throughput-chart-636x345-1.png" alt="Aggregate link throughput" />

Read the blog post 
[Blazingly Fast ETL Dataflow with .NET and actionETL](https://envobi.com/post/blazingly-fast-etl-dataflow-dotnet-actionetl)
for full details.


## Install and Run Benchmark

**actionETL** uses `dotnet new` templates and it's easy to install and run this benchmark:

1. Get an **actionETL** [free 30-day trial license](https://envobi.com/trial)
2. Download this project and put the received license in the "actionetl.license.json" file
3. Run the project
   ```c#
   cd actionETL.Demo.DataflowThroughput
   dotnet run --configuration Release
   ```
 
> [!NOTE]
> We create thousands of workers, so logging of `Info` messages has been disabled 
> to focus on the benchmark results. You can change to default logging by changing 
> `minlevel="Warn"` to `minlevel="Info"` in "nlog.config" for the file destination:
> ```xml
> <logger name="*" minlevel="Warn" writeTo="ToFile" />
> ```


## License

This benchmark project is released into the public domain.