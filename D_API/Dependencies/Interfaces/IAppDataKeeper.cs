﻿using D_API.DataContexts;
using D_API.Types.DataKeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace D_API.Dependencies.Interfaces
{
    public interface IAppDataKeeper
    {
        public record DataKeeperQuotaSettings(double? Upload, double? Download, double? Storage);

        public Task<DataOperationResults<double, bool>> Upload(Guid userkey, string datakey, byte[] data, bool overwrite);
        public Task<DataOperationResults<byte[]?, double>> Download(Guid userkey, string datakey);

        public Task<DataOperationResults<Guid>> GetReadonlyKey(Guid userkey, string datakey);
        public Task<DataOperationResults> AddReaders(Guid userkey, string datakey, params Guid[] readerkeys);
        public Task<DataOperationResults> RemoveReaders(Guid userkey, string datakey, params Guid[] readerkeys);

        public Task<DataOperationResults<byte[]?, double>> DownloadReadonly(Guid userkey, Guid dataId);

        public bool EnsureRoot();
        public Task<UserOperationResults> SetUserQuotas(Guid userkey, DataKeeperQuotaSettings settings);

        public Task<bool> CheckExists(Guid userkey, string datakey);
        public Task<FullTransferReport> GetFullTransferReport(Guid userkey);
    }
}
