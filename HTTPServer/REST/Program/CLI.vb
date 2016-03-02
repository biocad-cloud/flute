Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.REST.Platform

Module CLI

    <ExportAPI("/start", Usage:="/start [/port 80 /root <wwwroot_DIR>]")>
    Public Function Start(args As CommandLine.CommandLine) As Integer
        Dim cfg As Configs = Configs.LoadDefault
        Dim port As Integer = args.GetValue("/port", cfg.Portal)
        Dim HOME As String = args.GetValue("/root", cfg.WWWroot)
        cfg.Portal = port
        cfg.WWWroot = HOME
        cfg.Save()
        Return New PlatformEngine(HOME, port, True).Run
    End Function

    <ExportAPI("/run", Usage:="/run /dll <app.dll> [/port <80> /root <wwwroot_DIR>]")>
    Public Function RunApp(args As CommandLine.CommandLine) As Integer
        Dim cfg As Configs = Configs.LoadDefault
        Dim port As Integer = args.GetValue("/port", cfg.Portal)
        Dim HOME As String = args.GetValue("/root", cfg.WWWroot)
        Dim dll As String = args.GetValue("/dll", cfg.App)
        cfg.App = dll
        cfg.Portal = port
        cfg.WWWroot = HOME
        cfg.Save()
        Return New PlatformEngine(HOME, port, True, dll).Run
    End Function
End Module
