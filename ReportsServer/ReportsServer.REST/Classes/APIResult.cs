namespace ReportsServer.REST.Classes
{
    /// <summary>
    /// Action result.
    /// </summary>
    /// <typeparam name="TResult">Type of success result.</typeparam>
    public class APIResult<TResult> 
    {
        /// <summary>
        /// True - if action was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Success result data. Null - if action was failed.
        /// </summary>
        public TResult Result { get; set; }

        /// <summary>
        /// Error message of failed action.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}