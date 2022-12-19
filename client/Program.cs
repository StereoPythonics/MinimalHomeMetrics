
using System.Runtime.InteropServices;
using Topshelf;
using Topshelf.Runtime.DotNetCore;

partial class Program
{
    
    public static void Main(string[] args)
    {
        string folder = null;
        string port = null;
        string ip = null;

        var rc = HostFactory.Run(x =>                                   
        {
            x.AddCommandLineDefinition("folder", f => { folder = f; });
            x.AddCommandLineDefinition("port", p => { port = p; });
            x.AddCommandLineDefinition("ip", i => { ip = i; });
            if (
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            )
            {
              x.UseEnvironmentBuilder(
                target => new DotNetCoreEnvironmentBuilder(target)
              );
            }
            x.Service<MetricGatheringWorker>(s =>                                   
            {
               s.ConstructUsing(name => new MetricGatheringWorker(new Config(new string[]{folder, port, ip})));                
               s.WhenStarted(mgw => mgw.Start());                         
               s.WhenStopped(mgw => mgw.Stop());                          
            });
            x.RunAsLocalSystem();                                       

            x.SetDescription("Gathers metrics to stream to a host");                   
            x.SetDisplayName("MetricGatheringWorker");                                  
            x.SetServiceName("MetricGatheringWorker");                                  
        });                                                             

        var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());  
        Environment.ExitCode = exitCode;
    }
}
