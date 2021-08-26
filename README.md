# D_API

D_API is a personal project to practice API building, security, authorization and serving

I'll be using it primarily for some of my own applications where I'd like certain configuration or other information to be readily available from a central, trusted location (and with known disk permanence, as opposed to some projects)

## Installation
The installation process is much like any other ASP.NET API Project: Set up a reverse proxy, open up the firewall and run the thing

## Usage

_W.I.P._

### Rate Limiting
Just like basically any other API, there are rate limits for every user, to prevent abuse.
The default rate limits are:
- 5 Requests per second. _If you were to send 5 requests every second for a minute, you'd send 300 requests. But..._
- Within 1 minute, a maximum of 80 requests are allowed._If you were to send 80 requests every minute for 12 hours, you'd send 57,600 requests. But..._
- Within 12 hours, a maximum of 25,000 requests are allowed. _If you were to send 25,000 requests every 12 hours for a week, you'd send 350,000 requests. But..._
- Within a week, a maximum of 100,000 requests are allowed. _If you were to send 100,000 requests every week for 30 days, you'd send 1,500,000 requests. Please don't._

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)

# Support my Projects!
<a href="https://www.buymeacoffee.com/DiegoG1019"><img src="https://img.buymeacoffee.com/button-api/?text=Buy me a coffee&emoji=&slug=DiegoG1019&button_colour=5F7FFF&font_colour=ffffff&font_family=Inter&outline_colour=000000&coffee_colour=FFDD00"></a>

[![paypal.me/diegog1019](https://ionicabizau.github.io/badges/paypal.svg)](https://paypal.me/diegog1019)
