using System.Threading.Tasks.Dataflow;
using System.Net.Sockets;
using System.Net;
using Tcp.Framing;
public class MetricGatheringServer
{
    private readonly int port;
    List<MetricGatheringLink> metricGatheringLinks { get;set; }
    IMetricStorage Storage { get; }
    ActionBlock<IEnumerable<Metric>> writeBlock {get;}
    BatchBlock<Metric> batchBlock {get;}
    CancellationTokenSource source;

    public MetricGatheringServer(IMetricStorage storage, int port)
    {
        
        metricGatheringLinks = new List<MetricGatheringLink>();
        Storage = storage;
        this.port = port;
        batchBlock = new BatchBlock<Metric>(1);
        writeBlock = new ActionBlock<IEnumerable<Metric>>(
            async metrics => await Storage.StoreMetrics(metrics),
            new ExecutionDataflowBlockOptions(){MaxDegreeOfParallelism = 1}
            );
        batchBlock.LinkTo(writeBlock);
        
    }

    public void Start()
    {

        source = new CancellationTokenSource();
        Console.WriteLine("Starting Server");
        TcpListener receiveListener = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
        receiveListener.Start();

        Console.WriteLine("Listeners set up, waiting for conections");
        var receiverSetUp = Task.Run(async () =>
        {
            while(!source.Token.IsCancellationRequested)
            {
                var newLink = await AwaitMetricGathererConnection(receiveListener,source.Token);
                this.RegisterNewMetricLink(newLink);
            }
        });
    }
    public void Stop()
    {
        Console.WriteLine("Stopping Server");
        source?.Cancel();
    }
    public static async Task<MetricGatheringLink> AwaitMetricGathererConnection(TcpListener receiveListener, CancellationToken cancellationToken)
    {
        TcpClient receiveClient = await receiveListener.AcceptTcpClientAsync(cancellationToken); //blocks waiting for client connection
        Console.WriteLine("New Gatherer connection");
        NetworkStream receiveserverNetworkStream = receiveClient.GetStream();
        IObjectEnumerator<Metric> receiveObjectStreamer = new ObjectStreamer<Metric>(receiveserverNetworkStream);
        return new MetricGatheringLink(receiveObjectStreamer);

    }
    public void RegisterNewMetricLink(MetricGatheringLink link)
    {
        metricGatheringLinks.Add(link);
        link.receivedMetrics.LinkTo(batchBlock);
        link.Start();
    }
}
