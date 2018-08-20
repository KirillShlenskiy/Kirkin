namespace Kirkin.CommandLine
{
    internal interface IParameterFormattable
    {
        /// <summary>
        /// Returns the short string representation of this parameter.
        /// </summary>
        string ToShortString();

        /// <summary>
        /// Returns the long string representation of this parameter.
        /// </summary>
        string ToLongString();
    }
}