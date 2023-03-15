using System;
using System.Runtime.InteropServices;

namespace DODownloader
{
    internal static class PInvoke
    {
        private const uint ClassContext_LocalServer = 4;

        public static object GetComObject(Guid clsid, Guid riid)
        {
            int hResult = CoCreateInstance(ref clsid, null, ClassContext_LocalServer, ref riid, out var instance);
            if (hResult != 0)
            {
                throw new COMException("CoCreateInstance failed with the specified error", hResult);
            }
            return instance;
        }

        [DllImport("api-ms-win-core-com-l1-1-1.dll")]
        public static extern Int32 CoCreateInstance(
            ref Guid clsid,
            [MarshalAs(UnmanagedType.IUnknown)] object inner,
            uint context,
            ref Guid uuid,
            [MarshalAs(UnmanagedType.IUnknown)] out object rReturnedComObject);

        [DllImport("api-ms-win-core-com-l1-1-1.dll")]
        public static extern Int32 CoSetProxyBlanket(
            [In] IntPtr punk,
            [In] uint dwAuthnSvc,
            [In] uint dwAuthzSvc,
            [In] IntPtr pServerPrincName,
            [In] uint dwAuthLevel,
            [In] uint dwImpLevel,
            [In] IntPtr /* RPC_AUTH_IDENTITY_HANDLE */ pAuthInfo,
            [In] uint dwCapabilities);

        private static readonly IntPtr COLE_DefaultPrincipal = (IntPtr)ulong.MaxValue; // -1
        private const uint AuthN_Default = uint.MaxValue;
        private const uint AuthZ_Default = uint.MaxValue;
        private const uint AuthLevel_Default = 0;
        private const uint Auth_StaticCloaking = 32; // 0x20

        public enum ImpersonationLevel
        {
            Default = 0,
            Anonymous = 1,
            Identify = 2,
            Impersonate = 3,
            Delegate = 4
        }

        public static void AllowImpersonation<TInterface>(object proxyObject, ImpersonationLevel impLevel)
        {
            IntPtr pInterface = Marshal.GetComInterfaceForObject<object, TInterface>(proxyObject);
            try
            {
                int hResult = CoSetProxyBlanket(pInterface, AuthN_Default, AuthZ_Default, COLE_DefaultPrincipal, AuthLevel_Default,
                    (uint)impLevel, IntPtr.Zero, Auth_StaticCloaking);
                if (hResult != 0)
                {
                    throw new COMException("CoSetProxyBlanket failed", hResult);
                }
            }
            finally
            {
                Marshal.Release(pInterface);
            }
        }
    }
}
