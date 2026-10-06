using System.Globalization;
using System.Text;
using System.Text.Json;
using Travel.Models;
using SkyNet;

namespace Travel.codes
{
    public class Tips : WebPage
    {
        public override async Task OnInitialized()
        {
            HtmlDoc.SetTitle("Travel Tips | Northbound Notes");
            HtmlDoc.AddMetaElement("viewport", "width=device-width, initial-scale=1");
            HtmlDoc.AddMetaElement("description", "Travel tips for every kind of traveler and a packing list builder.");

            SiteData site = await LoadSite();
            string type = Pick(QueryValue("type"), site.TipTypes.Select(t => t.Key));
            if (type == string.Empty)
            {
                type = "solo";
            }
            List<Tip> list = TipList(site, type);
            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText
                .Replace("{plhd_chips}", Chips(site.TipTypes, "type", type))
                .Replace("{plhd_type}", type)
                .Replace("{plhd_count}", CountText(list.Count, "tip", "tips"))
                .Replace("{plhd_grid}", TipGrid(site, list));
        }

        public async Task<ApiResponse> Filter()
        {
            ApiResponse response = new ApiResponse();
            SiteData site = await LoadSite();
            string type = Pick(GetDataValue("type"), site.TipTypes.Select(t => t.Key));
            List<Tip> list = TipList(site, type);
            response.SetElementContents("tp-grid", TipGrid(site, list));
            response.SetElementContents("tp-count", CountText(list.Count, "tip", "tips"));
            return response;
        }

        public async Task<ApiResponse> Pack()
        {
            ApiResponse response = new ApiResponse();
            SiteData site = await LoadSite();
            string type = Pick(GetDataValue("type"), site.TipTypes.Select(t => t.Key));
            string climate = Pick(GetDataValue("climate"), new[] { "warm", "mild", "cold" });
            int days;
            if (!int.TryParse(GetDataValue("days"), out days) || !new[] { 3, 7, 14, 30 }.Contains(days))
            {
                days = 7;
            }
            if (type == string.Empty)
            {
                type = "solo";
            }
            if (climate == string.Empty)
            {
                climate = "mild";
            }
            int sets = Math.Min(days, 7);
            int tops = Math.Min(days, 5);
            int bottoms = days <= 3 ? 1 : days <= 7 ? 2 : 3;
            List<KeyValuePair<string, List<string>>> groups = new List<KeyValuePair<string, List<string>>>
            {
                new KeyValuePair<string, List<string>>("Essentials", new List<string> { "Passport or ID", "Travel insurance details", "Bank cards and a little local cash", "Phone and charger", "Universal adapter", "Reusable water bottle", "Small first-aid kit", "Any personal medication" }),
                new KeyValuePair<string, List<string>>("Clothes", new List<string> { sets + " sets of underwear and socks", tops + " tops", bottoms + (bottoms == 1 ? " pair" : " pairs") + " of trousers or skirts", "Sleepwear", "Comfortable walking shoes" }),
            };
            if (days >= 14)
            {
                groups[1].Value.Add("Laundry bag and detergent sheets");
            }
            List<string> cl = climate == "warm"
                ? new List<string> { "Sun hat", "Sunscreen", "Light, breathable layers", "Sandals", "Insect repellent" }
                : climate == "cold"
                    ? new List<string> { "Insulated jacket", "Thermal base layers", "Hat and gloves", "Waterproof boots", "Lip balm" }
                    : new List<string> { "Light jacket", "One warm layer", "Compact umbrella", "Scarf" };
            groups.Add(new KeyValuePair<string, List<string>>(LabelOf(new List<KeyLabel> { new KeyLabel { Key = "warm", Label = "Warm weather" }, new KeyLabel { Key = "mild", Label = "Mild weather" }, new KeyLabel { Key = "cold", Label = "Cold weather" } }, climate), cl));
            Dictionary<string, List<string>> extra = new Dictionary<string, List<string>>
            {
                { "solo", new List<string> { "Copies of documents (digital and paper)", "Door-stop alarm", "Offline maps" } },
                { "couples", new List<string> { "Shared toiletry bag", "One nice outfit each", "Card game for slow evenings" } },
                { "family", new List<string> { "Snacks and small games", "Children's medicine", "Wet wipes", "Favorite comfort toy" } },
                { "backpacking", new List<string> { "Padlock", "Quick-dry towel", "Packing cubes", "Earplugs and eye mask" } },
                { "luxury", new List<string> { "Evening outfit", "Garment bag", "Dressier shoes" } },
                { "adventure", new List<string> { "Headlamp", "Daypack", "Rain shell", "Blister kit" } },
                { "food", new List<string> { "Antacids", "Reusable cutlery", "Notebook for recipes" } },
                { "remote", new List<string> { "Laptop and stand", "Power bank", "Noise-cancelling headphones", "Portable hotspot" } }
            };
            groups.Add(new KeyValuePair<string, List<string>>("For " + LabelOf(site.TipTypes, type).ToLowerInvariant() + " travel", extra[type]));
            int total = groups.Sum(g => g.Value.Count);
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"tp-pack-h\"><b>Your " + days + "-day " + climate + " " + HtmlEncode(LabelOf(site.TipTypes, type).ToLowerInvariant()) + " list</b><span>" + total + " items</span></div>");
            sb.Append("<div class=\"tp-pack-g\">");
            foreach (KeyValuePair<string, List<string>> g in groups)
            {
                sb.Append("<div class=\"tp-pack-c\"><h4>" + HtmlEncode(g.Key) + "</h4><ul>");
                foreach (string item in g.Value)
                {
                    sb.Append("<li><label><input type=\"checkbox\"><span>" + HtmlEncode(item) + "</span></label></li>");
                }
                sb.Append("</ul></div>");
            }
            sb.Append("</div>");
            response.SetElementContents("tp-pack", sb.ToString());
            return response;
        }

        private static List<Tip> TipList(SiteData site, string type)
        {
            return site.Tips.Where(t => type == string.Empty || t.Type == type).ToList();
        }

        private static string TipGrid(SiteData site, List<Tip> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Tip t in list)
            {
                sb.Append("<div class=\"tp-tip\"><span class=\"tp-tag\">" + HtmlEncode(LabelOf(site.TipTypes, t.Type)) + "</span><b>" + HtmlEncode(t.Title) + "</b><p>" + HtmlEncode(t.Text) + "</p></div>");
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
                sb.Append("<div class=\"tp-sg-none\">No results for &ldquo;" + HtmlEncode(q) + "&rdquo;</div>");
            }
            else
            {
                foreach (string r in rows.Take(7))
                {
                    sb.Append(r);
                }
                sb.Append("<div class=\"tp-sg-foot\">" + total + (total == 1 ? " result" : " results") + " across places, stories and tips</div>");
            }
            response.SetElementContents("tp-sugg", sb.ToString());
            response.ExecuteScript("TipsJs.openSugg();");
            return response;
        }

        public async Task<ApiResponse> Subscribe()
        {
            ApiResponse response = new ApiResponse();
            await Task.CompletedTask;
            string email = (GetDataValue("email") ?? string.Empty).Trim();
            if (!IsEmail(email))
            {
                response.SetElementContents("tp-nl-msg", "<span class=\"tp-err\">Please enter a valid email address.</span>");
                return response;
            }
            response.SetElementContents("tp-nl-msg", "<span class=\"tp-ok\">Thanks! " + HtmlEncode(email) + " is on the list. (Demo only &mdash; nothing was stored.)</span>");
            response.ExecuteScript("TipsJs.subscribed();");
            return response;
        }

        private static string Sugg(string type, string img, string title, string sub, string href)
        {
            string pic = img == string.Empty ? "<span class=\"tp-sg-i\">&#10003;</span>" : "<img src=\"" + img + "\" alt=\"\">";
            return "<a class=\"tp-sg\" href=\"" + href + "\">" + pic + "<span><b>" + HtmlEncode(title) + "</b><small>" + type + " &middot; " + HtmlEncode(sub) + "</small></span></a>";
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
            sb.Append("<button type=\"button\" class=\"tp-chip" + (active == string.Empty ? " tp-act" : string.Empty) + "\" onclick=\"TipsJs.chip(this, '" + group + "', '')\">All</button>");
            foreach (KeyLabel k in items)
            {
                sb.Append("<button type=\"button\" class=\"tp-chip" + (k.Key == active ? " tp-act" : string.Empty) + "\" onclick=\"TipsJs.chip(this, '" + group + "', '" + k.Key + "')\">" + HtmlEncode(k.Label) + "</button>");
            }
            return sb.ToString();
        }

        private static string PostCard(SiteData site, Story p, bool big)
        {
            Place c = PlaceOf(site, p.Country);
            StringBuilder sb = new StringBuilder();
            sb.Append("<article class=\"tp-card" + (big ? " tp-card-big" : string.Empty) + "\"><a class=\"tp-card-img\" href=\"Post?id=" + p.Id + "\"><img src=\"" + p.Image + "\" alt=\"\" loading=\"lazy\">");
            sb.Append("<span class=\"tp-tag\">" + HtmlEncode(LabelOf(site.Categories, p.Category)) + "</span></a>");
            sb.Append("<div class=\"tp-card-b\"><div class=\"tp-cmeta\"><span>" + HtmlEncode(c.Name) + "</span><span>" + DateText(p.Date) + "</span><span>" + p.Minutes + " min</span></div>");
            sb.Append("<h3><a href=\"Post?id=" + p.Id + "\">" + HtmlEncode(p.Title) + "</a></h3><p>" + HtmlEncode(p.Excerpt) + "</p>");
            sb.Append("<a class=\"tp-rmore\" href=\"Post?id=" + p.Id + "\">Read the story &rarr;</a></div></article>");
            return sb.ToString();
        }

        private static string PostGrid(SiteData site, List<Story> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"tp-empty\">No stories match these filters yet.</div>";
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
            return "<a class=\"tp-place\" href=\"Destination?c=" + c.Key + "\"><img src=\"" + c.Image + "\" alt=\"\" loading=\"lazy\"><span class=\"tp-place-t\"><small>" + HtmlEncode(LabelOf(site.Continents, c.Continent)) + "</small><b>" + HtmlEncode(c.Name) + "</b><em>" + HtmlEncode(c.Tagline) + "</em><u>" + n + (n == 1 ? " story" : " stories") + "</u></span></a>";
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
                sb.Append("<a class=\"tp-sph\" href=\"" + href + "\"><img src=\"" + p.Image + "\" alt=\"" + HtmlEncode(p.Caption) + "\" loading=\"lazy\"><span>" + HtmlEncode(p.Caption) + "</span></a>");
            }
            return sb.ToString();
        }
    }
}
