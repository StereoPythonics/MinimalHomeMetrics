using System.Threading.Tasks.Dataflow;
using Tcp.Framing;

public class MetricGatheringLink
{
    IObjectEnumerator<Metric> MetricSource { get; set; }
    public BufferBlock<Metric> receivedMetrics { get; set; }

    public MetricGatheringLink(IObjectEnumerator<Metric> metricSource)
    {
        receivedMetrics = new BufferBlock<Metric>();
        MetricSource = metricSource;
    }
    public void Start()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Waiting for metrics");
                    List<Metric> metrics = await MetricSource.ReadAsyncEnumerable(new CancellationTokenSource().Token).Take(1).ToListAsync();
                    Console.WriteLine(string.Join(",",metrics.Select(m => Utf8Json.JsonSerializer.ToJsonString(m))));
                    metrics.ForEach(m => receivedMetrics.Post(m));
                }
                catch (InvalidDataException e)
                {
                    Console.WriteLine(e.GetType());
                    Console.WriteLine(e.Message);
                    break;
                }
            }
        });
    }
}
