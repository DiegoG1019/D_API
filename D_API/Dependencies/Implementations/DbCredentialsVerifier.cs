﻿using D_API.DataContexts;
using D_API.Dependencies.Abstract;
using D_API.Models.Auth;
using D_API.Types.Auth;

namespace D_API.Dependencies.Implementations;

public class DbCredentialsVerifier : AbstractAuthCredentialsVerifier
{
    private readonly ClientDataContext Db;

    public DbCredentialsVerifier(string hashKey, ClientDataContext db) : base(hashKey) => Db = db;

    public override async Task<Client?> FindClient(Guid key) 
        => await Db.Clients.FindAsync(key);

    public override async Task<CredentialVerificationResults> Verify(ClientValidCredentials credentials)
    {
        Client client;
        bool fail = (client = await Db.Clients.FindAsync(credentials.Key)) is null;
        return await VerifyClientCredentials(credentials, client, fail);
    }
}
