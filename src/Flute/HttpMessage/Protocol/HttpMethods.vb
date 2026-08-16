Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Core.Message.HttpHeader

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class HttpGet : Inherits ExportAPIAttribute

        Sub New(url As String)
            Call MyBase.New(url)
        End Sub

        Public Overrides Function ToString() As String
            Return $"http-get('{Name}')"
        End Function

    End Class

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class HttpPost : Inherits ExportAPIAttribute

        Sub New(url As String)
            Call MyBase.New(url)
        End Sub

        Public Overrides Function ToString() As String
            Return $"http-post('{Name}')"
        End Function
    End Class
End Namespace