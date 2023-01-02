using Tcp.Framing;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;

public class MetricGatheringWorker
{
    readonly Config config;
    BetterFileSystemChangeWatcher? watcher;
    BufferBlock<Metric> outboundBuffer;
    ActionBlock<Metric> streamWriteBlock;
    TcpClient? tcpClient;
    int tcpClientRetryMelliseconds = 10000;
    bool ConnectionStatusGood;
    IObjectEnumerator<Metric>? Streamer;
    

    public MetricGatheringWorker(Config config)
    {
        this.config = config;
        ConnectionStatusGood = false;
        outboundBuffer = new BufferBlock<Metric>(new DataflowBlockOptions(){EnsureOrdered = true});
        streamWriteBlock = new ActionBlock<Metric>(metric => {
            try
            {
                this.Streamer?.WriteEnumerable(new []{metric});
            }
            catch(Exception e)
            {
                Console.WriteLine("Something went wrong sending metrics to the server");
                Console.WriteLine(e.Message);
            }
        });
        outboundBuffer.LinkTo(streamWriteBlock);
    }

    public async Task Start()
    {
        Console.WriteLine("Starting connection");
        Streamer = await TryConnectToServer();
        watcher = new BetterFileSystemChangeWatcher(config.StoragePath, async e => await TryGetMetricsFromFile(e), TimeSpan.FromSeconds(1));
    }
    public async Task Stop()
    {

    }
    private async Task<IObjectEnumerator<Metric>> TryConnectToServer()
    {
        while (true)
        {
            try
            {
                Console.WriteLine($"Attempting a connection to metrics server");
                tcpClient = new TcpClient();
                await tcpClient?.ConnectAsync(config.Ip, config.Port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"Unable to reach metrics server, retrying in {tcpClientRetryMelliseconds}ms");
                await Task.Delay(tcpClientRetryMelliseconds);
                continue;
            }
            if (tcpClient.Connected) break;
        }
        Console.WriteLine($"Connection to metrics server Established, setting up object streamer");
        var enumerator = new ObjectStreamer<Metric>(tcpClient.GetStream());
        ConnectionStatusGood = true;
        enumerator.ConnectionDropped += async (o, e) =>
        {
            Console.WriteLine("Connection Drop Detected");
            ConnectionStatusGood = false;
            Streamer = await TryConnectToServer();
        };
        Console.WriteLine("Object Stream established");
        return enumerator;
    }
    private async Task TryGetMetricsFromFile(FileSystemEventArgs e)
    {
        await Task.Delay(1000); //sometimes events trigger as part of a clear+overwrite cycle
        if (!File.Exists(e.FullPath))
        {

            Console.WriteLine($"{e.FullPath} does not exist, skipping");
            return;
        }
        string text = File.ReadAllText(e.FullPath);
        if (string.IsNullOrEmpty(text))
        {
            Console.WriteLine($"{e.FullPath} empty, skipping");
            return;
        }
        else
        {
            try
            {
                var metrics = Utf8Json.JsonSerializer.Deserialize<List<Metric>>(text);
                metrics.ForEach(m => outboundBuffer.Post(m));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}