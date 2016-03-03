Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.HTTPInternal.Platform

Namespace AppEngine

    ''' <summary>
    ''' 调用和注册外部模块为rest服务的插件，从这里拓展核心服务层
    ''' </summary>
    Public Module ExternalCall

        Public Function Scan(Platform As PlatformEngine) As Boolean
            Dim dlls = FileIO.FileSystem.GetFiles(App.HOME, FileIO.SearchOption.SearchTopLevelOnly, "*.dll")

            For Each dllFile As String In dlls
                Try
                    Call ParseDll(dllFile, Platform)
                Catch ex As Exception
                    ex = New Exception(dllFile, ex)
                    Call ex.PrintException
                    Call App.LogException(ex)
                End Try
            Next

            Return True
        End Function

        ''' <summary>
        ''' Register external WebApp as services.
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <param name="platform"></param>
        ''' <returns></returns>
        Public Function ParseDll(dll As String, platform As PlatformEngine) As Integer
            Dim assm As Reflection.Assembly = Reflection.Assembly.LoadFile(dll)
            Dim types As Type() = (From typeDef As Type
                                   In assm.GetTypes
                                   Where typeDef.IsInheritsFrom(GetType(WebApp)) AndAlso Not typeDef.IsAbstract
                                   Select typeDef).ToArray
            If types.Length = 0 Then
                Return -1
            End If

            Dim Apps As WebApp() =
                types.ToArray(Of WebApp)(
                Function(typeDef As Type) DirectCast(Activator.CreateInstance(typeDef, {platform}), WebApp))

            For Each app As WebApp In Apps
                Call platform.AppManager.Register(app)
            Next

            Return Apps.Length
        End Function
    End Module
End Namespace