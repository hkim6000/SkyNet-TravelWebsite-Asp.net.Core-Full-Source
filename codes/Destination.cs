using System.Globalization;
using System.Text;
using System.Text.Json;
using Travel.Models;
using SkyNet;

namespace Travel.codes
{
    public class Destination : WebPage
    {
        public override async Task OnInitialized()
        {
            HtmlDoc.SetTitle("Destination | Northbound Notes");
            HtmlDoc.AddMetaElement("viewport", "width=device-width, initial-scale=1");
            HtmlDoc.AddMetaElement("description", "Country travel guide.");

            SiteData site = await LoadSite();
            string key = Pick(QueryValue("c"), site.Countries.Select(c => c.Key));
            Place? c = site.Countries.FirstOrDefault(x => x.Key == key) ?? site.Countries.FirstOrDefault();
            if (c == null)
            {
                return;
            }
            HtmlDoc.SetTitle(c.Name + " travel guide | Northbound Notes");
            List<Story> posts = site.Posts.Where(p => p.Country == c.Key).OrderByDescending(p => p.Date).ToList();
            StringBuilder head = new StringBuilder();
            head.Append("<section class=\"dt-banner dt-banner-c\"><img src=\"images/banners/" + c.Key + ".webp\" alt=\"\">");
            head.Append("<div class=\"dt-banner-txt\"><div class=\"dt-eyebrow\">" + HtmlEncode(LabelOf(site.Continents, c.Continent)) + "</div><h1>" + HtmlEncode(c.Name) + "</h1><p>" + HtmlEncode(c.Tagline) + "</p></div></section>");
            head.Append("<div class=\"dt-wrap\"><div class=\"dt-crumb\"><a href=\"Home\">Home</a><span>/</span><a href=\"Destinations\">Destinations</a><span>/</span>" + HtmlEncode(c.Name) + "</div></div>");

            StringBuilder ov = new StringBuilder();
            ov.Append("<div><div class=\"dt-eyebrow\">Overview</div><h2>Why go to " + HtmlEncode(c.Name) + "</h2><p>" + HtmlEncode(c.Overview) + "</p></div>");
            ov.Append("<div class=\"dt-facts\"><div><span>Best time to go</span><b>" + HtmlEncode(c.Season) + "</b></div>");
            ov.Append("<div><span>Continent</span><b>" + HtmlEncode(LabelOf(site.Continents, c.Continent)) + "</b></div>");
            ov.Append("<div><span>Stories on the blog</span><b>" + posts.Count + "</b></div></div>");

            StringBuilder hl = new StringBuilder();
            for (int i = 0; i < c.Highlights.Count; i++)
            {
                hl.Append("<div class=\"dt-hlc\"><span>" + (i + 1).ToString("00", CultureInfo.InvariantCulture) + "</span><b>" + HtmlEncode(c.Highlights[i]) + "</b></div>");
            }

            List<Place> others = site.Countries.Where(x => x.Key != c.Key).OrderByDescending(x => x.Continent == c.Continent).ThenBy(x => x.Name).Take(4).ToList();
            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText
                .Replace("{plhd_head}", head.ToString())
                .Replace("{plhd_overview}", ov.ToString())
                .Replace("{plhd_highlights}", hl.ToString())
                .Replace("{plhd_gallink}", "<a href=\"Gallery?cont=" + c.Continent + "\">More from " + HtmlEncode(LabelOf(site.Continents, c.Continent)) + " &rarr;</a>")
                .Replace("{plhd_photos}", Strip(site.Photos.Where(p => p.Country == c.Key).ToList(), "Gallery?cont=" + c.Continent))
                .Replace("{plhd_posts}", posts.Count == 0 ? "<div class=\"dt-empty\">Stories coming soon.</div>" : PostGrid(site, posts.Take(3).ToList()))
                .Replace("{plhd_more}", PlaceGrid(site, others));
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
                sb.Append("<div class=\"dt-sg-none\">No results for &ldquo;" + HtmlEncode(q) + "&rdquo;</div>");
            }
            else
            {
                foreach (string r in rows.Take(7))
                {
                    sb.Append(r);
                }
                sb.Append("<div class=\"dt-sg-foot\">" + total + (total == 1 ? " result" : " results") + " across places, stories and tips</div>");
            }
            response.SetElementContents("dt-sugg", sb.ToString());
            response.ExecuteScript("DestinationJs.openSugg();");
            return response;
        }

        public async Task<ApiResponse> Subscribe()
        {
            ApiResponse response = new ApiResponse();
            await Task.CompletedTask;
            string email = (GetDataValue("email") ?? string.Empty).Trim();
            if (!IsEmail(email))
            {
                response.SetElementContents("dt-nl-msg", "<span class=\"dt-err\">Please enter a valid email address.</span>");
                return response;
            }
            response.SetElementContents("dt-nl-msg", "<span class=\"dt-ok\">Thanks! " + HtmlEncode(email) + " is on the list. (Demo only &mdash; nothing was stored.)</span>");
            response.ExecuteScript("DestinationJs.subscribed();");
            return response;
        }

        private static string Sugg(string type, string img, string title, string sub, string href)
        {
            string pic = img == string.Empty ? "<span class=\"dt-sg-i\">&#10003;</span>" : "<img src=\"" + img + "\" alt=\"\">";
            return "<a class=\"dt-sg\" href=\"" + href + "\">" + pic + "<span><b>" + HtmlEncode(title) + "</b><small>" + type + " &middot; " + HtmlEncode(sub) + "</small></span></a>";
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
            sb.Append("<button type=\"button\" class=\"dt-chip" + (active == string.Empty ? " dt-act" : string.Empty) + "\" onclick=\"DestinationJs.chip(this, '" + group + "', '')\">All</button>");
            foreach (KeyLabel k in items)
            {
                sb.Append("<button type=\"button\" class=\"dt-chip" + (k.Key == active ? " dt-act" : string.Empty) + "\" onclick=\"DestinationJs.chip(this, '" + group + "', '" + k.Key + "')\">" + HtmlEncode(k.Label) + "</button>");
            }
            return sb.ToString();
        }

        private static string PostCard(SiteData site, Story p, bool big)
        {
            Place c = PlaceOf(site, p.Country);
            StringBuilder sb = new StringBuilder();
            sb.Append("<article class=\"dt-card" + (big ? " dt-card-big" : string.Empty) + "\"><a class=\"dt-card-img\" href=\"Post?id=" + p.Id + "\"><img src=\"" + p.Image + "\" alt=\"\" loading=\"lazy\">");
            sb.Append("<span class=\"dt-tag\">" + HtmlEncode(LabelOf(site.Categories, p.Category)) + "</span></a>");
            sb.Append("<div class=\"dt-card-b\"><div class=\"dt-cmeta\"><span>" + HtmlEncode(c.Name) + "</span><span>" + DateText(p.Date) + "</span><span>" + p.Minutes + " min</span></div>");
            sb.Append("<h3><a href=\"Post?id=" + p.Id + "\">" + HtmlEncode(p.Title) + "</a></h3><p>" + HtmlEncode(p.Excerpt) + "</p>");
            sb.Append("<a class=\"dt-rmore\" href=\"Post?id=" + p.Id + "\">Read the story &rarr;</a></div></article>");
            return sb.ToString();
        }

        private static string PostGrid(SiteData site, List<Story> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"dt-empty\">No stories match these filters yet.</div>";
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
            return "<a class=\"dt-place\" href=\"Destination?c=" + c.Key + "\"><img src=\"" + c.Image + "\" alt=\"\" loading=\"lazy\"><span class=\"dt-place-t\"><small>" + HtmlEncode(LabelOf(site.Continents, c.Continent)) + "</small><b>" + HtmlEncode(c.Name) + "</b><em>" + HtmlEncode(c.Tagline) + "</em><u>" + n + (n == 1 ? " story" : " stories") + "</u></span></a>";
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
                sb.Append("<a class=\"dt-sph\" href=\"" + href + "\"><img src=\"" + p.Image + "\" alt=\"" + HtmlEncode(p.Caption) + "\" loading=\"lazy\"><span>" + HtmlEncode(p.Caption) + "</span></a>");
            }
            return sb.ToString();
        }
    }
}
