Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Linq

Namespace AppEngine

    Public Class Content : Implements IReadOnlyId

        Public Property Name As String Implements IReadOnlyId.locusId
        Public Property content As String
        Public Property FileName As String
        ''' <summary>
        ''' Content-Type: text/plain
        ''' </summary>
        ''' <returns></returns>
        Public Property type As String

        Public Overrides Function ToString() As String
            Dim t As String = type
            If Not String.IsNullOrEmpty(t) Then
                t = $"({t}) "
            End If
            Return $"{t}{Name}"
        End Function

        Public Shared Function Parser(val As String) As Content
            Dim array As String() = val.lTokens.Skip(1).ToArray
            Dim name As String = ""
            Dim fileName As String = ""

            Call __nameParser(array(Scan0), name, fileName)

            Dim contentType As String = __contentType(array(1))
            Dim offset As Integer

            If String.IsNullOrEmpty(contentType) Then
                offset = 2
            Else
                offset = 3
            End If

            array = array.Skip(offset).ToArray
            array = array.Take(array.Length - 1).ToArray

            Dim content As String = array.JoinBy(vbLf)
            Dim contentData As New Content With {
                .Name = name,
                .content = content,
                .FileName = fileName,
                .type = contentType
            }

            Return contentData
        End Function

        '------WebKitFormBoundary2As7gdNwsg669rY1
        'Content-Disposition: form-data; name="email"

        'xie.guigang@gcmodeller.org
        '------WebKitFormBoundary2As7gdNwsg669rY1
        'Content-Disposition: form-data; name="job_title"

        'task test_debug
        '------WebKitFormBoundary2As7gdNwsg669rY1
        'Content-Disposition: form-data; name="memeText"; filename=".NETFramework,Version=v4.6.AssemblyAttributes.vb"
        'Content-Type: text/plain

        'Option Strict Off
        '    Option Explicit On

        '    Imports System
        '    Imports System.Reflection
        '    <Assembly: Global.System.Runtime.Versioning.TargetFrameworkAttribute(".NETFramework,Version=v4.6", FrameworkDisplayName:=".NET Framework 4.6")>

        '------WebKitFormBoundary2As7gdNwsg669rY1
        'Content-Disposition: form-data; name="memeText"


        '------WebKitFormBoundary2As7gdNwsg669rY1--

        Const WebKitFormBoundary As String = "^[-]+WebKitFormBoundary.+?$"

        Public Shared Function ContentParser(post As String) As Content()
            Dim array As String() = Regex.Split(post, WebKitFormBoundary, RegexOptions.Multiline).ToArray
            Dim value As Content() = array.ToArray(Function(x) Parser(x), where:=Function(x) Not String.IsNullOrWhiteSpace(x))
            Return value
        End Function

        ''' <summary>
        ''' Content-Type: text/plain
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Private Shared Function __contentType(input As String) As String
            If String.IsNullOrEmpty(input) Then
                Return ""
            End If
            Dim type As String = Regex.Replace(input, "Content-Type[:]", "", options:=RegexOptions.IgnoreCase).Trim
            Return type
        End Function

        ''' <summary>
        ''' filename=".NETFramework,Version=v4.6.AssemblyAttributes.vb"
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="Name"></param>
        ''' <param name="fileName"></param>
        Private Shared Sub __nameParser(input As String, ByRef Name As String, ByRef fileName As String)
            Name = Regex.Match(input, "name="".+?""").Value
            Name = Regex.Replace(Name, "name=", "", options:=RegexOptions.IgnoreCase)
            Name = Name.GetString
            fileName = Regex.Match(input, "filename="".+?""").Value
            fileName = Regex.Replace(fileName, "filename=", "", options:=RegexOptions.IgnoreCase)
            fileName = fileName.GetString
        End Sub
    End Class
End Namespace