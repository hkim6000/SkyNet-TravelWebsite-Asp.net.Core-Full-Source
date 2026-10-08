using System.Globalization;
using System.Text;
using System.Text.Json;
using Travel.Models;
using SkyNet;

namespace Travel.codes
{
    public class Gallery : WebPage
    {
        public override async Task OnInitialized()
        {
            HtmlDoc.SetTitle("Gallery | Northbound Notes");
            HtmlDoc.AddMetaElement("viewport", "width=device-width, initial-scale=1");
            HtmlDoc.AddMetaElement("description", "Travel photos from twelve countries.");

            SiteData site = await LoadSite();
            string theme = Pick(QueryValue("theme"), site.Themes.Select(t => t.Key));
            string cont = Pick(QueryValue("cont"), site.Continents.Select(c => c.Key));
            List<Photo> list = Photos(site, theme, cont);
            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText
                .Replace("{plhd_chips}", Chips(site.Themes, "theme", theme))
                .Replace("{plhd_theme}", theme)
                .Replace("{plhd_v_cont}", cont)
                .Replace("{plhd_count}", CountText(list.Count, "photo", "photos"))
                .Replace("{plhd_grid}", Masonry(site, list));
        }

        public async Task<ApiResponse> Filter()
        {
            ApiResponse response = new ApiResponse();
            SiteData site = await LoadSite();
            string theme = Pick(GetDataValue("theme"), site.Themes.Select(t => t.Key));
            string cont = Pick(GetDataValue("cont"), site.Continents.Select(c => c.Key));
            List<Photo> list = Photos(site, theme, cont);
            response.SetElementContents("ga-grid", Masonry(site, list));
            response.SetElementContents("ga-count", CountText(list.Count, "photo", "photos"));
            return response;
        }

        public async Task<ApiResponse> View()
        {
            ApiResponse response = new ApiResponse();
            SiteData site = await LoadSite();
            string theme = Pick(GetDataValue("theme"), site.Themes.Select(t => t.Key));
            string cont = Pick(GetDataValue("cont"), site.Continents.Select(c => c.Key));
            string id = (GetDataValue("id") ?? string.Empty).Trim();
            List<Photo> list = Photos(site, theme, cont);
            int i = list.FindIndex(p => p.Id == id);
            if (i < 0)
            {
                return response;
            }
            Photo ph = list[i];
            Place c = PlaceOf(site, ph.Country);
            StringBuilder sb = new StringBuilder();
            sb.Append("<img class=\"ga-lb-img\" src=\"" + ph.Image + "\" alt=\"" + HtmlEncode(ph.Caption) + "\">");
            sb.Append("<div class=\"ga-lb-cap\"><div><b>" + HtmlEncode(ph.Caption) + "</b><span>" + HtmlEncode(c.Name) + " &middot; " + HtmlEncode(LabelOf(site.Themes, ph.Theme)) + " &middot; " + (i + 1) + " of " + list.Count + "</span></div>");
            sb.Append("<div class=\"ga-lb-nav\">");
            if (i > 0)
            {
                sb.Append("<button type=\"button\" data-step=\"prev\" aria-label=\"Previous\" onclick=\"GalleryJs.photo('" + list[i - 1].Id + "')\">&larr;</button>");
            }
            if (i + 1 < list.Count)
            {
                sb.Append("<button type=\"button\" data-step=\"next\" aria-label=\"Next\" onclick=\"GalleryJs.photo('" + list[i + 1].Id + "')\">&rarr;</button>");
            }
            sb.Append("<a href=\"Destination?c=" + c.Key + "\">" + HtmlEncode(c.Name) + " guide &rarr;</a></div></div>");
            response.SetElementContents("ga-lb-body", sb.ToString());
            response.ExecuteScript("GalleryJs.openLb();");
            return response;
        }

        private static List<Photo> Photos(SiteData site, string theme, string cont)
        {
            return site.Photos.Where(p => (theme == string.Empty || p.Theme == theme) && (cont == string.Empty || p.Continent == cont)).ToList();
        }

        private static string Masonry(SiteData site, List<Photo> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"ga-empty\">No photos match these filters.</div>";
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                Photo p = list[i];
                string shape = i % 5 == 0 ? " ga-tall" : string.Empty;
                sb.Append("<button type=\"button\" class=\"ga-ph" + shape + "\" onclick=\"GalleryJs.photo('" + p.Id + "')\"><img src=\"" + p.Image + "\" alt=\"" + HtmlEncode(p.Caption) + "\" loading=\"lazy\">");
                sb.Append("<span><b>" + HtmlEncode(p.Caption) + "</b><small>" + HtmlEncode(PlaceOf(site, p.Country).Name) + "</small></span></button>");
            }
            return sb.ToString();
        }

        public async Task<ApiResponse> Search()
        {
            ApiResponse response = new ApiResponse();
            string q = Clip(GetDataValue("q"));
            SiteData site = await LoadSite();
            List<string> rows = new List<string>();
            int total = 0;
            if (q.Length >= 2)
            {
                foreach (Place c in site.Countries.Where(c => Has(c.Name, q) || Has(c.Tagline, q) || c.Highlights.Any(h => Has(h, q))))
                {
                    total++;
                    rows.Add(Sugg("Place", c.Image, c.Name, c.Tagline, "Destination?c=" + c.Key));
                }
                foreach (Story p in site.Posts.Where(p => Has(p.Title, q) || Has(p.Excerpt, q) || Has(PlaceOf(site, p.Country).Name, q)).OrderByDescending(p => p.Date))
                {
                    total++;
                    rows.Add(Sugg("Story", p.Image, p.Title, PlaceOf(site, p.Country).Name + " · " + DateText(p.Date), "Post?id=" + p.Id));
                }
                foreach (Tip t in site.Tips.Where(t => Has(t.Title, q) || Has(t.Text, q)))
                {
                    total++;
                    rows.Add(Sugg("Tip", string.Empty, t.Title, LabelOf(site.TipTypes, t.Type) + " travel", "Tips?type=" + t.Type));
                }
            }
            StringBuilder sb = new StringBuilder();
            if (total == 0)
            {
                sb.Append("<div class=\"ga-sg-none\">No results for &ldquo;" + HtmlEncode(q) + "&rdquo;</div>");
            }
            else
            {
                foreach (string r in rows.Take(7))
                {
                    sb.Append(r);
                }
                sb.Append("<div class=\"ga-sg-foot\">" + total + (total == 1 ? " result" : " results") + " across places, stories and tips</div>");
            }
            response.SetElementContents("ga-sugg", sb.ToString());
            response.ExecuteScript("GalleryJs.openSugg();");
            return response;
        }

        public async Task<ApiResponse> Subscribe()
        {
            ApiResponse response = new ApiResponse();
            
            string email = (GetDataValue("email") ?? string.Empty).Trim();
            if (!IsEmail(email))
            {
                response.SetElementContents("ga-nl-msg", "<span class=\"ga-err\">Please enter a valid email address.</span>");
                return response;
            }
            response.SetElementContents("ga-nl-msg", "<span class=\"ga-ok\">Thanks! " + HtmlEncode(email) + " is on the list. (Demo only &mdash; nothing was stored.)</span>");
            response.ExecuteScript("GalleryJs.subscribed();");
            return response;
        }

        private static string Sugg(string type, string img, string title, string sub, string href)
        {
            string pic = img == string.Empty ? "<span class=\"ga-sg-i\">&#10003;</span>" : "<img src=\"" + img + "\" alt=\"\">";
            return "<a class=\"ga-sg\" href=\"" + href + "\">" + pic + "<span><b>" + HtmlEncode(title) + "</b><small>" + type + " &middot; " + HtmlEncode(sub) + "</small></span></a>";
        }

        private async Task<SiteData> LoadSite()
        {
            string file = Path.Combine(DataPath ?? string.Empty, "site.json");
            if (!File.Exists(file))
            {
                file = Path.Combine(Directory.GetCurrentDirectory(), "data", "site.json");
            }
            if (!File.Exists(file))
            {
                return new SiteData();
            }
            string json = await File.ReadAllTextAsync(file);
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<SiteData>(json, options) ?? new SiteData();
        }

        private static string Clip(string? value)
        {
            string q = (value ?? string.Empty).Trim();
            return q.Length > 40 ? q.Substring(0, 40) : q;
        }

        private static string Pick(string? value, IEnumerable<string> allowed)
        {
            string v = (value ?? string.Empty).Trim().ToLowerInvariant();
            return allowed.Contains(v) ? v : string.Empty;
        }

        private static bool Has(string text, string q)
        {
            return (text ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEmail(string v)
        {
            if (v.Length < 5 || v.Length > 80 || v.Contains(' '))
            {
                return false;
            }
            int at = v.IndexOf('@');
            int dot = v.LastIndexOf('.');
            return at > 0 && at == v.LastIndexOf('@') && dot > at + 1 && dot < v.Length - 1;
        }

        private static string DateText(string ymd)
        {
            DateTime d;
            if (DateTime.TryParseExact(ymd, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            {
                return d.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
            }
            return ymd;
        }

        private static string CountText(int n, string one, string many)
        {
            return n + " " + (n == 1 ? one : many);
        }

        private static string LabelOf(List<KeyLabel> list, string key)
        {
            return list.Where(x => x.Key == key).Select(x => x.Label).FirstOrDefault() ?? key;
        }

        private static Place PlaceOf(SiteData site, string key)
        {
            return site.Countries.FirstOrDefault(c => c.Key == key) ?? new Place { Key = key, Name = key };
        }

        private static string Chips(List<KeyLabel> items, string group, string active)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<button type=\"button\" class=\"ga-chip" + (active == string.Empty ? " ga-act" : string.Empty) + "\" onclick=\"GalleryJs.chip(this, '" + group + "', '')\">All</button>");
            foreach (KeyLabel k in items)
            {
                sb.Append("<button type=\"button\" class=\"ga-chip" + (k.Key == active ? " ga-act" : string.Empty) + "\" onclick=\"GalleryJs.chip(this, '" + group + "', '" + k.Key + "')\">" + HtmlEncode(k.Label) + "</button>");
            }
            return sb.ToString();
        }

        private static string PostCard(SiteData site, Story p, bool big)
        {
            Place c = PlaceOf(site, p.Country);
            StringBuilder sb = new StringBuilder();
            sb.Append("<article class=\"ga-card" + (big ? " ga-card-big" : string.Empty) + "\"><a class=\"ga-card-img\" href=\"Post?id=" + p.Id + "\"><img src=\"" + p.Image + "\" alt=\"\" loading=\"lazy\">");
            sb.Append("<span class=\"ga-tag\">" + HtmlEncode(LabelOf(site.Categories, p.Category)) + "</span></a>");
            sb.Append("<div class=\"ga-card-b\"><div class=\"ga-cmeta\"><span>" + HtmlEncode(c.Name) + "</span><span>" + DateText(p.Date) + "</span><span>" + p.Minutes + " min</span></div>");
            sb.Append("<h3><a href=\"Post?id=" + p.Id + "\">" + HtmlEncode(p.Title) + "</a></h3><p>" + HtmlEncode(p.Excerpt) + "</p>");
            sb.Append("<a class=\"ga-rmore\" href=\"Post?id=" + p.Id + "\">Read the story &rarr;</a></div></article>");
            return sb.ToString();
        }

        private static string PostGrid(SiteData site, List<Story> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"ga-empty\">No stories match these filters yet.</div>";
            }
            StringBuilder sb = new StringBuilder();
            foreach (Story p in list)
            {
                sb.Append(PostCard(site, p, false));
            }
            return sb.ToString();
        }

        private static string PlaceCard(SiteData site, Place c)
        {
            int n = site.Posts.Count(p => p.Country == c.Key);
            return "<a class=\"ga-place\" href=\"Destination?c=" + c.Key + "\"><img src=\"" + c.Image + "\" alt=\"\" loading=\"lazy\"><span class=\"ga-place-t\"><small>" + HtmlEncode(LabelOf(site.Continents, c.Continent)) + "</small><b>" + HtmlEncode(c.Name) + "</b><em>" + HtmlEncode(c.Tagline) + "</em><u>" + n + (n == 1 ? " story" : " stories") + "</u></span></a>";
        }

        private static string PlaceGrid(SiteData site, List<Place> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Place c in list)
            {
                sb.Append(PlaceCard(site, c));
            }
            return sb.ToString();
        }

        private static string Strip(List<Photo> list, string href)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Photo p in list)
            {
                sb.Append("<a class=\"ga-sph\" href=\"" + href + "\"><img src=\"" + p.Image + "\" alt=\"" + HtmlEncode(p.Caption) + "\" loading=\"lazy\"><span>" + HtmlEncode(p.Caption) + "</span></a>");
            }
            return sb.ToString();
        }
    }
}
