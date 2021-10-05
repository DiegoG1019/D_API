using D_API.Lib.Models.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using static D_API.Lib.Models.Responses.TransferQuotaStatusResponse;

namespace D_API.Lib.Models.Data
{
    public class DataUploadResults
    {
        public bool OverwroteData { get; private set; }
        public bool Success { get; private set; }

        public DataUploadResults(bool success, DataUploadSuccessResponse response)
        {
            Success = success;
            OverwroteData = response.Overwritten;
        }
    }

    public class DataDownloadResults
    {
        public byte[]? Data { get; private set; }

        public DataDownloadResults(DataDownloadSuccessResponse response) => Data = response.Data;
    }

    public class TransferReportResults
    {
        public TransferReport Usage { get; private set; }
        public TransferReport Quota { get; private set; }
        public double StorageUsage { get; private set; }
        public double StorageQuota { get; private set; }

        public override string ToString() => $"Usage:({Usage})|Quota:({Quota}|StorageUsage:{StorageUsage}|StorageQuota:{StorageQuota})";

        public TransferReportResults(TransferQuotaStatusResponse response)
        {
            Usage = response.TransferUsage;
            Quota = response.TransferQuota;
            StorageUsage = response.StorageUsage;
            StorageQuota = response.StorageQuota;
        }
    }
}
