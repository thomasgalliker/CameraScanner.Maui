
namespace CameraScanner.Maui
{
    public interface IPermissionService
    {
        Task<bool> CheckAndRequestCameraPermissionAsync();
    }
}