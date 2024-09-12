using System.ComponentModel;

namespace CameraScanner.Maui
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class PreserveAttribute : Attribute
    {
        public bool AllMembers;
        public bool Conditional;

        public PreserveAttribute()
        {
        }

        public PreserveAttribute(Type type)
        {
        }
    }
}