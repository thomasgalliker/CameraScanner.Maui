using System.Diagnostics;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CameraDemoApp.ViewModels
{
    [DebuggerDisplay("{Value}")]
    public class CaptureQualityViewModel : ObservableObject, IEquatable<CaptureQualityViewModel>
    {
        private bool isSelected;

        public CaptureQualityViewModel(CaptureQuality captureQuality, bool isSelected)
        {
            this.Value = captureQuality;
            this.IsSelected = isSelected;
        }

        public CaptureQuality Value { get; }

        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value);
        }

        public bool Equals(CaptureQualityViewModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(this.Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((CaptureQualityViewModel)obj);
        }

        public override int GetHashCode()
        {
            return this.Value != null ? this.Value.GetHashCode() : 0;
        }
    }
}