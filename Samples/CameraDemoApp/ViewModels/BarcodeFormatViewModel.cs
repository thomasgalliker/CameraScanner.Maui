using System.Diagnostics;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CameraDemoApp.ViewModels
{
    [DebuggerDisplay("{Value}")]
    public class BarcodeFormatViewModel : ObservableObject, IEquatable<BarcodeFormatViewModel>
    {
        private bool isSelected;

        public BarcodeFormatViewModel(BarcodeFormats barcodeFormats, bool isSelected)
        {
            this.Value = barcodeFormats;
            this.IsSelected = isSelected;
        }

        public BarcodeFormats Value { get; }

        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value);
        }

        public bool Equals(BarcodeFormatViewModel other)
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

            return this.Equals((BarcodeFormatViewModel)obj);
        }

        public override int GetHashCode()
        {
            return this.Value != null ? this.Value.GetHashCode() : 0;
        }
    }
}