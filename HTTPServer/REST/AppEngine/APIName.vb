Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace AppEngine

    Public Module APIName

        <Extension> Public Function GetAPIName(api As System.Reflection.MethodInfo) As String
            Dim entry As ExportAPIAttribute = api.GetAttribute(Of ExportAPIAttribute)
            If entry Is Nothing Then
                Return ""
            Else
                Return entry.Name
            End If
        End Function

        Public Function GetAPIName(api As WebApp.GET_API) As String
            Return api.Method.GetAPIName
        End Function

        Public Function GetAPIName(api As WebApp.POST_API) As String
            Return api.Method.GetAPIName
        End Function
    End Module
End Namespace