using System.Runtime.InteropServices;

namespace DODownloader
{
    internal class DODownloadFactory
    {
        private string callerName;
        public DODownloadFactory(string callerName)
        {
            this.callerName = callerName;
        }

        public DODownload CreateDownloadWithFileOutput(DOFile file, string outputFilePath)
        {
            var downloadObj = CreateDownload();
            SetCommonProperties(downloadObj, file);
            downloadObj.SetProperty(DODownloadProperty.LocalPath, outputFilePath);
            return new DODownload(file, downloadObj);
        }

        private IDODownload CreateDownload()
        {
            IDODownload download = null;
            IDOManager manager = (IDOManager)PInvoke.GetComObject(Constants.CLSID_DOManager, Constants.IID_DOManager);
            try
            {
                manager.CreateDownload(out download);
                PInvoke.AllowImpersonation<IDODownload>(download, PInvoke.RPC_C_IMP.IMPERSONATE);
            }
            finally
            {
                Marshal.FinalReleaseComObject(manager);
            }
            return download;
        }

        private void SetCommonProperties(IDODownload downloadObj, DOFile file)
        {
            if (!string.IsNullOrEmpty(callerName))
            {
                downloadObj.SetProperty(DODownloadProperty.DisplayName, callerName);
            }
            if (!string.IsNullOrEmpty(file.Id))
            {
                downloadObj.SetProperty(DODownloadProperty.ContentId, file.Id);
            }
            if (!string.IsNullOrEmpty(file.Url))
            {
                downloadObj.SetProperty(DODownloadProperty.Uri, file.Url);
            }
            if (file.SizeBytes != 0)
            {
                downloadObj.SetProperty(DODownloadProperty.TotalSizeBytes, file.SizeBytes);
            }
        }
    }
}
