Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Serialization.JSON
Imports NetGraphData = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Network

Namespace Network.htmlwidget

    ''' <summary>
    ''' 将``htmlwidget``之中的D3.js网络模型解析为scibasic的标准网络模型
    ''' </summary>
    Public Module BuildData

        Const JSON$ = "<script type[=]""application/json"".+?</script>"

        ''' <summary>
        ''' 参数为html文本或者url路径
        ''' </summary>
        ''' <param name="html$"></param>
        ''' <returns></returns>
        Public Function ParseHTML(html$) As String
            If Not html.FileExists Then
                html = html.GET
            End If

            html = Regex.Match(html, BuildData.JSON, RegexICSng).Value
            html = html.GetStackValue(">", "<")

            Return html
        End Function

        Public Function BuildGraph(html$) As NetGraphData
            Dim json$ = BuildData.ParseHTML(html)
            Dim data As htmlwidget.NetGraph = json.LoadObject(Of htmlwidget.JSON).x

        End Function
    End Module
End Namespace