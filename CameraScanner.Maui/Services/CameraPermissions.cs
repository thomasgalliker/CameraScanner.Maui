﻿namespace CameraScanner.Maui
{
    public class CameraPermissions : ICameraPermissions
    {
        private static readonly Lazy<ICameraPermissions> Implementation = new Lazy<ICameraPermissions>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static ICameraPermissions Current
        {
            get => Implementation.Value;
        }

        private static ICameraPermissions CreateInstance()
        {
#if (ANDROID || IOS)
            return new CameraPermissions();
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }

        private CameraPermissions()
        {
        }

        public async Task<bool> CheckPermissionAsync()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            return permissionStatus == PermissionStatus.Granted;
        }

        public async Task<bool> CheckAndRequesPermissionAsync()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();

                if (permissionStatus == PermissionStatus.Granted)
                {
                    return true;
                }
            }

            return false;
        }


        public async Task<bool> RequestPermissionAsync()
        {
            var permissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
            return permissionStatus == PermissionStatus.Granted;
        }
    }
}