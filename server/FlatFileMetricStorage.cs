using System.Text;

public class FlatFileMetricStorage : IMetricStorage
{
    DirectoryInfo target;
    public FlatFileMetricStorage(DirectoryInfo targetFolder){
        if(!targetFolder.Exists) targetFolder.Create();
        target = targetFolder;
    }
    public async Task StoreMetrics(IEnumerable<Metric> metrics)
    {
        var groups = metrics.GroupBy(m => GetDatedMetricFileName(m)).ToList();
        foreach(var g in groups){
            await WriteToFile(new FileInfo(Path.Combine(target.FullName, g.Key)),g.ToList());
        }
    }

    private string GetDatedMetricFileName(Metric metric)
    {
        return $"{metric.Name ?? "null"}_{metric.source ?? null}_{metric.TimeStamp.ToUniversalTime().ToString("yyyy-MM-dd")}.json";
    }

    public async Task WriteToFile(FileInfo file, List<Metric> metrics)
    {
        using( FileStream fs = file.OpenWrite() )
        {   
            bool append = fs.Length > 1;
            if(append) {fs.Seek(-1,SeekOrigin.End);}
            using MemoryStream ms = new MemoryStream();
            await Utf8Json.JsonSerializer.SerializeAsync(ms, metrics.ToArray());
            ms.Seek(append ? 1 : 0, SeekOrigin.Begin);
            if(append){fs.Write(Encoding.UTF8.GetBytes(",\n"));}
            ms.CopyTo(fs);
        }
    }
}