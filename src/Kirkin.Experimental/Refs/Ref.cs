namespace Kirkin.Refs
{
    /// <summary>
    /// Reference to a value.
    /// </summary>
    internal abstract class Ref<T> : IRef
    {
        /// <summary>
        /// Gets or sets the value that this reference is pointing to.
        /// </summary>
        public abstract T Value { get; set; }

        object IRef.Value
        {
            get
            {
                return Value;
            }
            set
            {
                Value = (T)value;
            }
        }
    }
}