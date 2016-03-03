Namespace Platform

    Public MustInherit Class PlatformSub

        Public ReadOnly Property PlatformEngine As PlatformEngine

        Sub New(main As PlatformEngine)
            PlatformEngine = main
        End Sub
    End Class
End Namespace