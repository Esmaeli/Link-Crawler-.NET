using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;

class Program
{
    static async System.Threading.Tasks.Task Main()
    {
        Console.Write("Enter the URL to crawl: ");
        string url = Console.ReadLine();

        if (Uri.TryCreate(url, UriKind.Absolute, out var validUrl) && (validUrl.Scheme == Uri.UriSchemeHttp || validUrl.Scheme == Uri.UriSchemeHttps))
        {
            Console.WriteLine("\nCrawling links...\n");

            var crawler = new WebpageCrawler(url);
            var links = await crawler.CrawlLinksAsync();

            Console.WriteLine($"Found {links.Count} links.");

            Console.Write("Enter the file name to save links (or press Enter to use default 'links.txt'): ");
            string fileName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "links.txt";
            }

            SaveLinksToFile(links, fileName);

            Console.WriteLine($"Links saved to '{fileName}'.");
        }
        else
        {
            Console.WriteLine("Invalid URL. Please enter a valid URL starting with 'http://' or 'https://'.");
        }
    }

    static void SaveLinksToFile(List<string> links, string fileName)
    {
        File.WriteAllLines(fileName, links);
    }
}

class WebpageCrawler
{
    private readonly string baseUrl;

    public WebpageCrawler(string baseUrl)
    {
        this.baseUrl = baseUrl;
    }

    public async System.Threading.Tasks.Task<List<string>> CrawlLinksAsync()
    {
        var links = new List<string>();

        using (var httpClient = new HttpClient())
        {
            string htmlContent = await httpClient.GetStringAsync(baseUrl);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            var hrefs = htmlDocument.DocumentNode.Descendants("a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(href => !string.IsNullOrWhiteSpace(href));

            foreach (var href in hrefs)
            {
                string fullUrl = href.StartsWith("http") ? href : new Uri(new Uri(baseUrl), href).ToString();
                links.Add(fullUrl);
            }
        }

        return links;
    }
}
