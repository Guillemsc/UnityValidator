namespace GValidator.Validation.Messages
{
    /// <summary>
    /// Specifies the severity of a validation finding.
    /// </summary>
    public enum ValidationMessageType
    {
        /// <summary>
        /// An informational finding that does not fail validation.
        /// </summary>
        Info,

        /// <summary>
        /// A finding that should be reviewed but does not fail validation.
        /// </summary>
        Warning,

        /// <summary>
        /// A finding that fails validation and CI.
        /// </summary>
        Error,
    }
}
