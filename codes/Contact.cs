using System.Globalization;
using System.Text;
using System.Text.Json;
using Travel.Models;
using SkyNet;

namespace Travel.codes
{
    public class Contact : WebPage
    {
        public override async Task OnInitialized()
        {
            HtmlDoc.SetTitle("Contact | Northbound Notes");
            HtmlDoc.AddMetaElement("viewport", "width=device-width, initial-scale=1");
            HtmlDoc.AddMetaElement("description", "Get in touch with Northbound Notes.");
            await Task.CompletedTask;
        }

        private static readonly string[] Topics = { "collab", "question", "feedback", "other" };

        public async Task<ApiResponse> Send()
        {
            ApiResponse response = new ApiResponse();
            await Task.CompletedTask;
            string name = (GetDataValue("name") ?? string.Empty).Trim();
            string email = (GetDataValue("email") ?? string.Empty).Trim();
            string topic = (GetDataValue("topic") ?? string.Empty).Trim();
            string message = (GetDataValue("message") ?? string.Empty).Trim();

            string eName = name.Length < 2 || name.Length > 60 ? "Please tell me your name." : string.Empty;
            string eEmail = !IsEmail(email) ? "Please enter a valid email address." : string.Empty;
            string eTopic = !Topics.Contains(topic) ? "Please choose a topic." : string.Empty;
            string eMsg = message.Length < 10 ? "Please write at least 10 characters." : message.Length > 1500 ? "Please keep it under 1,500 characters." : string.Empty;

            response.SetElementContents("co-e-name", eName);
            response.SetElementContents("co-e-email", eEmail);
            response.SetElementContents("co-e-topic", eTopic);
            response.SetElementContents("co-e-msg", eMsg);

            if (eName + eEmail + eTopic + eMsg != string.Empty)
            {
                response.SetElementContents("co-sent", string.Empty);
                return response;
            }

            response.SetElementContents("co-sent", "<b>Thanks, " + HtmlEncode(name) + "!</b> This is a demo form, so your message was checked on the server but not sent anywhere.");
            response.ExecuteScript("ContactJs.sent();");
            return response;
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
                sb.Append("<div class=\"co-sg-none\">No results for &ldquo;" + HtmlEncode(q) + "&rdquo;</div>");
            }
            else
            {
                foreach (string r in rows.Take(7))
                {
                    sb.Append(r);
                }
                sb.Append("<div class=\"co-sg-foot\">" + total + (total == 1 ? " result" : " results") + " across places, stories and tips</div>");
            }
            response.SetElementContents("co-sugg", sb.ToString());
            response.ExecuteScript("ContactJs.openSugg();");
            return response;
        }

        public async Task<ApiResponse> Subscribe()
        {
            ApiResponse response = new ApiResponse();
            await Task.CompletedTask;
            string email = (GetDataValue("email") ?? string.Empty).Trim();
            if (!IsEmail(email))
            {
                response.SetElementContents("co-nl-msg", "<span class=\"co-err\">Please enter a valid email address.</span>");
                return response;
            }
            response.SetElementContents("co-nl-msg", "<span class=\"co-ok\">Thanks! " + HtmlEncode(email) + " is on the list. (Demo only &mdash; nothing was stored.)</span>");
            response.ExecuteScript("ContactJs.subscribed();");
            return response;
        }

        private static string Sugg(string type, string img, string title, string sub, string href)
        {
            string pic = img == string.Empty ? "<span class=\"co-sg-i\">&#10003;</span>" : "<img src=\"" + img + "\" alt=\"\">";
            return "<a class=\"co-sg\" href=\"" + href + "\">" + pic + "<span><b>" + HtmlEncode(title) + "</b><small>" + type + " &middot; " + HtmlEncode(sub) + "</small></span></a>";
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
            sb.Append("<button type=\"button\" class=\"co-chip" + (active == string.Empty ? " co-act" : string.Empty) + "\" onclick=\"ContactJs.chip(this, '" + group + "', '')\">All</button>");
            foreach (KeyLabel k in items)
            {
                sb.Append("<button type=\"button\" class=\"co-chip" + (k.Key == active ? " co-act" : string.Empty) + "\" onclick=\"ContactJs.chip(this, '" + group + "', '" + k.Key + "')\">" + HtmlEncode(k.Label) + "</button>");
            }
            return sb.ToString();
        }

        private static string PostCard(SiteData site, Story p, bool big)
        {
            Place c = PlaceOf(site, p.Country);
            StringBuilder sb = new StringBuilder();
            sb.Append("<article class=\"co-card" + (big ? " co-card-big" : string.Empty) + "\"><a class=\"co-card-img\" href=\"Post?id=" + p.Id + "\"><img src=\"" + p.Image + "\" alt=\"\" loading=\"lazy\">");
            sb.Append("<span class=\"co-tag\">" + HtmlEncode(LabelOf(site.Categories, p.Category)) + "</span></a>");
            sb.Append("<div class=\"co-card-b\"><div class=\"co-cmeta\"><span>" + HtmlEncode(c.Name) + "</span><span>" + DateText(p.Date) + "</span><span>" + p.Minutes + " min</span></div>");
            sb.Append("<h3><a href=\"Post?id=" + p.Id + "\">" + HtmlEncode(p.Title) + "</a></h3><p>" + HtmlEncode(p.Excerpt) + "</p>");
            sb.Append("<a class=\"co-rmore\" href=\"Post?id=" + p.Id + "\">Read the story &rarr;</a></div></article>");
            return sb.ToString();
        }

        private static string PostGrid(SiteData site, List<Story> list)
        {
            if (list.Count == 0)
            {
                return "<div class=\"co-empty\">No stories match these filters yet.</div>";
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
            return "<a class=\"co-place\" href=\"Destination?c=" + c.Key + "\"><img src=\"" + c.Image + "\" alt=\"\" loading=\"lazy\"><span class=\"co-place-t\"><small>" + HtmlEncode(LabelOf(site.Continents, c.Continent)) + "</small><b>" + HtmlEncode(c.Name) + "</b><em>" + HtmlEncode(c.Tagline) + "</em><u>" + n + (n == 1 ? " story" : " stories") + "</u></span></a>";
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
                sb.Append("<a class=\"co-sph\" href=\"" + href + "\"><img src=\"" + p.Image + "\" alt=\"" + HtmlEncode(p.Caption) + "\" loading=\"lazy\"><span>" + HtmlEncode(p.Caption) + "</span></a>");
            }
            return sb.ToString();
        }
    }
}
