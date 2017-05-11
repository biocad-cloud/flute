Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Module Module1

    Sub Main()
        Dim json = SMRUCC.WebCloud.d3js.Network.htmlwidget.BuildData.BuildGraph("../../..\viewer.html")
        Dim colors As SolidBrush() = d3js _
            .ScaleChromatic _
            .category10 _
            .Select(Function(c) New SolidBrush(c)) _
            .ToArray
        Dim graph = json.CreateGraph(Function(n) colors(CInt(n.NodeType)))
        Call graph.doRandomLayout
        Call graph.doForceLayout(showProgress:=True, Repulsion:=2500, Stiffness:=50, Damping:=.25, iterations:=1500)
        Call graph.Tabular.Save("./test")
        Call graph.DrawImage(
            New Size(1600, 1600),
            scale:=4.5,
            labelColorAsNodeColor:=True).Save("../../..\/viewer.png")
    End Sub
End Module
