Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.REST.AppEngine.APIMethods

Namespace AppEngine

    Public Class __API_Invoker
        Public Property Name As String
        Public Property EntryPoint As System.Reflection.MethodInfo
        Public Property Help As String
        Public Property Method As APIMethod
        Public Property Error404 As String

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Function InvokePOST(obj As Object, args As String, inputs As StreamReader, ByRef result As String) As Boolean
            Try
                Return __invokePOST(obj, args, inputs, result)
            Catch ex As Exception
                Return __handleERROR(ex, args, result)
            End Try
        End Function

        Public Function Invoke(obj As Object, args As String, ByRef result As String) As Boolean
            Try
                Return __invoke(obj, args, result)
            Catch ex As Exception
                Return __handleERROR(ex, args, result)
            End Try
        End Function

        Private Function __handleERROR(ex As Exception, url As String, ByRef result As String) As Boolean
            ex = New Exception("Request page: " & url, ex)
#If DEBUG Then
            result = ex.ToString
#Else
                result = Fakes(ex.ToString)
#End If
            If Not String.IsNullOrEmpty(Error404) Then
                result = result.Replace("--->", "<br />--->")
                result = result.lTokens.JoinBy(vbCrLf & "<br />")
                result = Error404.Replace("%EXCEPTION%", $"<table><tr><td><font size=""2"">{result}</font></td></tr></table>")
            End If

            Return False
        End Function

        Private Function VirtualPath(strData As String(), prefix As String) As Dictionary(Of String, String)
            Dim LQuery = (From source As String In strData
                          Let trimPrefix = Regex.Replace(source, "in [A-Z][:]\\", "", RegexOptions.IgnoreCase)
                          Let line = Regex.Match(trimPrefix, "[:]line \d+").Value
                          Let path = trimPrefix.Replace(line, "")
                          Select source, path).ToArray
            Dim LTokens = (From obj In LQuery Let tokens = obj.path.Split("\"c) Select tokens, obj.source).ToArray
            Dim p As Integer

            For i As Integer = 0 To (From obj In LTokens Select obj.tokens.Count).ToArray.Min - 1
                p = i

                If (From n In LTokens Select n.tokens(p) Distinct).ToArray.Count > 1 Then
                    Exit For
                End If
            Next

            Dim LSkips = (From obj In LTokens Select obj.source, obj.tokens.Skip(p).ToArray).ToArray
            Dim LpreFakes = (From obj In LSkips
                             Select obj.source,
                                 virtual = String.Join("/", obj.ToArray).Replace(".vb", ".vbs")).ToArray
            Dim hash = LpreFakes.ToDictionary(
                Function(obj) obj.source,
                elementSelector:=Function(obj) $"in {prefix}/{obj.virtual}:line {CInt(5000 * RandomDouble() + 100)}")
            Return hash
        End Function

        Private Function Fakes(ex As String) As String
            Dim line As String() = (From m As Match In Regex.Matches(ex, "in .+?[:]line \d+") Select str = m.Value).ToArray
            Dim hash = VirtualPath(line, "/root/ubuntu.d~/->/wwwroot/~mipaimai.com/api.php?virtual=ms_visualBasic_sh:/")
            Dim sbr = New StringBuilder(ex)

            For Each obj In hash
                Call sbr.Replace(obj.Key, obj.Value)
            Next

            Return sbr.ToString
        End Function

        Private Function __invokePOST(obj As Object, argvs As String, inputs As StreamReader, ByRef result As String) As Boolean
            Dim value As Object = EntryPoint.Invoke(obj, {argvs, inputs})
            result = DirectCast(value, String)
            Return True
        End Function

        Private Function __invoke(obj As Object, argvs As String, ByRef result As String) As Boolean
            Dim value As Object = EntryPoint.Invoke(obj, {argvs})
            result = DirectCast(value, String)
            Return True
        End Function
    End Class
End Namespace