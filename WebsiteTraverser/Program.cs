using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace WebsiteTraverser
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "https://tretton37.com/";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
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

            Console.ReadLine();
        }
    }
}
