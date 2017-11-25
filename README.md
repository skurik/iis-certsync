# iis-webfarm-certsync
A script to update multiple IIS sites' HTTPS certificate based on a source site

If you are using Let's Encrypt to renew your certificates on IIS and at the same time use Application Request Routing to provide load balancing or to facilitate a blue/green deploy, chances are your blue/green sites are not having their certificates renewed properly when the main, public-facing server has.

This leads to all HTTPS requests failing with __502__, but can be fixed simply by assigning the appropriate certificate to the blue/green sites.

This script provides a simple workaround - whenever Let's Encrypt finishes its automatic renewal task, the script makes sure the certificates are in sync.

## Usage

1. Install [scriptcs](https://github.com/scriptcs)
2. Download the __update-web-farm-bindings.csx__ script and __scriptcs_packages.config__
3. Install NuGet packages required by the script:

    `scriptcs -install`
