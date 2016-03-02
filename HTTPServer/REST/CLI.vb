Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.REST.Platform

Module CLI

    <ExportAPI("/start", Usage:="/start [/port 80 /root <wwwroot_DIR>]")>
    Public Function Start(args As CommandLine.CommandLine) As Integer
        Dim port As Integer = args.GetValue("/port", 80)
        Dim HOME As String = args.GetValue("/root", App.HOME & "/wwwroot/")
        Return New PlatformEngine(HOME, port, True).Run
    End Function

    <ExportAPI("/run", Usage:="/run /dll <app.dll> [/port <80> /root <wwwroot_DIR>]")>
    Public Function RunApp(args As CommandLine.CommandLine) As Integer
        Dim port As Integer = args.GetValue("/port", 80)
        Dim HOME As String = args.GetValue("/root", App.HOME & "/wwwroot/")
        Dim dll As String = args("/dll")
        Return New PlatformEngine(HOME, port, True).Run
    End Function
End Module
