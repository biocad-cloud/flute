﻿#Region "Microsoft.VisualBasic::ffc5785f0d51027613d1cfff3dd738b0, ..\httpd\HTTPServer\REST\Program\CLI.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.WebCloud.HTTPInternal.Platform

<GroupingDefine(CLI.httpdServerCLI, Description:="Server CLI for running this httpd web server.")>
Module CLI

    Public Const httpdServerCLI$ = NameOf(httpdServerCLI)
    Public Const Utility$ = NameOf(Utility)

    <ExportAPI("/start",
               Info:="Run start the httpd web server.",
               Usage:="/start [/port 80 /root <wwwroot_DIR> /threads -1 /cache]")>
    <Argument("/port", True, CLITypes.Integer,
              AcceptTypes:={GetType(Integer)},
              Description:="The server port of this httpd web server to listen.")>
    <Argument("/root", True, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(String)},
              Description:="The website html root directory path.")>
    <Argument("/threads", True, CLITypes.Integer,
              AcceptTypes:={GetType(Integer)},
              Description:="The number of threads of the server thread pool.")>
    <Argument("/cache", True, CLITypes.Boolean,
              AcceptTypes:={GetType(Boolean)},
              Description:="Is this server running in file system cache mode? Not recommended for open.")>
    <Group(httpdServerCLI)>
    Public Function Start(args As CommandLine) As Integer
        Dim port As Integer = args.GetValue("/port", 80)
        Dim HOME As String = args.GetValue("/root", App.CurrentDirectory)
        Dim threads As Integer = args.GetValue("/threads", -1)
        Dim cacheMode As Boolean = args.GetBoolean("/cache")

        Return New PlatformEngine(HOME, port,
                                  True,
                                  threads:=threads,
                                  cache:=cacheMode).Run
    End Function

    <ExportAPI("/run",
               Info:="Run start the web server with specific Web App.",
               Usage:="/run /dll <app.dll> [/port <80> /root <wwwroot_DIR>]")>
    <Group(httpdServerCLI)>
    Public Function RunApp(args As CommandLine) As Integer
        Dim port As Integer = args.GetValue("/port", 80)
        Dim HOME As String = args.GetValue("/root", App.CurrentDirectory)
        Dim dll As String = args.GetValue("/dll", "")
        Return New PlatformEngine(HOME, port, True, dll).Run
    End Function

    <ExportAPI("/GET",
               Info:="Tools for http get request the content of a specific url.",
               Usage:="/GET /url [<url>/std_in] [/out <file/std_out>]")>
    <Argument("/url", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(String)},
              Description:="The resource URL on the web.")>
    <Argument("/out", True, CLITypes.File, PipelineTypes.std_out,
              AcceptTypes:={GetType(String)},
              Description:="The save location of your requested data file.")>
    Public Function [GET](args As CommandLine) As Integer
        Dim url As String = args.ReadInput("/url")

        VBDebugger.ForceSTDError = True

        Using out As StreamWriter = args.OpenStreamOutput("/out")
            Dim html As String = url.GET
            Call out.Write(html)
        End Using

        Return 0
    End Function
End Module
