# Atlas
*the launch rocket of the gemini capsule*

* C#
* net7.0
* zero dependencies
* Linux and Windows
* x86, x64 and ARM

## Features
* analythics
* server side animations on supported clients (eg. Lagrange)
* gemini:// with titan:// file uploads
* automatic certificate generation if none specified
* spartan:// file uploads and downloads
* CGI interface compatible with [jetforce](https://github.com/michael-lazar/jetforce) 
* CGI streaming (for things like gemini://chat.mozz.us/)
* vhosts
* directory listing
* easy JSON config file
* easy tsv Mimetype map
* dockerfile available

## analythics
# Atlas Statistics

## Hits

```
9	                                                ███                     
8	                                                ███                     
8	                                                ███                     
7	                                                ███                     
7	                                                ███                     
6	                                                ███                     
5	                                                ███                     
5	                                                ███                     
4	                                                ███                     
4	                                                ███                     
3	                                                ███                     
2	                                                ███                     
2	                                                ███                     
1	                                                ███                     
1	                                                ███                     
 	Jan   Feb   Mar   Apr   May   Jun   Jul   Aug   Sep   Oct   Nov   Dec   
```

## Requests

```
[1]	█████████████████████████████████████████████████████████████████	33%
[2]	█████████████████████████████████████████████████████████████████	33%
[3]	██████████████████████                                           	11%
[4]	██████████████████████                                           	11%
[5]	██████████████████████                                           	11%

[1]	atlas.stats
[2]	index.gmi
[3]	blog/atlas-my-gemini-server.gmi
[4]	pages/atlas.gmi
[5]	blog/the-http-web-is-a-lost-cause.gmi
```

## Bandwidth (Day)

```
37K	                                                ████            
35K	                                                ████            
32K	                                                ████            
30K	                                                ████            
27K	                                                ████            
25K	                                                ████            
22K	                                                ████            
20K	                                                ████            
17K	                                                ████            
15K	                                                ████            
12K	                                                ████            
10K	                                                ████            
7K	                                                ████            
5K	                                                ████            
2K	                                                ████            
 	Sun     Mon     Tue     Wed     Thu     Fri     Sat     Sat     
```


### Roadmap (in no particular order):

* FastCGI
* Use single Docker volume
* caching
* certificate validation
* rate limiting
* proper networking with SocketAsyncEventArgs
* * not a priority, testing shows it scales to a few 100 concurrent users

### Sample configuration
```json
{
  "SlowMode": true, // animations, currently only for gemini
  "GeminiPort": 1965,
  "SpartanPort": 300,
  "Capsules": {
    "allsafe.net": {
      "AbsoluteRootPath": "/srv/gemini/allsafe.net/",
      "AbsoluteTlsCertPath": "/srv/gemini/allsafe.net/allsafe.net.pfx",
      "FQDN": "allsafe.net",
      "Index": "index.gmi",
      "Locations": [
        {
          "AbsoluteRootPath": "/srv/gemini/allsafe.net/",
          "Index": "index.gmi",
        }
      ]
    },
    "evilcorp.net": {
      "AbsoluteRootPath": "/srv/gemini/evilcorp.net/",
      "AbsoluteTlsCertPath": "",// will be automatically created and placed at AbsoluteRootPath/FQDN.pfx
      "FQDN": "evilcorp.net",
      "Index": "index.gmi",
      "MaxUploadSize": 4194304, // global max upload size
      "Locations": [
        {
          "AbsoluteRootPath": "/srv/gemini/evilcorp.net/",
          "Index": "index.gmi",
        },
        {
          "AbsoluteRootPath": "/srv/gemini/evilcorp.net/cgi/",
          "Index": "script.csx",
          "CGI": true,
          "RequireClientCert": true,  // disables access for spartan protocol due to lack of support
        },
        {
          "AbsoluteRootPath": "/srv/gemini/evilcorp.net/files/",
          "Index": "index.gmi",
          "DirectoryListing": true, 
          "AllowFileUploads": true, // public Titan/Spartan  uploads in this location
          "AllowedMimeTypes": {
            "text/*": { // whitelist all text files
              "MaxSizeBytes": 1048576 // override max upload size for text files
            },
            "image/*": {}, // whitelist all image files to be uploaded
            "audio/mpeg": {}, //
            "audio/ogg": {},  // whitelist certain audio files
            "audio/wave": {}  //
          }
        }
      ]
    }
  }
}
```

### sample CGI script

[atlas-comments](https://github.com/Alumniminium/atlas-comments)

### CGI Interface

The CGI interface provides the following environment variables:

| Variable | Description | Default |
|---|---|---|
| DOTNET_CLI_HOME | Required for .NET assemblies to execute | ~/.dotnet |
| GATEWAY_INTERFACE | CGI Version | CGI/1.1 |
| SERVER_PROTOCOL | Either Gemini or Spartan | GEMINI / SPARTAN |
| SERVER_PORT | Gemini or Spartan Port according to config.json | 1965 / 300 |
| SERVER_SOFTWARE | atlas/version string | atlas/0.2b |
| URL | URL of the Request | gemini://evil.corp/cgi/binary?queryString=value#fragment&token |
| SCRIPT_NAME | the CGI script name | binary |
| PATH_INFO | See CGI documentation | Hopefully correct |
| QUERY_STRING | Query from the URL | ?queryString=value#fragment&token |
| SERVER_NAME | the FQDN of the vhost | evil.corp |
| REMOTE_HOST | The IP of the client sending the request | 127.0.0.1 |
| REMOTE_ADDR | as above | as above |
| TLS_VERSION | Gemini Only | 1.3 |
| REMOTE_USER | TLS Cert Subject without CN= | trbl |
| TLS_CLIENT_SUBJECT | as above | as above |
| TLS_CLIENT_VALID | Certificate is not expired | true |
| TLS_CLIENT_TRUSTED | Certificate issued by atlas | false |
| TLS_CLIENT_HASH | The Certificate Thumbprint | 0baf2asdb23i02.. |
| TLS_CLIENT_NOT_BEFORE | Certificate Valid From Time | 08/28/2022 18:26:30 |
| TLS_CLIENT_NOT_AFTER | Certificate Valid To Time | 08/28/3000 18:26:30 |
| TLS_CLIENT_SERIAL_NUMBER | The Certificate Serial Number | |
| AUTH_TYPE | CERTIFICATE or NONE | NONE | 

