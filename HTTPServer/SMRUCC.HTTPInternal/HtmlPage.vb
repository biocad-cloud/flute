Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Public Class HtmlPage : Inherits ClassObject

    Public Property Url As String
    Public Property Title As String
    Public Property html As String

    Public Function BuildPage(template As String) As String
        Dim sb As New StringBuilder(template)

        Call sb.Replace("{title}", Title)
        Call sb.Replace("{HTML}", html)

        Return sb.ToString
    End Function

    Public Shared Function LoadPage(path As String, wwwroot As String) As HtmlPage
        Dim html As String = path.ReadAllText
        Dim head As String = Regex.Match(html, "---.+?---", RegexOptions.Singleline).Value
        Dim title As String = Regex.Match(head, "title:.+?$", RegexICMul).Value
        Dim url As String =
            ProgramPathSearchTool.RelativePath(wwwroot, path)

        html = Mid(html, head.Length + 1).Trim
        title = title.GetTagValue(":").x.Trim

        Return New HtmlPage With {
            .html = html,
            .Title = title,
            .Url = url
        }
    End Function
End Class
