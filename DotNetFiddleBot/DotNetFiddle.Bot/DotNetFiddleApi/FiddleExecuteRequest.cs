namespace DotNetFiddle.Bot.DotNetFiddleApi
{
    public enum Compiler
    {
        Net45 = 1,
        Roslyn = 2
    }

    public enum Language
    {
        CSharp = 1,
        VbNet,
        FSharp
    }

    public enum ProjectType
    {
        Console = 1,
        Script,
        Mvc,
        Nancy
    }

    public class FiddleExecuteRequest
    {
        public Language Language { get; set; }
        public ProjectType ProjectType { get; set; }
        public Compiler Compiler { get; set; }
        public string CodeBlock { get; set; }
        public string[] ConsoleInputLines { get; set; }
        public MvcCodeBlock MvcCodeBlock { get; set; }
        public NuGetPackages[] NuGetPackages { get; set; }

    }
}