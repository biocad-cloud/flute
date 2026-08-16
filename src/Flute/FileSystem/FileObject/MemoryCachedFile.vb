Imports System.IO
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Namespace FileSystem

    Public Class MemoryCachedFile : Inherits FileObject

        ReadOnly cache As MemoryStream

        Public Overrides ReadOnly Property ContentLength As Long
            Get
                Return cache.Length
            End Get
        End Property

        Sub New(fileName$, data As Byte(), Optional mime As ContentType = Nothing)
            Call MyBase.New(fileName, mime)

            ' create cache data stream
            Me.cache = New MemoryStream(data)
        End Sub

        Public Overrides Function GetResource() As Stream
            Return cache
        End Function

        Public Overrides Function GetByteBuffer() As Byte()
            Return cache.ToArray
        End Function
    End Class
End Namespace