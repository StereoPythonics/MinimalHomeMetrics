public class Config
{
    public int Port { get; }
    public DirectoryInfo StoragePath { get; }
    public string Ip { get; }
    public Config(string[] args)
    {
        StoragePath = new DirectoryInfo(
        args.Length > 0 ?
        args[0] :
        Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "No Sorry this does not work", "MetricStorage")
        );

        if (!StoragePath.Exists) StoragePath.Create();

        Port = args.Length > 1 ?
            int.TryParse(args[1], out int tempint) ?
                tempint
                : 4456
            : 4456;

        Ip = args.Length > 2 ? args[2] : "127.0.0.1";

        Console.WriteLine($"Starting metric gathering worker pulling metrics from {StoragePath}, and publishing on {Port} to {Ip}");
    }
}

