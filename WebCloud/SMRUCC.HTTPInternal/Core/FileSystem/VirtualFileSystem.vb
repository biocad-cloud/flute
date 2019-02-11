Imports System.IO
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.WebCloud.HTTPInternal.Platform.Plugins
Imports SMRUCC.WebCloud.HTTPInternal.Core.Cache
Imports fs = Microsoft.VisualBasic.FileIO.FileSystem

Namespace Core.Cache

    Public Class VirtualFileSystem

#Region "两种数据组织的方式"
        ReadOnly files As New Dictionary(Of String, CachedFile)
        ''' <summary>
        ''' The root of this in-memory virtual filesystem
        ''' </summary>
        ReadOnly fileTree As FileNode
#End Region

        ReadOnly _cacheUpdate As UpdateThread

        Sub New()
            _cacheUpdate = New UpdateThread(1000 * 60 * 30,
                  Sub()
                      For Each file In CachedFile.CacheAllFiles(wwwroot.FullName)
                          _cache(file.Key) = file.Value
                      Next
                  End Sub)
            _cacheUpdate.Start()

            Call "Running in file system cache mode!".__DEBUG_ECHO
        End Sub
    End Class
End Namespace