namespace VideoAi
{

    public class VideoBreakdown
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public int DurationInSeconds { get; set; }
        public SummarizedInsights SummarizedInsights { get; set; }
        public Breakdown[] Breakdowns { get; set; }
    }

    public class SummarizedInsights
    {
        public string ThumbnailUrl { get; set; }
    }

    public class Breakdown
    {
        public string State { get; set; }
        public string ProcessingProgress { get; set; }
        public Insights Insights { get; set; }
    }

    public class Insights
    {
        public TranscriptBlock[] TranscriptBlocks { get; set; }
    }

    public class TranscriptBlock
    {
        public int Id { get; set; }
        public Annotation[] Annotations { get; set; }
    }

    public class Annotation
    {
        public string Name { get; set; }
    }
}
