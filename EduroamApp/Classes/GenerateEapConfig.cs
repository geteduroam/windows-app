namespace EduroamApp
{       
    /// <summary>
    /// Stores information found in json for generating EAP config installer.
    /// </summary>
    public class GenerateEapConfig
    {
        // Properties
        public int Status { get; set; }
        public Datum Data { get; set; }
        public string Tou { get; set; }

        public class Datum
        {
            // Properties
            public string Profile { get; set; }
            public string Device { get; set; }
            public string Link { get; set; }
            public string Mime { get; set; }
        }
    }
}
