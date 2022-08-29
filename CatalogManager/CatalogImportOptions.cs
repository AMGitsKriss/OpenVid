using System.Collections.Generic;

namespace CatalogManager
{
    public class CatalogImportOptions
    {
        public string ImportDirectory { get; set; }
        public List<EncoderPresetOptions> EncoderPresets { get; set; }
    }

    public class EncoderPresetOptions
    {
        public string Encoder { get; set; }
        public string RenderSpeed { get; set; }
        public string Format { get; set; }
        public double Quality { get; set; }
        public int MaxHeight { get; set; }
    }
}
