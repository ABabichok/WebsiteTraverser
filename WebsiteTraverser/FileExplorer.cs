using System.IO;

namespace WebsiteTraverser
{
    public class FileExplorer
    {
        public void CreateFolder(string folderPath)
        {
            if (Directory.Exists(folderPath)) return;

            Directory.CreateDirectory(folderPath);
        }
    }
}
