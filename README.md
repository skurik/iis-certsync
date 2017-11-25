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
    
4. Try running the script (see explanation of the command line parameters below):

    `scriptcs c:\SSL\scripts\update-web-farm-bindings.csx -- c:\SSL\scripts\log.txt MySite "*:443:www.mysite.com" "MySiteBlue|*:44301:www.mysite.com,MySiteGreen|*:44302:www.mysite.com"`
    
The script is being called as follows:

    `scriptcs <script-path> -- <log-path> <source-site> <source-site-binding> <list-of-targets>`
    
Where

* `<source-site>` is the name of the site you want to copy the certificate settings from. This will be the site whose certificate Let's Encrypt is renewing properly.
* `<source-site-binding>` specifies which source site binding we should use to get the certificate. The pattern is `<ip>:<port>:<hostname>` with * being used for non-specified values
* `<list-of-targets>` is a comma-separated list of pairs of the form `<site-name>|<binding>` with both components having the same form as the source site equivalent described above
    
After you tune your command line, I suggest adding the command as an additional action of the Let's Encrypt scheduled task.
