using System;
using System.Runtime.InteropServices;

namespace DODownloader
{
    internal static class PInvoke
    {
        private const uint CLSCTX_LOCAL_SERVER = 4;

        private static readonly IntPtr COLE_DEFAULT_PRINCIPAL = (IntPtr)ulong.MaxValue; // -1

        public static object GetComObject(Guid clsid, Guid riid)
        {
            int hResult = CoCreateInstance(ref clsid, null, CLSCTX_LOCAL_SERVER, ref riid, out var instance);
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

        public enum RPC_C_AUTHN
        {
            DEFAULT = -1,
            GSS_KERBEROS = 16,
        }

        public enum RPC_C_AUTHZ
        {
            DEFAULT = -1,
            NONE = 0
        }

        public enum RPC_C_AUTHN_LEVEL
        {
            DEFAULT = 0,
            NONE = 1,
            CONNECT = 2,
            CALL = 3,
            PKT = 4,
            PKT_INTEGRITY = 5,
            PKT_PRIVACY = 6
        }

        public enum RPC_C_IMP
        {
            ANONYMOUS = 1,
            IDENTIFY = 2,
            IMPERSONATE = 3,
            DELEGATE = 4
        }

        public enum EO_AUTHN_CAP
        {
            None = 0,
            MutualAuth = 1,
            StaticCloaking = 32,
            DynamicCloaking = 64
        }

        [DllImport("api-ms-win-core-com-l1-1-1.dll")]
        public static extern Int32 CoSetProxyBlanket(
            [In] IntPtr punk,
            [In] RPC_C_AUTHN dwAuthnSvc,
            [In] RPC_C_AUTHZ dwAuthzSvc,
            [In] IntPtr pServerPrincName,
            [In] RPC_C_AUTHN_LEVEL dwAuthLevel,
            [In] RPC_C_IMP dwImpLevel,
            [In] IntPtr /* RPC_AUTH_IDENTITY_HANDLE */ pAuthInfo,
            [In] EO_AUTHN_CAP dwCapabilities);

        public static void AllowImpersonation<TInterface>(object proxyObject, RPC_C_IMP impLevel = RPC_C_IMP.IMPERSONATE)
        {
            IntPtr pInterface = Marshal.GetComInterfaceForObject<object, TInterface>(proxyObject);

            try
            {
                int hResult = CoSetProxyBlanket(pInterface,
                    RPC_C_AUTHN.DEFAULT, RPC_C_AUTHZ.DEFAULT, COLE_DEFAULT_PRINCIPAL, RPC_C_AUTHN_LEVEL.DEFAULT,
                    impLevel, IntPtr.Zero, EO_AUTHN_CAP.StaticCloaking);
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
