public class BetterFileSystemChangeWatcher
{
    private readonly TimeSpan threshold;
    FileSystemWatcher watcher;
    Dictionary<string,DateTime> fileStates;

    public BetterFileSystemChangeWatcher(DirectoryInfo directory, Action<FileSystemEventArgs> action, TimeSpan threshold)
    {
        watcher = new FileSystemWatcher(directory.FullName);
        fileStates = new Dictionary<string, DateTime>();

        watcher.Changed += (o, e) => { if(testIfChangeIsGenuine(e)) action(e); };

        watcher.Filter = "*.*";
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        this.threshold = threshold;
    }
    bool testIfChangeIsGenuine(FileSystemEventArgs e)
    {
        var changetime = File.GetLastWriteTimeUtc(e.FullPath);
        if(fileStates.TryGetValue(e.FullPath, out DateTime lastChangeTime) && lastChangeTime + threshold < changetime)
        {
            fileStates[e.FullPath] = File.GetLastWriteTimeUtc(e.FullPath);
            return true;
        }
        else
        {
            
            fileStates.TryAdd(e.FullPath, File.GetLastWriteTimeUtc(e.FullPath));
            return false;
        }
    }
}
