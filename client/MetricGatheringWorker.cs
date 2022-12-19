using Tcp.Framing;
using System.Net.Sockets;
using System;

public class MetricGatheringWorker
{
    readonly Config config;
    TcpClient? tcpClient;
    BetterFileSystemChangeWatcher? watcher;
    IObjectEnumerator<Metric>? Streamer;
    int retryMilliseconds = 10000;

    public MetricGatheringWorker(Config config)
    {
        this.config = config;
    }
    

    public void Start()
    {
        Console.WriteLine("Starting connection");
        Task.Run(() => Console.WriteLine("Tasks are supported"));
        Task.Run(async () => await TryConnectToServer());
    }
    public void Stop() { }
    private async Task TryConnectToServer()
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
                Console.WriteLine($"Unable to reach metrics server, retrying in {retryMilliseconds}ms");
                await Task.Delay(retryMilliseconds);
                continue;
            }
            if (tcpClient.Connected) break;
        }
        Console.WriteLine($"Connection to metrics server Established");
        Streamer = new ObjectStreamer<Metric>(tcpClient.GetStream());
        watcher = new BetterFileSystemChangeWatcher(config.StoragePath,async e => await TryGetAndPublishMetrics(e), TimeSpan.FromSeconds(1));
    }
    private async Task TryGetAndPublishMetrics(FileSystemEventArgs e)
    {
        Console.WriteLine($"Metric update detected in {e.FullPath}");
        var thing = async () => {
            if (File.Exists(e.FullPath) && tcpClient.Connected)
            {
                await Task.Delay(1000);
                string text = File.ReadAllText(e.FullPath);
                if(string.IsNullOrEmpty(text))
                {   
                    Console.WriteLine($"{e.FullPath} empty, skipping");
                }
                if (!string.IsNullOrEmpty(text))
                {
                    Console.WriteLine("FileText:");
                    Console.WriteLine(text);
                    try{
                    var metrics = Utf8Json.JsonSerializer.Deserialize<List<Metric>>(text);
                    await Streamer.WriteEnumerable(metrics);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }};
        try
        {
            await thing();
        }
        catch (Exception exception)
        {
            Console.WriteLine("Bad connection to Server - Reconnecting");
            Console.WriteLine(exception);
            Console.WriteLine(exception.Message);
            tcpClient.Close();
            await TryConnectToServer();
            await thing();
            
        }
        
    }
    
}