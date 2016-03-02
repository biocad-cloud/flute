Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function
End Module

Module CLI

    ''' <summary>
    ''' Run the http server
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/start", Usage:="/start [/port <default:=80> /root <./wwwroot>]",
               Info:="Start the simple http server.",
               Example:="/start /root ~/.server/wwwroot/ /port 412")>
    <ParameterInfo("/port", True,
                   Description:="The data port for this http server to bind.")>
    <ParameterInfo("/root", True,
                   Description:="The wwwroot directory for your http html files, default location is the wwwroot directory in your App HOME directory.")>
    Public Function Start(args As CommandLine.CommandLine) As Integer
        Dim port As Integer = args.GetValue("/port", 80)
        Dim root As String = args.GetValue("/root", App.HOME & "/wwwroot")
        Return New HttpInternal.HttpFileSystem(port, root, True).Run
    End Function
End Module
