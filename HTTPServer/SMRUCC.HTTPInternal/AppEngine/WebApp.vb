Imports System.IO
Imports SMRUCC.HTTPInternal.Platform

Namespace AppEngine

    ''' <summary>
    ''' API interface description: <see cref="GET_API"/>, <see cref="POST_API"/>.
    ''' (外部对象需要继承这个基类才可以在App引擎之中注册自身为服务)
    ''' </summary>
    Public MustInherit Class WebApp : Inherits PlatformSub

        Sub New(main As PlatformEngine)
            Call MyBase.New(main)
        End Sub

        ''' <summary>
        ''' 通过复写这个方法可以使用自定义的404模板
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function Page404() As String

        Public Overrides Function ToString() As String
            Return $"{PlatformEngine.ToString} ==> {Me.GetType.Name}"
        End Function

        ''' <summary>
        ''' <see cref="APIMethods.[GET]"/>
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Delegate Function GET_API(args As String) As String
        ''' <summary>
        ''' <see cref="APIMethods.POST"/>
        ''' </summary>
        ''' <param name="args"></param>
        ''' <param name="params"></param>
        ''' <returns></returns>
        Public Delegate Function POST_API(args As String, params As StreamReader) As String

    End Class
End Namespace