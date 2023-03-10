
namespace DODownloader
{
    internal class DOFile
    {
        public string Url { get; private set; }
        public string Id { get; private set; }
        public ulong SizeBytes { get; private set; }

        public string PiecesHashFileUrl { get; private set; }
        public string HashOfHashes { get; private set; }

        public DOFile(string url, string id = null, ulong sizeBytes = 0, string phfUrl = null, string hoh = null)
        {
            Url = url;
            Id = id;
            SizeBytes = sizeBytes;
            PiecesHashFileUrl = phfUrl;
            HashOfHashes = hoh;
        }
    }
}
