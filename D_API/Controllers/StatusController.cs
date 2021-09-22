﻿using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace D_API.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController : ControllerBase
{
	private readonly ClientRateLimitOptions Cptions;
	private readonly IClientPolicyStore ClientPolicyStore;

	public StatusController(IOptions<ClientRateLimitOptions> optionsAccessor, IClientPolicyStore clientPolicyStore)
	{
		Cptions = optionsAccessor.Value;
		ClientPolicyStore = clientPolicyStore;
	}

    [HttpGet("ratelimit")]
    public async Task<ClientRateLimitPolicy> GetRateLimitStatus() 
		=> await ClientPolicyStore.GetAsync($"{Cptions.ClientPolicyPrefix}_cl-key-1");
}
