# httpd [version 1.0.0.0]
> SMRUCC.REST.CLI

<!--more-->

**httpd: SMRUCC web cloud application platform**
_httpd: SMRUCC web cloud application platform_
Copyright © R&D, SMRUCC 2016. All rights reserved.

**Module AssemblyName**: file:///C:/Users/Admin/OneDrive/Zika-News/ftp_publish/httpd.exe
**Root namespace**: ``SMRUCC.REST.CLI``


All of the command that available in this program has been list below:

##### Generic function API list
|Function API|Info|
|------------|----|
|[/GET](#/GET)||
|[/run](#/run)||
|[/start](#/start)||

## CLI API list
--------------------------
<h3 id="/GET"> 1. /GET</h3>


**Prototype**: ``SMRUCC.REST.CLI::Int32 GET(args As Microsoft.VisualBasic.CommandLine.CommandLine)``

###### Usage
```bash
httpd /GET /url [<url>/std_in] [/out <file/std_out>]
```
<h3 id="/run"> 2. /run</h3>


**Prototype**: ``SMRUCC.REST.CLI::Int32 RunApp(args As Microsoft.VisualBasic.CommandLine.CommandLine)``

###### Usage
```bash
httpd /run /dll <app.dll> [/port <80> /root <wwwroot_DIR>]
```
<h3 id="/start"> 3. /start</h3>


**Prototype**: ``SMRUCC.REST.CLI::Int32 Start(args As Microsoft.VisualBasic.CommandLine.CommandLine)``

###### Usage
```bash
httpd /start [/port 80 /root <wwwroot_DIR> /threads -1 /cache]
```
