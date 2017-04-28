Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text

Namespace Scripting

    Public Module Interpolate

        Const Expression$ = "<%= [^>]+? %>"

        ''' <summary>
        ''' ``&lt;%= relative_path %>``
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ReadHTML(path$, Optional encoding As Encodings = Encodings.UTF8) As String
            Dim codepage As Encoding = encoding.CodePage
            Dim parent$ = path.ParentPath
            Dim html As New StringBuilder(path.ReadAllText(codepage))
            Dim includes$() = Regex _
                .Matches(html.ToString, Interpolate.Expression, RegexICSng) _
                .ToArray

            ' <%= include_path %>

            For Each include As String In includes
                Dim rel_path$ = include.Trim("<"c, ">"c, "%"c)
                rel_path = Mid(rel_path, 2).Trim  ' 去除等号
                rel_path = parent & "/" & rel_path
                rel_path = FileIO.FileSystem.GetFileInfo(rel_path).FullName
                Dim content$ = rel_path.ReadAllText(codepage)
                Call html.Replace(include, content)
            Next

            Return html.ToString
        End Function
    End Module
End Namespace