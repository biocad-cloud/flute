Imports System.IO
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Namespace FileSystem

    Public Class VirtualMappedFile : Inherits FileObject

        Public ReadOnly Property mappedPath As String

        Public ReadOnly Property isValid As Boolean
            Get
                Return mappedPath.FileExists
            End Get
        End Property

        Public Overrides ReadOnly Property ContentLength As Long
            Get
                Return mappedPath.FileLength
            End Get
        End Property

        Sub New(fileName$, mappedPath$, Optional mime As ContentType = Nothing)
            Call MyBase.New(fileName, mime)

            Me.mappedPath = mappedPath
        End Sub

        Public Overrides Function GetResource() As Stream
            Return mappedPath.Open(FileMode.Open, doClear:=False)
        End Function

        Public Overrides Function GetByteBuffer() As Byte()
            Return mappedPath.ReadBinary
        End Function
    End Class
End Namespace