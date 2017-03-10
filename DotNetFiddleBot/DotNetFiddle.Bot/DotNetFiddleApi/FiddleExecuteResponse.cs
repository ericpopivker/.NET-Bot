namespace DotNetFiddle.Bot.DotNetFiddleApi
{
    public class ExecuteFiddleResponse
    {
        public string ConsoleOutput { get; set; }
        public RunStatsViewModel Stats { get; set; }
        public string WebPageHtmlOutput { get; set; }
        public bool IsConsoleInputRequested { get; set; }
        public bool HasErrors { get; set; }
        public bool HasCompilationErrors { get; set; }
    }
}