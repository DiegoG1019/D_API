namespace D_API.Types.DataKeeper
{
    public class UploadRequest
    {
        public byte[]? Data { get; set; }
        public bool Overwrite { get; set; }
    }
}
