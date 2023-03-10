using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DODownloader
{
    internal class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine("DODownloader.exe <url> <outputFilePath>");
        }

        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return 1;
            }

            string url = args[0];
            string outputFilePath = args[1];

            var file = new DOFile(url);
            var download = new DODownload(file, "DODownloader App", outputFilePath);
            download.SetForeground();
            var callback = new DODownloadCallback(download.Id.ToString());
            download.SetHandler(callback);
            download.StartAndWaitUntilCompletion(60);

            var fileSize = new FileInfo(outputFilePath).Length;
            Console.WriteLine($"Download completed, file of size {fileSize / 1048576.0}MB written to {outputFilePath}");

            return 0;
        }
    }
}
