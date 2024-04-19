Imports Flute.Http.Core.Message

Public Class SessionManager : Inherits ServerComponent

    Public ReadOnly Property Id As String

    Sub New(cookies As Cookies, settings As Configuration)
        Call MyBase.New(settings)
    End Sub

End Class
