namespace CameraScanner.Maui
{
    public abstract class ParsedResult
    {
        public string DisplayResult { get; protected set; }

        public virtual string GetDisplayResult()
        {
            // TODO: Make method abstract
            return this.DisplayResult;
        }
    }
}