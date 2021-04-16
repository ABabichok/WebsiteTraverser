using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebsiteTraverser
{
    public class Traverser
    {
        private Uri _websiteUri;
        private string _websitePath;
        private HashSet<string> _downloadedFiles;
        private List<Task> _downloadFileTasks;

        public Traverser(string url)
        {
            _websiteUri = new Uri(url);
            _downloadedFiles = new HashSet<string>();
            _downloadFileTasks = new List<Task>();
        }

        public async Task Run()
        {
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                _websitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), _websiteUri.Host);
                CreateFolder(_websitePath);

                Traverse(_websiteUri);

                await Task.WhenAll(_downloadFileTasks);

                stopWatch.Stop();
                string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                    stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds, stopWatch.Elapsed.Milliseconds);
                Console.WriteLine(string.Concat("\n\nDOWNLOADING FINISHED. ELAPSED TIME --> ", elapsedTime));
                Console.WriteLine("\n\tDOWNLOADED FILES:\n");
                foreach (var file in _downloadedFiles)
                {
                    Console.WriteLine($"\t\t{file}");
                }
            } catch(Exception ex)
            {
                Console.WriteLine($"EXCEPTION --> {ex.Message}");
            }
        }

        private void Traverse(Uri uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK) return;
                if (_downloadedFiles.Contains(response.ResponseUri.AbsolutePath)) return;

                var isHtmlFile = response.ContentType.Contains("text/html");

                using (var webClient = new WebClient())
                {
                    var fileName = response.ResponseUri.Segments[response.ResponseUri.Segments.Length - 1];

                    if (fileName == "/" || fileName == string.Empty)
                    {
                        if (response.ResponseUri.Fragment != string.Empty)
                        {
                            fileName = response.ResponseUri.Fragment;
                        }

                        if (fileName == "/")
                        {
                            fileName = response.ResponseUri.Fragment;
                        }

                        if (fileName == string.Empty)
                        {
                            fileName = "Index";
                        }
                    }

                    var fileExtension = Path.GetExtension(fileName);

                    var localPath = Path.GetDirectoryName(response.ResponseUri.LocalPath);
                    if (fileExtension == string.Empty)
                    {
                        localPath = response.ResponseUri.LocalPath;
                        if (isHtmlFile) fileName = string.Concat(fileName, ".html");
                    }

                    var fullLocalPath = string.Concat(_websitePath, localPath);
                    CreateFolder(fullLocalPath);

                    _downloadFileTasks.Add(DownloadFileAsync(fullLocalPath, fileName, response.ResponseUri));
                    _downloadedFiles.Add(response.ResponseUri.AbsolutePath);
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
                                var url = match.Groups[1].Value;
                                var uriToTravenre = GetWebsiteUri(url);
                                if (uriToTravenre is not null && url != "/")
                                {
                                    Traverse(uriToTravenre);
                                }
                                else
                                {
                                    Console.WriteLine($"URL is not correct or is from another website --> {match.Groups[1].Value}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private Uri GetWebsiteUri(string url)
        {
            Uri uri;
            Uri.TryCreate(url, UriKind.Absolute, out uri);

            if (uri is null) 
            {
                url = url.Trim('/');
                return new Uri(Path.Combine(_websiteUri.AbsoluteUri, url));
            }

            return (uri.Host == _websiteUri.Host && hasValidUriScheme(uri.Scheme)) ? uri : null;
        }

        private bool hasValidUriScheme(string scheme) => scheme == "http" || scheme == "https";

        public void CreateFolder(string folderPath)
        {
            if (Directory.Exists(folderPath)) return;

            Directory.CreateDirectory(folderPath);
        }

        private void DownloadProgressChanged(object localPath, object fileName, DownloadProgressChangedEventArgs e) => Console.WriteLine($"\tDownloading {fileName} to the {localPath} --> {e.ProgressPercentage} %");

        private void DownloadFileCompleted(object localPath, object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error is not null)
            {
                Console.WriteLine($"\nAn error ocurred while trying to download {sender}\n");
                return;
            }

            Console.WriteLine($"\n\t\t{sender} succesfully downloaded to the {localPath}\n");
        }

        private Task DownloadFileAsync(string fullLocalPath, string fileName, Uri uri)
        {
            fullLocalPath = fullLocalPath.Replace("/", "\\");
            using (var webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += (sender, e) => DownloadProgressChanged(fullLocalPath, fileName, e);
                webClient.DownloadFileCompleted += (sender, e) => DownloadFileCompleted(fullLocalPath, fileName, e);
                return webClient.DownloadFileTaskAsync(uri, Path.Combine(fullLocalPath, fileName));
            }
        }

    }
}
