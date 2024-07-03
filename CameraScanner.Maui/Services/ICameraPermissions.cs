
namespace CameraScanner.Maui
{
    public interface ICameraPermissions
    {
        /// <summary>
        /// Checks if the camera permission is granted.
        /// </summary>
        /// <returns><c>true</c> if the permission is granted, otherwise, <c>false</c>.</returns>
        Task<bool> CheckPermissionAsync();

        /// <summary>
        /// Checks if the camera permission is granted
        /// and requests the permission if not already done so.
        /// </summary>
        /// <returns><c>true</c> if the permission is granted, otherwise, <c>false</c>.</returns>
        Task<bool> CheckAndRequesPermissionAsync();

        /// <summary>
        /// Requests the camera permission.
        /// </summary>
        /// <returns><c>true</c> if the permission is granted, otherwise, <c>false</c>.</returns>
        Task<bool> RequestPermissionAsync();
    }
}