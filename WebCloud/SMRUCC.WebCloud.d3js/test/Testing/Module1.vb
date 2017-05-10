Module Module1

    Sub Main()
        Dim json = SMRUCC.WebCloud.d3js.Network.htmlwidget.BuildData.BuildGraph("D:\GCModeller\src\runtime\httpd\WebCloud\SMRUCC.WebCloud.d3js\test\viewer.html")
        Call json .Save ("./test")
    End Sub
End Module
