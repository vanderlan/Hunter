using System.Net;
using System.Net.Mail;
using System.Text;
using HtmlAgilityPack;

internal class Program
{
    private const string CareersUrl = "https://taller.applytojob.com/apply/";
    private const string CsvFile = "C:\\repos\\vanderlan\\Hunter\\positions.csv";

    private static readonly string BaseFolder =
        Path.GetDirectoryName(CsvFile)!;

    private static readonly string PositionsFolder =
        Path.Combine(BaseFolder, "current-positions");

    private static void Main()
    {
        Console.WriteLine("=== Job monitor started ===");

        Directory.CreateDirectory(PositionsFolder);

        Console.WriteLine("Fetching current positions...");
        var currentPositions = FetchCurrentPositions();
        Console.WriteLine($"Found {currentPositions.Count} positions");

        Console.WriteLine("Loading stored positions...");
        var storedPositions = LoadStoredPositions();
        Console.WriteLine($"Loaded {storedPositions.Count} stored positions");

        var added = currentPositions
            .Where(c => !storedPositions.Any(s => s.Url == c.Url))
            .ToList();

        var removed = storedPositions
            .Where(s => !currentPositions.Any(c => c.Url == s.Url))
            .ToList();

        Console.WriteLine($"Added: {added.Count}, Removed: {removed.Count}");

        if (!added.Any() && !removed.Any())
        {
            Console.WriteLine("No changes detected. Exiting.");
            return;
        }

        SavePositions(currentPositions);

        CreateOrUpdatePositionFiles(currentPositions);
        RemoveDeletedPositionFiles(removed);

        SendEmail(added, removed);

        Console.WriteLine("=== Job monitor finished ===");
    }

    // =====================
    // DATA MODEL
    // =====================
    private record Position(string Title, string Url);

    // =====================
    // SCRAPING (LIST)
    // =====================
    private static List<Position> FetchCurrentPositions()
    {
        var web = new HtmlWeb();
        var doc = web.Load(CareersUrl);

        var nodes = doc.DocumentNode
            .SelectNodes("//a[contains(@href,'/apply/')]");

        if (nodes == null)
        {
            Console.WriteLine("No job links found.");
            return [];
        }

        var positions = nodes
            .Select(n => new
            {
                Title = WebUtility.HtmlDecode(n.InnerText.Trim()),
                Url = n.GetAttributeValue("href", "").Trim()
            })
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Title) &&
                !x.Title.Equals("View All Jobs", StringComparison.OrdinalIgnoreCase) &&
                IsRealJobUrl(x.Url)
            )
            .Select(x => new Position(x.Title, x.Url))
            .DistinctBy(p => p.Url)
            .ToList();

        Console.WriteLine($"Filtered to {positions.Count} real positions.");
        return positions;
    }

    private static bool IsRealJobUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // /apply/{token}/{job-slug}
        var segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        return segments.Length >= 3 && segments[0] == "apply";
    }

    // =====================
    // CSV STORAGE
    // =====================
    private static List<Position> LoadStoredPositions()
    {
        if (!File.Exists(CsvFile))
        {
            Console.WriteLine("CSV does not exist. First run.");
            return [];
        }

        return File.ReadAllLines(CsvFile)
            .Skip(1)
            .Select(line =>
            {
                var parts = line.Split(',', 2);
                return new Position(parts[0], parts[1]);
            })
            .ToList();
    }

    private static void SavePositions(IEnumerable<Position> positions)
    {
        Console.WriteLine("Saving CSV...");

        var sb = new StringBuilder();
        sb.AppendLine("Title,Url");

        foreach (var p in positions)
            sb.AppendLine($"{p.Title},{p.Url}");

        File.WriteAllText(CsvFile, sb.ToString());
    }

    // =====================
    // POSITION FILES
    // =====================
    private static void CreateOrUpdatePositionFiles(IEnumerable<Position> positions)
    {
        foreach (var position in positions)
        {
            var fileName = SanitizeFileName(position.Title) + ".txt";
            var filePath = Path.Combine(PositionsFolder, fileName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"File exists: {fileName}");
                continue;
            }

            Console.WriteLine($"Creating file: {fileName}");

            var details = FetchJobDetails(position.Url);

            var sb = new StringBuilder();
            foreach (var section in details)
            {
                sb.AppendLine($"{section.Key}:");
                sb.AppendLine(section.Value);
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }

    private static void RemoveDeletedPositionFiles(IEnumerable<Position> removed)
    {
        foreach (var position in removed)
        {
            var fileName = SanitizeFileName(position.Title) + ".txt";
            var filePath = Path.Combine(PositionsFolder, fileName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Deleting file: {fileName}");
                File.Delete(filePath);
            }
        }
    }

    // =====================
    // JOB DETAILS SCRAPING
    // =====================
    private static Dictionary<string, string> FetchJobDetails(string url)
    {
        Console.WriteLine($"Fetching job details: {url}");

        var web = new HtmlWeb();
        var doc = web.Load(url);

        var root = doc.DocumentNode;

        return new Dictionary<string, string>
        {
            ["Summary"] = ExtractSection(root, "Summary"),
            ["Responsibilities"] = ExtractSection(root, "Responsibilities"),
            ["Requirements"] = ExtractSection(root, "Requirements"),
            ["Must-Have Skills"] = ExtractSection(root, "Must-Have Skills"),
            ["Nice-to-Have Skills"] = ExtractSection(root, "Nice-to-Have Skills")
        };
    }

    private static string ExtractSection(HtmlNode root, string title)
    {
        var header = root.Descendants()
            .FirstOrDefault(n =>
                n.InnerText.Trim()
                    .Equals(title, StringComparison.OrdinalIgnoreCase));

        if (header == null)
        {
            Console.WriteLine($"Section not found: {title}");
            return string.Empty;
        }

        var sb = new StringBuilder();
        var node = header.NextSibling;

        while (node != null)
        {
            var text = node.InnerText.Trim();

            if (!string.IsNullOrWhiteSpace(text))
            {
                if (text.Length < 40 && char.IsUpper(text[0]))
                    break;

                sb.AppendLine(WebUtility.HtmlDecode(text));
            }

            node = node.NextSibling;
        }

        return sb.ToString().Trim();
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        return name.Trim();
    }

    // =====================
    // EMAIL
    // =====================
    private static void SendEmail(
        List<Position> added,
        List<Position> removed)
    {
        Console.WriteLine("Sending email notification...");

        var body = new StringBuilder();

        if (added.Any())
        {
            body.AppendLine("New positions opened:\n");
            foreach (var p in added)
                body.AppendLine($"- {p.Title}\n  {p.Url}\n");
        }

        if (removed.Any())
        {
            body.AppendLine("\nPositions removed:\n");
            foreach (var p in removed)
                body.AppendLine($"- {p.Title}");
        }

        var mail = new MailMessage
        {
            From = new MailAddress("vanderlan.gs@gmail.com"),
            Subject = "Taller Careers – Job Changes Detected",
            Body = body.ToString()
        };

        mail.To.Add("vanderlan.gs@gmail.com");

        var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(
                "vanderlan.gs@gmail.com",
                "cqkn cauj uvsg hzjm"
            ),
            EnableSsl = true
        };

        smtp.Send(mail);

        Console.WriteLine("Email sent.");
    }
}
