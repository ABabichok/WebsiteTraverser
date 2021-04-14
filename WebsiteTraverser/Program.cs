using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace WebsiteTraverser
{
    class Program
    {
        private static HashSet<string> _downloadedPages = new HashSet<string>();

        static void Main(string[] args)
        {
            string url = "https://tretton37.com";

            Uri uri = new Uri(url);
            var websitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), uri.Host);

            CreateFolder(websitePath);

            
            var request = (HttpWebRequest)WebRequest.Create(url);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK) return;
                if (_downloadedPages.Contains(response.ResponseUri.AbsoluteUri)) return;

                var isHtmlFile = response.ContentType.Contains("text/html");

                using (var webClient = new WebClient())
                {
                    var fileName = Path.GetFileName(response.ResponseUri.LocalPath);
                    if (fileName == string.Empty) fileName = "Index";
                    var fileExtension = Path.GetExtension(fileName);

                    var localPath = Path.GetDirectoryName(response.ResponseUri.LocalPath);
                    if (fileExtension == string.Empty)
                    {
                        localPath = response.ResponseUri.LocalPath;
                        if (isHtmlFile) fileName = string.Concat(fileName, ".html");
                    }

                    var fullLocalPath = string.Concat(websitePath, localPath);
                    CreateFolder(fullLocalPath);
                    webClient.DownloadFile(response.ResponseUri, Path.Combine(fullLocalPath, fileName));

                    _downloadedPages.Add(response.ResponseUri.AbsoluteUri);
                }

                if (isHtmlFile)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();
                        Regex regex = new Regex(@"href\s*=\s*(?:[""'](?<1>[^""']*)[""'])", RegexOptions.IgnoreCase);
                        MatchCollection matches = regex.Matches(html);
                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                Console.WriteLine(match.Groups[1]);
                            }
                        }
                    }
                }
            }

            Console.ReadLine();
        }

        private static void CreateFolder(string folderPath)
        {
            if (Directory.Exists(folderPath)) return;

            Directory.CreateDirectory(folderPath);
        }

    }
}
