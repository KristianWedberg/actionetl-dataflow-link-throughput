using actionETL;
using actionETL.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace actionETL.DataflowLinkThroughput
{
    static class Program
    {
        static async Task Main()
        {
            const long desiredAggregateLinkRows = 1_000_000_000;
            var testCases = new (int sources, int downstreamLinksPerSource)[]
            {
                (1,1) // Warm up the JIT etc. to get consistent timings
                , (1,1), (2,1), (3,1), (4,1), (16,1), (64,1), (256,1), (1024,1) // Many Sources, No Transforms
                , (1,2), (1,3), (1,4), (1,16), (1,64), (1,256), (1,1024) // Single Source, Many Transforms 
                , (2,2), (4,4), (8,8), (16,16), (32,32) // Multiple Sources and Transforms 
            };
            WorkerSystem workerSystem = null;
            var results = new StringBuilder(Environment.NewLine + "AggregateLinkRows, Workers, Sources, DownstreamLinksPerSource, TotalLinks, Duration (s), AggregateLinkThroughput (Million Rows/s)" + Environment.NewLine);

            foreach (var (numberOfSources, downstreamLinksPerSource) in testCases)
            {
                int numberOfLinks = numberOfSources * downstreamLinksPerSource;
                long rowsPerSource = (long)Math.Ceiling((double)desiredAggregateLinkRows / numberOfLinks);
                long actualAggregateLinkRows = numberOfSources * rowsPerSource * downstreamLinksPerSource; // Avoid round-off errors

                workerSystem = new WorkerSystem()
                    .Root(ws =>
                    {
                        for (int s = 0; s < numberOfSources; s++)
                        {
                            var rrs = new RepeatRowsSource<MyRow>(ws, $"Source {s}", rowsPerSource, new MyRow(42))
                                { SendTemplateRows = true };
                            var output = rrs.Output;

                            for (int l = 1; l < downstreamLinksPerSource; l++)
                                output = output.Link.MulticastTransform($"Transform {s} - {l}", 1).TypedOutputs[0];

                            output.Link.TrashTarget($"Target {s}");
                        }
                    });

                GC.Collect(); // Level-set memory allocation
                GC.Collect();

                (await workerSystem.StartAsync()) // Run the worker system
                    .ThrowOnFailure();

                // Log the result
                double totalSeconds = workerSystem.RunningDuration.Value.TotalSeconds;
                double throughput = actualAggregateLinkRows / totalSeconds / 1_000_000.0;
                int numberOfWorkers = numberOfSources * (downstreamLinksPerSource + 1);
                var m = $@"{actualAggregateLinkRows,11}, {numberOfWorkers,6}, {numberOfSources,6}, {downstreamLinksPerSource,6}, {numberOfLinks,6}, {totalSeconds,7:N3}, {throughput,9:F2}{Environment.NewLine}";
                results.Append(m);
                workerSystem.Logger.Warn(ALogCategory.ProgressWorker, $"Workers={numberOfWorkers}, Sources={numberOfSources}, DownstreamLinksPerSource={downstreamLinksPerSource}, TotalLinks={numberOfLinks}, Duration (s)={totalSeconds:N3}, AggregateLinkThroughput (Million Rows/s)={throughput:F2}");
            }

            workerSystem.Logger.Warn(ALogCategory.UserInformation, results.ToString());
        }
    }

    class MyRow
    {
        public long Data { get; }

        public MyRow(long data)
        {
            Data = data;
        }
    }
}