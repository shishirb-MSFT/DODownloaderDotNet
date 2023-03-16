using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DODownloader
{
    internal class DODownloadFactory
    {
        private readonly string callerName;
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

        public List<IDODownload> EnumerateDownloads()
        {
            return GetDownloads(enumCategory: null);
        }

        // Enumerate downloads with filtering on a property value.
        // DO client supports filtering on Id, Uri, ContentId, DisplayName and LocalPath properties.
        public List<IDODownload> EnumerateDownloads(DODownloadProperty filterType, string filterKey)
        {
            var enumCategory = new DO_DOWNLOAD_ENUM_CATEGORY { Property = filterType, Value = filterKey };
            return GetDownloads(enumCategory);
        }

        private IDODownload CreateDownload()
        {
            IDODownload download = null;
            IDOManager manager = GetDOManager();
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

        private List<IDODownload> GetDownloads(DO_DOWNLOAD_ENUM_CATEGORY? enumCategory)
        {
            var downloads = new List<IDODownload>();
            IDOManager manager = GetDOManager();
            var category = IntPtr.Zero;
            try
            {
                if (enumCategory.HasValue)
                {
                    category = Marshal.AllocCoTaskMem(Marshal.SizeOf<DO_DOWNLOAD_ENUM_CATEGORY>());
                    Marshal.StructureToPtr(enumCategory.Value, category, false);
                }
                manager.EnumDownloads(category, out IEnumUnknown list);
                uint cFetched = 0;
                do
                {
                    var objs = new object[1];
                    list.Next(1, objs, ref cFetched);
                    if (cFetched == 1)
                    {
                        downloads.Add((IDODownload)objs[0]);
                    }
                } while (cFetched > 0);
            }
            catch (COMException ce)
            {
                const int noSuchDownloads = unchecked((int)0x80D02005);
                if (ce.HResult != noSuchDownloads)
                {
                    throw;
                }
            }
            finally
            {
                if (category != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(category);
                }
                if (manager != null)
                {
                    Marshal.FinalReleaseComObject(manager);
                }
            }
            return downloads;
        }

        private IDOManager GetDOManager()
        {
            return (IDOManager)PInvoke.GetComObject(Constants.CLSID_DeliveryOptimization, Constants.IID_DOManager);
        }
    }
}
