# D_API

D_API is a personal project to practice API building, security, authorization and serving

I'll be using it primarily for some of my own applications where I'd like certain configuration or other information to be readily available from a central, trusted location (and with known disk permanence, as opposed to some projects)

## Table of Contents
- [Installation](#installation)
	- [Configuration](#configuration)
- [Usage](#usage)
	- [Client](#client)
	- [Generating API Keys](#generating-new-api-keys-json-web-tokens-jwt)
	- [Endpoints](#endpoints)
	- [Rate Limiting](#rate-limiting)
- [Contributing](#contributing)
- [License](#license)
- [Support My Project](#support-my-projects)

## Installation
The installation process is much like any other ASP.NET API Project: Set up a reverse proxy, open up the firewall and run the thing

Aside from that, the app will need to be run once before actually being used, so that it can create all configuration templates you can then fill out. It's highly recommended to introduce an environment variable called `D_API_dir`, whose value is a valid directory the app has read/write access to, where it can manage its configurations and other data

### Configuration
The program will create a `apisettings.cfg.json` file somewhere in the file system, this location should appear in the console output
- `string APIKey`: The Telegram API Key of the bot you want to use to interface with the D_API, in order to gain access to useful data such as the current status, and to generate keys
- `List<long> AllowedUsers`: A list of user Ids for the bot to respond to. Any user that is not registered here will be IGNORED.
- `long? EventChannelId`: A channel Id to which the bot will output important (Warning, Error and Fatal) log messages. Set to `null` to ignore, defaults to `0` (invalid)
- `Security`: a subsection containing:
	- `Audience`: The audience value with which to generate new keys. Generated keys will not work without this value.
	- `Issuer`: The issuer value with which to generate new keys. Generated keys will not work without this value.

Other than all that, it's also __highly__ recommended that the user passes a private generation key to the app as a command line argument, as the default key, albeit randomly generated and hard to memorize, is OPEN SOURCE and a SECURITY LIABILITY

## Usage

### Client
I've developed a [Client](/D_API.Lib/) to simplify the usage of the API in your app, it offers methods for every endpoint and is comprehensively customizable, and is complete with automatic [rate limiting](/D_API.Lib/Types/D_APIRequestQueue.cs)
Refer to the appropiate [README](/D_API.Lib/README.md) for more info.

### Generating new API Keys (Json Web Tokens (jwt))
That's what the bot is for! Assumming you've configured everything correctly as per the previous section, all you need to do is go over to the Telegram bot you registered, type `/help`, and it'll tell you what to do

### Endpoints

* API/TEST
	This controller contains methods intended to be used to test the API and probe its availability
	
	Defined in [TestController.cs](/D_API/Controllers/TestController.cs), this controller has the following `endpoints`:
	- `GET:*/api/test/probe`: Probes the API to see if it's alive, can be used anonymously (without a JWT)
	- `GET:*/api/test/probeAuth`: Probes the API, can only be used by authenticated users, under any role
	- `GET:*/api/test/probeAuthMod`: Probes the API, can only be used by authenticated users under the `mod`, `admin`, or `root` roles
	- `GET:*/api/test/probeAuthAdmin`: Probes the API, can only be used by authenticated users under the `admin` or `root` roles
	- `GET:*/api/test/probeAuthRoot`: Probes the API, can only be used by authenticated users under the `root` roles

* API/v1/AppDataHost
	This controller is intended to be used to store information for certain apps, like configuration files for apps that are hosted in a location with no disk permanence, or otherwise where a secret, on location configuration file is not easily accesible or modifiable.

	Defined in [AppDataHost.cs](/D_API/Controllers/AppDataHost.cs), this controller has the following `endpoints`:
	- `GET:*/api/v1/appdatahost/config/{appname}`: Obtains the data associated with the given `appname`, can only be used by authenticated users. Returns `200 Ok` response along with the data if found and the user (The `name` value given when generating the key) is the original writer, `403 Forbidden` if the data is found but the user is not the original writer, and `404 Not Found` if the data is simply not there
	- `POST:*/api/v1/appdatahost/config/{appname}/`: Equals `POST:*/api/v1/appdatahost/config/{appname}/false`
	- `POST:*/api/v1/appdatahost/config/{appname}/{ow}`: Saves the contents of the request's body to a non-volatile (Survives server and machine downtime) location, if the user is the original writer, or the data did not previously exist. Cannot overwrite existing data even if access is granted, unless `ow` is set to `true`

*Privacy Notice:* Users under the `root` role have unrestricted (read/write) access to this data, as long as they know the appname its registered under. Saved data cannot be enumerated through the API (Guaranteed true only in [The original repo](https://github.com/DiegoG1019/D_API/))

### Rate Limiting
Just like basically any other API, there are rate limits for every user, to prevent abuse.
The default rate limits are:
- 5 Requests per second. _If you were to send 5 requests every second for a minute, you'd send 300 requests. But..._
- Within 1 minute, a maximum of 80 requests are allowed. _If you were to send 80 requests every minute for 12 hours, you'd send 57,600 requests. But..._
- Within 12 hours, a maximum of 25,000 requests are allowed. _If you were to send 25,000 requests every 12 hours for a week, you'd send 350,000 requests. But..._
- Within a week, a maximum of 100,000 requests are allowed. _If you were to send 100,000 requests every week for 30 days, you'd send 1,500,000 requests. Please don't._

If you wish to change these, refer to [appsettings.json](/D_API/appsettings.json)

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)

# Support my Projects!
<a href="https://www.buymeacoffee.com/DiegoG1019"><img src="https://img.buymeacoffee.com/button-api/?text=Buy me a coffee&emoji=&slug=DiegoG1019&button_colour=5F7FFF&font_colour=ffffff&font_family=Inter&outline_colour=000000&coffee_colour=FFDD00"></a>

[![paypal.me/diegog1019](https://ionicabizau.github.io/badges/paypal.svg)](https://paypal.me/diegog1019)
