Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.HTTPInternal.AppEngine.APIMethods
Imports SMRUCC.HTTPInternal.AppEngine.APIMethods.Arguments
Imports SMRUCC.HTTPInternal.Platform

<[Namespace]("Example")>
Public Class ExampleApp : Inherits SMRUCC.HTTPInternal.AppEngine.WebApp

    Public Sub New(main As PlatformEngine)
        MyBase.New(main)
    End Sub

    <[GET](GetType(Integer()))>
    <ExportAPI("/example/test.json")>
    Public Function JsonExample(req As HttpRequest, response As HttpResponse) As Boolean
        Dim test As Integer() = {1, 2, 3, 4, 5, 6, 11, 2, 3, 689, 3453, 4}  ' replace this with your operation code, something like sql query result.
        Call response.WriteJSON(test)
        Return True
    End Function

    Public Overrides Function Page404() As String
        Throw New NotImplementedException()
    End Function
End Class
