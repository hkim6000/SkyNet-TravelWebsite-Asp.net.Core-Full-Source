using System.Globalization;
using System.Text;
using System.Text.Json;
using Travel.Models;
using SkyNet;

namespace Travel.codes
{
    public class Post : WebPage
    {
        public override async Task OnInitialized()
        {
            HtmlDoc.SetTitle("Story | Northbound Notes");
            HtmlDoc.AddMetaElement("viewport", "width=device-width, initial-scale=1");
            HtmlDoc.AddMetaElement("description", "A story from Northbound Notes.");

            SiteData site = await LoadSite();
            string id = (QueryValue("id") ?? string.Empty).Trim();
            List<Story> ordered = site.Posts.OrderByDescending(p => p.Date).ToList();
            Story? s = ordered.FirstOrDefault(p => p.Id == id) ?? ordered.FirstOrDefault();
            if (s == null)
            {
                HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText.Replace("{plhd_crumb}", "Not found").Replace("{plhd_post}", "<p>Story not found.</p>")
                    .Replace("{plhd_side}", string.Empty).Replace("{plhd_nav}", string.Empty);
                return;
            }
            HtmlDoc.SetTitle(s.Title + " | Northbound Notes");
            Place c = PlaceOf(site, s.Country);
            StringBuilder a = new StringBuilder();
            a.Append("<img class=\"po-cover\" src=\"" + s.Image + "\" alt=\"\">");
            a.Append("<div class=\"po-ameta\"><a class=\"po-tag\" href=\"Blog?cat=" + s.Category + "\">" + HtmlEncode(LabelOf(site.Categories, s.Category)) + "</a>");
            a.Append("<a href=\"Destination?c=" + c.Key + "\">" + HtmlEncode(c.Name) + "</a><span>" + DateText(s.Date) + "</span><span>" + s.Minutes + " min read</span></div>");
            a.Append("<h1>" + HtmlEncode(s.Title) + "</h1><p class=\"po-lead\">" + HtmlEncode(s.Excerpt) + "</p>");
            foreach (string para in s.Body)
            {
                a.Append("<p>" + HtmlEncode(para) + "</p>");
            }
            a.Append("<div class=\"po-tags\">");
            foreach (string t in s.Tags)
            {
                a.Append("<span>#" + HtmlEncode(t.Replace(" ", string.Empty)) + "</span>");
            }
            a.Append("</div><div class=\"po-author\"><span class=\"po-ava\">ME</span><div><b>Mara Ellison</b><p>Slow traveler and writer. Twelve years on the road, still learning.</p><a href=\"About\">About me &rarr;</a></div></div>");

            StringBuilder side = new StringBuilder();
            side.Append("<div class=\"po-sbox\"><div class=\"po-eyebrow\">Destination</div>" + PlaceCard(site, c) + "</div>");
            side.Append("<div class=\"po-sbox\"><div class=\"po-eyebrow\">Related stories</div>");
            foreach (Story r in site.Posts.Where(p => p.Id != s.Id && (p.Country == s.Country || p.Category == s.Category)).OrderByDescending(p => p.Country == s.Country).ThenByDescending(p => p.Date).Take(3))
            {
                side.Append("<a class=\"po-mini\" href=\"Post?id=" + r.Id + "\"><img src=\"" + r.Image + "\" alt=\"\" loading=\"lazy\"><span><b>" + HtmlEncode(r.Title) + "</b><small>" + DateText(r.Date) + "</small></span></a>");
            }
            side.Append("</div>");

            int idx = ordered.IndexOf(s);
            StringBuilder nav = new StringBuilder();
            if (idx + 1 < ordered.Count)
            {
                nav.Append("<a class=\"po-prev\" href=\"Post?id=" + ordered[idx + 1].Id + "\"><small>&larr; Older</small><b>" + HtmlEncode(ordered[idx + 1].Title) + "</b></a>");
            }
            else
            {
                nav.Append("<span></span>");
            }
            if (idx > 0)
            {
                nav.Append("<a class=\"po-next\" href=\"Post?id=" + ordered[idx - 1].Id + "\"><small>Newer &rarr;</small><b>" + HtmlEncode(ordered[idx - 1].Title) + "</b></a>");
            }

            HtmlDoc.HtmlBodyText = HtmlDoc.HtmlBodyText
                .Replace("{plhd_crumb}", HtmlEncode(s.Title))
                .Replace("{plhd_post}", a.ToString())
                .Replace("{plhd_side}", side.ToString())
                .Replace("{plhd_nav}", nav.ToString());
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
                sb.Append("<div class=\"po-sg-none\">No results for &ldquo;" + HtmlEncode(q) + "&rdquo;</div>");
            }
            else
            {
                foreach (string r in rows.Take(7))
                {
                    sb.Append(r);
                }
                sb.Append("<div class=\"po-sg-foot\">" + total + (total == 1 ? " result" : " results") + " across places, stories and tips</div>");
            }
            response.SetElementContents("po-sugg", sb.ToString());
            response.ExecuteScript("PostJs.openSugg();");
            return response;
        }

        public async Task<ApiResponse> Subscribe()
        {
            ApiResponse response = new ApiResponse();
            await Task.CompletedTask;
            string email = (GetDataValue("email") ?? string.Empty).Trim();
            if (!IsEmail(email))
            {
                response.SetElementContents("po-nl-msg", "<span class=\"po-err\">Please enter a valid email address.</span>");
                return response;
            }
            response.SetElementContents("po-nl-msg", "<span class=\"po-ok\">Thanks! " + HtmlEncode(email) + " is on the list. (Demo only &mdash; nothing was stored.)</span>");
            response.ExecuteScript("PostJs.subscribed();");
            return response;
        }

        private static string Sugg(string type, string img, string title, string sub, string href)
        {
            string pic = img == string.Empty ? "<span class=\"po-sg-i\">&#10003;</span>" : "<img src=\"" + img + "\" alt=\"\">";
            return "<a class=\"po-sg\" href=\"" + href + "\">" + pic + "<span><b>" + HtmlEncode(title) + "</b><small>" + type + " &middot; " + HtmlEncode(sub) + "</small></span></a>";
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
            sb.Append("<button type=\"button\" class=\"po-chip" + (active == string.Empty ? " po-act" : string.Empty) + "\" onclick=\"PostJs.chip(this, '" + group + "', '')\">All</button>");
            foreach (KeyLabel k in items)
            {
                sb.Append("<button type=\"button\" class=\"po-chip" + (k.Key == active ? " po-act" : string.Empty) + "\" onclick=\"PostJs.chip(this, '" + group + "', '" + k.Key + "')\">" + HtmlEncode(k.Label) + "</button>");
            }
            return sb.ToString();
        }

        private static string PostCard(SiteData site, Story p, bool big)
        {
            Place c = PlaceOf(site, p.Country);
            StringBuilder sb = new StringBuilder();
            sb.Append("<article class=\"po-card" + (big ? " po-card-big" : string.Empty) + "\"><a class=\"po-card-img\" href=\"Post?id=" + p.Id + "\"><img src=\"" + p.Image + "\" alt=\"\" loading=\"lazy\">");
            sb.Append("<span class=\"po-tag\">" + HtmlEncode(LabelOf(site.Categories, p.Category)) + "</span></a>");
            sb.Append("<div class=\"po-card-b\"><div class=\"po-cmeta\"><span>" + HtmlEncode(c.Name) + "</span><span>" + DateText(p.Date) + "</span><span>" + p.Minutes + " min</span></div>");
            sb.Append("<h3><a href=\"Post?id=" + p.Id + "\">" + HtmlEncode(p.Title) + "</a></h3><p>" + HtmlEncode(p.Excerpt) + "</p>");
            sb.Append("<a class=\"po-rmore\" href=\"Post?id=" + p.Id + "\">Read the story &rarr;</a></div></article>");
            return sb.ToString();
        }

        private static string PostGrid(SiteData site, List<Story> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"po-empty\">No stories match these filters yet.</div>";
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
            return "<a class=\"po-place\" href=\"Destination?c=" + c.Key + "\"><img src=\"" + c.Image + "\" alt=\"\" loading=\"lazy\"><span class=\"po-place-t\"><small>" + HtmlEncode(LabelOf(site.Continents, c.Continent)) + "</small><b>" + HtmlEncode(c.Name) + "</b><em>" + HtmlEncode(c.Tagline) + "</em><u>" + n + (n == 1 ? " story" : " stories") + "</u></span></a>";
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
                sb.Append("<a class=\"po-sph\" href=\"" + href + "\"><img src=\"" + p.Image + "\" alt=\"" + HtmlEncode(p.Caption) + "\" loading=\"lazy\"><span>" + HtmlEncode(p.Caption) + "</span></a>");
            }
            return sb.ToString();
        }
    }
}
