Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Visualize

Module Module1

    Sub Main()
        Dim json = SMRUCC.WebCloud.d3js.Network.htmlwidget.BuildData.BuildGraph("../../..\viewer.html")
        Dim graph = json.CreateGraph
        Call graph.doForceLayout(showProgress:=True, iterations:=50)
        Call graph.Tabular.Save("./test")
        Call graph.DrawImage(New Size(2000, 2000), scale:=3).Save("./test.png")
    End Sub
End Module
