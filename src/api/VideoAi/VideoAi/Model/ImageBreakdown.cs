namespace VideoAi.Model
{
    public class ImageBreakdown
    {
        public ImageCategory[] Categories { get; set; }
        public ImageDescription Description { get; set; }
    }

    public class ImageCategory
    {
        public string Name { get; set; }
        public decimal Score { get; set; }
    }

    public class ImageDescription
    {
        public string[] Tags { get; set; }

        public ImageCaption[] Captions { get; set; }
    }

    public class ImageCaption
    {
        public string Text { get; set; }
        public double Confidence { get; set; }
    }
}
