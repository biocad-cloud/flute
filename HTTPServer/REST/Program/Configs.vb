Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Configs
    Public Property Portal As Integer
    Public Property WWWroot As String
    Public Property App As String

    Public Shared ReadOnly Property DefaultFile As String = HOME & "/httpd.json"

    Public Shared Function LoadDefault() As Configs
        Try
            Return JsonContract.LoadJsonFile(Of Configs)(DefaultFile)
        Catch ex As Exception
            ex = New Exception(DefaultFile, ex)
            Call LogException(ex)
            Dim __new As New Configs With {
                .Portal = 80,
                .WWWroot = HOME & "/wwwroot/"
            }
            Call __new.Save()
            Return __new
        End Try
    End Function

    Sub Save()
        Call Me.GetJson.SaveTo(DefaultFile)
    End Sub
End Class
