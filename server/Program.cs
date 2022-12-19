using System.Runtime.InteropServices;
using Topshelf;
using Topshelf.Runtime.DotNetCore;
class Program
{

    public static void Main(string[] args)
    {
        DirectoryInfo storagePath = null;
        int port = 4456;

        var rc = HostFactory.Run(x =>                                   
        {
            x.AddCommandLineDefinition("storageFolder", f => { storagePath = new DirectoryInfo(f); });
            x.AddCommandLineDefinition("port", p => { port = int.TryParse(p, out int tempint) ? tempint: 4456 ;});
            if (
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            )
            {
              x.UseEnvironmentBuilder(
                target => new DotNetCoreEnvironmentBuilder(target)
              );
            }
            x.Service<MetricGatheringServer>(s =>                                   
            {
               s.ConstructUsing(name => new MetricGatheringServer(new FlatFileMetricStorage(storagePath), port));                
               s.WhenStarted(mgs => mgs.Start());                         
               s.WhenStopped(mgs => mgs.Stop());                          
            });
            x.RunAsLocalSystem();                                       

            x.SetDescription("Gathers metrics to stream to a host");                   
            x.SetDisplayName("MetricGatheringServer");                                  
            x.SetServiceName("MetricGatheringServer");                                  
        });                                                             

        var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());  
        Environment.ExitCode = exitCode;       
    }
    
}