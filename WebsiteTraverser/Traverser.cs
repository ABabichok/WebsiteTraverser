using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace WebsiteTraverser
{
    public class Traverser
    {
        private string _websiteUrl;
        private string _websitePath;
        private FileExplorer _fileExplorer;
        private HashSet<string> _downloadedPages;

        public Traverser(string url)
        {
            _websiteUrl = url;
            _fileExplorer = new FileExplorer();
            _downloadedPages = new HashSet<string>();
        }

        public void Run()
        {
            Uri uri = new Uri(_websiteUrl);
            _websitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), uri.Host);

            _fileExplorer.CreateFolder(_websitePath);
            Traverse(uri);
        }

        private void Traverse(Uri uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
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

                    var fullLocalPath = string.Concat(_websitePath, localPath);
                    _fileExplorer.CreateFolder(fullLocalPath);

                    DownloadFileAsync(fullLocalPath, fileName, response.ResponseUri);

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
                                //Console.WriteLine(match.Groups[1]);
                            }
                        }
                    }
                }
            }
        }

        private void DownloadProgressChanged(object localPath, object fileName, DownloadProgressChangedEventArgs e) => Console.WriteLine($"Downloading {fileName} to the {localPath} --> {e.ProgressPercentage} %");

        private void DownloadFileCompleted(object localPath, object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error is not null)
            {
                Console.WriteLine($"An error ocurred while trying to download {sender}");
                return;
            }

            Console.WriteLine($"\n{sender} succesfully downloaded to the {localPath}\n");
        }

        private void DownloadFileAsync(string fullLocalPath, string fileName, Uri uri)
        {
            fullLocalPath = fullLocalPath.Replace("/", "\\");
            using (var webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += (sender, e) => DownloadProgressChanged(fullLocalPath, fileName, e);
                webClient.DownloadFileCompleted += (sender, e) => DownloadFileCompleted(fullLocalPath, fileName, e);
                webClient.DownloadFileAsync(uri, Path.Combine(fullLocalPath, fileName));
            }

        }

    }
}
