namespace Travel.Models
{
    public class SiteData
    {
        public List<KeyLabel> Continents { get; set; } = new List<KeyLabel>();
        public List<KeyLabel> Categories { get; set; } = new List<KeyLabel>();
        public List<KeyLabel> Themes { get; set; } = new List<KeyLabel>();
        public List<KeyLabel> TipTypes { get; set; } = new List<KeyLabel>();
        public List<Place> Countries { get; set; } = new List<Place>();
        public List<Story> Posts { get; set; } = new List<Story>();
        public List<Photo> Photos { get; set; } = new List<Photo>();
        public List<Tip> Tips { get; set; } = new List<Tip>();
    }

    public class KeyLabel
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class Place
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Continent { get; set; } = string.Empty;
        public string Tagline { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public List<string> Highlights { get; set; } = new List<string>();
        public string Image { get; set; } = string.Empty;
    }

    public class Story
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int Minutes { get; set; }
        public string Excerpt { get; set; } = string.Empty;
        public List<string> Body { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public string Image { get; set; } = string.Empty;
    }

    public class Photo
    {
        public string Id { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Continent { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }

    public class Tip
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
