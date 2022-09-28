namespace CatalogManager.Models
{
    public class SubtitleFile
    {
        public string StreamId { get; internal set; }
        public string OutputFile { get; internal set; }
        public string OutputFolder { get; internal set; }
        public string SourceFileFullName { get; internal set; }
        public string Language { get; internal set; }
    }
}
