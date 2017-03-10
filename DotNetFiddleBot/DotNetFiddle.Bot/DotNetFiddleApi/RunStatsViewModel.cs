namespace DotNetFiddle.Bot.DotNetFiddleApi
{
    public class RunStatsViewModel
    {
        public string RunAt { get; set; }
        public string CompileTime { get; set; }
        public string ExecuteTime { get; set; }
        public string MemoryUsage { get; set; }
        public string CpuUsage { get; set; }
    }
}