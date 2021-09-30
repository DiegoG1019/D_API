namespace D_API.Types.DataKeeper
{
    public enum DataOpResult
    {
        /// <summary>
        /// The user tried to perform an operation, but had passed their transfer quota for the upload or download operation
        /// </summary>
        OverTransferQuota,

        /// <summary>
        /// The user tried to perform an operation, but had passed their storage quota
        /// </summary>
        OverStorageQuota,

        /// <summary>
        /// The user tried to overwrite data they did not specifically request to overwrite
        /// </summary>
        NoOverwrite, 

        /// <summary>
        /// The user tried to access data that does not exist
        /// </summary>
        DataDoesNotExist,

        /// <summary>
        /// The user tried to access data they do not have access to
        /// </summary>
        DataInaccessible,

        /// <summary>
        /// The user tried to perform an operation without the proper arguments
        /// </summary>
        BadArguments,

        /// <summary>
        /// The operation was succesful
        /// </summary>
        Success
    }

    public record DataOperationResults(DataOpResult Result) { }
    public record DataOperationResults<T>(DataOpResult Result, T Value) { }
    public record DataOperationResults<T1,T2>(DataOpResult Result, T1 FirstValue, T2 SecondValue) { }

    public enum UserOperationResult
    {
        Success,
        Failure,
    }

    public record UserOperationResults(UserOperationResult Result) { }
}
