Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program

    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/make")>
    <Usage("/make --site <url, example as: https://website.com> [--out <output_dir, default=./> --sleep <request_interval,default=0.5s>]")>
    <Description("Make sitemap.xml file")>
    <Argument("--site", False, CLITypes.String, AcceptTypes:={GetType(String)}, Description:="target website url to make sitemap, example as: https://website.com")>
    <Argument("--out", True, CLITypes.File, AcceptTypes:={GetType(String)}, Description:="result sitemap file output dir path, default is in current workdir. a sitemap.xml and sitemap.xsl will generated inside this output dir.")>
    <Argument("--sleep", True, CLITypes.Double, AcceptTypes:={GetType(Double)}, Description:="thread sleep interval in time unit 'seconds' before fetch next url page.")>
    Public Function MakeSitemap(site As String, Optional out As String = "./", Optional sleep As Double = 0.5, Optional args As CommandLine = Nothing) As Integer

    End Function
End Module
