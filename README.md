# D_API

D_API is a personal project to practice API building, security, authorization and serving

I'll be using it primarily for some of my own applications where I'd like certain configuration or other information to be readily available from a central, trusted location (and with known disk permanence, as opposed to some projects)

## Table of Contents
- [Installation](#installation)
	- [Configuration](#configuration)
- [Usage](#usage)
	- [Client](#client)
	- [Accessing the API](#accessing-the-api)
	- [Endpoints](#endpoints)
	- [Response Types](#response-types)
	- [Rate Limiting](#rate-limiting)
- [Contributing](#contributing)
- [License](#license)
- [Support My Project](#support-my-projects)

## Installation
The installation process is much like any other ASP.NET API Project: Set up a reverse proxy, open up the firewall and run the thing

Aside from that, the app will need to be run once before actually being used, so that it can create all configuration templates you can then fill out. It's highly recommended to introduce an environment variable called `D_API_dir`, whose value is a valid directory the app has read/write access to, where it can manage its configurations and other data

### Configuration
The program will create a `apisettings.cfg.json` file somewhere in the file system, this location should appear in the console output
- `string APIKey`: The Telegram API Key of the bot you want to use to interface with the D_API, in order to gain access to useful data such as the current status, and to output important log data
- `List<long> AllowedUsers`: A list of user Ids for the bot to respond to. Any user that is not registered here will be IGNORED.
- `long? EventChannelId`: A channel Id to which the bot will output important (Warning, Error and Fatal) log messages. Set to `null` to ignore, defaults to `0` (invalid)
- `Security`: a subsection containing:
	- `string? Audience`: The audience value with which to generate new keys. Generated keys will not work without this value. Ignored and not required if set to null.
	- `string? Issuer`: The issuer value with which to generate new keys. Generated keys will not work without this value. Ignored and not required if set to null.
	- `string? JWTSecurityKey`: The key used when generating JWT Tokens. Defaulted if set to null.
	- `string? Hashkey`: The key used when hashing user secrets and other sensitive user data. Defaulted if set to null.
	- `string? EncryptionKey`: The key used when encrypting data. Defaulted if set to null.
	- `string? EncryptionIV`: The IV used when encrypting data. Defaulted if set to null.
- `DbConnectionSettings`: a subsection containing:
	- `int Endpoint`: An Enum value: 0 - NoDB | 1 - SQLServer | 2 - CosmosDB
	- `string? ConnectionString`: The connection string used for the database

Alternatively, some values can be configured from command line arguments or environment variables. These values, albeit randomly generated and hard to memorize, are HARD-CODED, OPEN SOURCE and a SECURITY LIABILITY

| Value | Config File | Command Line | Environment Variable | Has Default Value |
| :-: | :-: | :-: | :-: | :-: | 
| JWT Security Key | Security.JWTSecurityKey | 0 | JsonWebTokenKey | True |
| HashKey | Security.HashKey | 1 | SecretHashKey | True |
| Encryption Key | Security.EncryptionKey | 2 | EncryptionKey | False |
| Encryption IV | Security.EncryptionIV | 3 | EncryptionIV | False |

## Usage

### Client
I've developed a [Client](/D_API.Lib/) to simplify the usage of the API in your app, it offers methods for every endpoint and is comprehensively customizable, and is complete with automatic [rate limiting](/D_API.Lib/Types/D_APIRequestQueue.cs)
Refer to the appropiate [README](/D_API.Lib/README.md) for more info.

### Accessing The API
In order to use the API, you must be a registered user. Upon account creation, you will be granted a unique Id, an identifier of your choosing, and a secret.
All three pieces of data must be correct before a new session can be opened for you on request. A new session can be requested via `GET:*/api/v1/auth/newsession`, after which you'll receive a JWT you can then use via `GET:*/api/v1/auth/renew` to obtain a request token, which you can use to access your account's services.

### Endpoints

The following is a list of all endpoints in this API, along with the type of response and Http Status Code they return. For more info on response types, view the section below this one.

* `API/v1/data`
	
	This controller is intended to be used to store information for certain apps, like configuration files for apps that are hosted in a location with no disk permanence, or otherwise where a secret, on location configuration file is not easily accesible or modifiable.

	Defined in [AppDataHost.cs](/D_API/Controllers/AppDataHost.cs), this controller has the following `endpoints`:
	- `GET:*/api/v1/data/download/{datakey}`: Downloads the requested data by the given key, as long as the user has access to the resource.
	
		Returns
		- `403 Forbidden, BadUserKey` If the user is invalid
		- `400 Bad Request, BadDataKey` If the request is invalid.
		- `403 Forbidden, DataQuotaExceeded` If the user has exceeded their daily download quota
		- `404 Not Found, DataDownloadFailure` If the given data key does not belong to any piece of data the user has access to.
		- `401 Unauthorized, DataDownloadFailure` If the user does not have access to the requested piece of data.
		- `200 OK, DataDownloadSuccess` Along with the data
	- `POST:*/api/v1/data/upload/{datakey}`: Uploads the desired data from the body by the given key, as long as the user has access to the resource.
	
		Returns
		- `403 Forbidden, BadUserKey` If the user is invalid
		- `400 Bad Request, BadDataKey` If the request is invalid.
		- `404 Not Found, DataUploadFailure` If the given data key does not belong to any piece of data the user has access to.
		- `403 Forbidden, DataQuotaExceeded` If the user has exceeded their storage quota
		- `403 Forbidden, DataUploadFailure` If the data already exists, and overwrite is not set to true
		- `403 Forbidden, DataQuotaExceeded` If the user has exceeded their daily upload quota
		- `401 Unauthorized, DataUploadFailure` If the user does not have access to the requested piece of data.
		- `200 OK, DataUploadSuccess` Along with the data
		
		Request
		- UploadRequest
		```json
		{
			"Data": null or a one dimensional array of bytes,
			"Overwrite": true or false
		}
		```
		If Data is null, the desired Datakey will be deleted. If Data already previously existed, and Overwrite is not present or set to false, the request will be rejected.
	- `GET:*/api/v1/data/access/{datakey}`: Verifies if the current user has access to the requested data
	
		Returns
		- `403 Forbidden, BadUserKey` If the user is invalid.
		- `400 BadRequest, BadDataKey` If the request is invalid
		- `200 OK, AccessCheck` With a `boolean` response representing if the user has access to the resource
	- `GET:*/api/v1/data/transferreport`: Returns a report representing the current usage and quotas of the user
	
		Returns
		- `403 Forbidden, BadUserKey` If the user is invalid.
		- `200 OK, TransferQuotaStatus` With the report

	*Privacy Notice:* User storage is unencrypted by default and can potentially be accessed by administrative entities. It's highly recommended to encrypt sensitive data before uploading. Saved data cannot be enumerated through the API (Guaranteed true only in [The original repo](https://github.com/DiegoG1019/D_API/))
	
* `API/v1/Auth`
	
	This controller is intended to facitiliate user authentication to request a JWT
	
	Defined in [AuthController.cs](/D_API/Controllers/AuthController.cs), this controller has the following `endpoints`:
	- `GET:*/api/v1/auth/newsession`: Requests a session token from the API, that lasts 1 hour, which will be granted if the user is succesfully authenticated through the credentials passed in the request body. The token granted by this endpoint is only useful to request for Request tokens through renew
	
		Returns 
		- `200 Ok, NewSessionSuccess` Along with the Session JWT if the user is succesfully authorized
		- `401 Unauthorized, NewSessionFailure` Along with an explanation, if the user failed authentication or if the user's credentials have been revoked
		- `403 Forbidden, NewSessionFailure` If the credentials are not recognized. 
		- `400 Bad Request, NewSessionBadRequest` if the body is malformed, along with a list of problems.
	- `GET:*/api/v1/auth/renew`: When the user uses a Session Token on this endpoint, the server will return a request token which will allow the user to issue requests to other endpoints in the API for 30 seconds, until they request a new one. 
	
		Returns 
		- `200 Ok, RenewSessionSuccess` response along the Request JWT if the user is found and validated 
		- `403 Forbidden, BadUserKey` If the token is invalid
		- `401 Unauthorized, RenewSessionFailure` If the user key's could not be found, was revoked, or is inactive
	- `GET:*/api/v1/auth/status`: 
	
		Returns
		- `200 Ok, AuthStatus` Signifying that the token is still valid. 
		- `401 Unauthorized, AuthStatus` If the user is unaunthenticated or is invalid
		- `400 BadRequest, AuthStatus` If the user sent a JWT that is not a request JWT
	
* `API/v1/User`
	
	This controller is dedicated to handling user-related requests, like creating a new user

	Defined in [AuthController.cs](/D_API/Controllers/UserController.cs), this controller has the following `endpoints`:
	- `POST:*/api/v1/user/create`: Obtains `UserCreationData` from the body of the request. 
	
		Returns
		- `403 Forbidden, NewUserFailure` with a reason message if the user already exists or if the request was, for some reason, denied. 
		- `200 OK, NewUserSuccess` if the request was accepted and the services were configured correctly.
		
		Request
		- UserCreationData
		```json
		{
			"Identifier": "*",
			"Roles": "*,*",
			"Data": null or [
				{
					"Service": 0, --AppDataHost
					"Data": [
						null or number Upload,
						null or number Downoad,
						null or number Storage
					]
				}
			]
		}
		```
		Data can be null, or have only the desired service configuration data. If Data contains multiple configuration settings for the same service, the entire request will be rejected.

### Response Types
All responses from this API are encapsulated in a response object, all which contain an `APIResponseCode` and a `string Title`
They are grouped by Controller as the first digit, and Endpoint by the second digit

- `UnspecifiedError = 0, string? ErrorType, string? Message`
- `TooManyRequests = 1`
- `Message = 100, string MessageType, string MessageData`
- `NewSessionSuccess = 200, string Token`
- `NewSessionFailure = 201, string Reason, string Details`
- `NewSessionBadRequest = 202, string[] Reasons`
- `RenewSessionSuccess = 210, string Token`
- `RenewSessionFailure = 211, string Reason`
- `RoleReport = 220, string[] Roles`
- `AuthStatus = 230, bool IsAuthorized, bool IsRequestToken`
- `DataUploadSuccess = 300, string DataKey, bool Overwritten`
- `DataUploadFailure = 301, string DataKey, string Reason`
- `DataDownloadSuccess = 310, string DataKey, byte[]? Data`
- `DataDownloadFailure = 311, string DataKey, string Reason`
- `TransferQuotaStatus = 320, TransferReport TransferUsage, TransferReport TransferQuota, double StorageUsage, double StorageQuota`
- `DataQuotaExceeded = 321, double Excess, string Kind`
- `AccessCheck = 340, bool IsAccesible`
- `NewUserSuccess = 400, UserCreationResults? Results`

#### TransferQuotaStatus
```
Class TransferReport {
	double Upload
	double Download
}
```

#### NewUserSuccess
```
Class UserCreationResults {
	UserCreationResult? Result
	UserCredentials? Credentials
	string? ReasonForDenial
	ServiceData[] ServiceData
	string[] ServiceConfigurationResults
}
```

```
Enum UserCreationResult {
	Accepted = 0,
	AlreadyExists = 1,
	Denied = 2
}
```

```
Class UserCredentials {
	Guid Key
	string Secret
	string Identifier
}
```

```
Class ServiceData {
	Service Service
	object[] Data
}
```

```
Enum Service {
	Data = 1 // 0000 0001
}
```
* This enum is a flags enum, each value is set to a PO2 value, and can be or'ed (x|y) to verify certain flags, and and'd (x&y) to add certain flags

#### Examples
##### Unspecified Error
```json
{
	"APIResponseCode": 0,
	"Title": "",
	"ErrorType": null,
	"Message": null
}
```
##### TransferQuotaStatus
```json
{
	"APIResponseCode": 320,
	"Title": "Transfer Report",
	"TransferUsage": {
		"Upload": 0,
		"Download": 0
	},
	"TransferQuota": {
		"Upload": 0,
		"Download": 0
	},
	"StorageUsage": 0,
	"StorageQuota": 0
}
```
##### NewUserSuccess
```json
{
	"APIResponseCode": 320,
	"Title": "Transfer Report",
	"Results": {
		"Result": 0,
		"ReasonForDenial": null,
		"Credentials": {
			"Key": "50df15e2-e933-45a5-911c-0bba1d1dec7a",
			"Secret": ";CX9eZ`+Ñi#d$m,}Y3<JJz'KwQ}shDjd*%|@Zm@Zh5)CpG3f56mJ&Gx5OñM3:%%l",
			"Identifier": "Daniel"
		},
		"ServiceData": [
			{
				"Service": 0,
				"Data": [
					15, 15, 15, 15, "Hello"
				]
			}
		],
		"ServiceConfigurationResults": [
			"Success"
		]
	}

}
```

### Rate Limiting
Just like basically any other API, there are rate limits for every user, to prevent abuse.
The default rate limits are:
- For `ALL` Endpoints:
	- In a single second, a maximum of 5 requests are allowed.
- For `*/api/test/*`:
	- In a single second, a maximum of 2 requests are allowed.
- For `*/api/v1/auth/*`:
	- In a single second, a maximum of 1 requests are allowed.
	
Some endpoints are whitelisted from rate limiting, they are:
- `*:/api/v1/auth/status`
- `GET:/api/license` (currently inactive)

If you wish to change these, refer to [appsettings.json](/D_API/appsettings.json)

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)

# Support my Projects!
<a href="https://www.buymeacoffee.com/DiegoG1019"><img src="https://img.buymeacoffee.com/button-api/?text=Buy me a coffee&emoji=&slug=DiegoG1019&button_colour=5F7FFF&font_colour=ffffff&font_family=Inter&outline_colour=000000&coffee_colour=FFDD00"></a>

[![paypal.me/diegog1019](https://ionicabizau.github.io/badges/paypal.svg)](https://paypal.me/diegog1019)
