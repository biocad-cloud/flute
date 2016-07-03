Imports Microsoft.VisualBasic.Language

Namespace Platform

    ''' <summary>
    ''' Web App engine platform components.
    ''' </summary>
    Public MustInherit Class PlatformSub : Inherits ClassObject

        ''' <summary>
        ''' Platform engine parent host
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PlatformEngine As PlatformEngine

        Sub New(main As PlatformEngine)
            PlatformEngine = main
        End Sub
    End Class
End Namespace