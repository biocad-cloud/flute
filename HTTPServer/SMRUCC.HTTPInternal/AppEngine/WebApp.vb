Imports System.IO
Imports SMRUCC.HTTPInternal.Platform

Namespace AppEngine

    Public MustInherit Class WebApp : Inherits PlatformSub

        Sub New(main As PlatformEngine)
            Call MyBase.New(main)
        End Sub

        ''' <summary>
        ''' 404模板
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function Page404() As String

        Public Overrides Function ToString() As String
            Return $"{PlatformEngine.ToString} ==> {Me.GetType.Name}"
        End Function

        Public Delegate Function GET_API(args As String) As String
        Public Delegate Function POST_API(args As String, params As StreamReader) As String

    End Class
End Namespace