Namespace Core.Cache

    Public Class VirtualFileSystem

        ReadOnly files As New Dictionary(Of String, CachedFile)
        ''' <summary>
        ''' The root of this in-memory virtual filesystem
        ''' </summary>
        ReadOnly fileTree As FileNode

    End Class
End Namespace