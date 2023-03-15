﻿using System.Runtime.InteropServices;

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

        public DODownload CreateDownloadWithStreamOutput(DOFile file, SequentialStreamReceiver dataStream)
        {
            var downloadObj = CreateDownload();
            SetCommonProperties(downloadObj, file);

            // Without UnknownWrapper dosvc receives VT_DISPATCH and returns E_INVALIDARG
            downloadObj.SetProperty(DODownloadProperty.StreamInterface, new UnknownWrapper(dataStream));

            return new DODownload(file, downloadObj);
        }

        private IDODownload CreateDownload()
        {
            IDODownload download = null;
            IDOManager manager = (IDOManager)PInvoke.GetComObject(Constants.CLSID_DeliveryOptimization, Constants.IID_DOManager);
            try
            {
                manager.CreateDownload(out download);
                PInvoke.AllowImpersonation<IDODownload>(download, PInvoke.ImpersonationLevel.Impersonate);
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
