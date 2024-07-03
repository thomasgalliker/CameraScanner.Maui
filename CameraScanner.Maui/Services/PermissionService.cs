using System;

namespace CameraScanner.Maui
{
    public class PermissionService : IPermissionService
    {
        private static Lazy<IPermissionService> Implementation = new Lazy<IPermissionService>(CreatePermissionService, LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static IPermissionService Current
        {
            get => Implementation.Value;
        }

        private static IPermissionService CreatePermissionService()
        {
#if (ANDROID || IOS)
            return new PermissionService();
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }

        private PermissionService()
        {
        }

        public async Task<bool> CheckAndRequestCameraPermissionAsync()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permissionStatus != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.Camera>();
            }

            permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permissionStatus == PermissionStatus.Granted)
            {
                return true;
            }

            return false;
        }
    }
}