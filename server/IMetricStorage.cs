public interface IMetricStorage
{
    Task StoreMetrics(IEnumerable<Metric> metric);
}
