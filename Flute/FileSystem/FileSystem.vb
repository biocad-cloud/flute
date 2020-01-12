Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

''' <summary>
''' Physical file system combine with logical file mapping 
''' </summary>
Public Class FileSystem

    Public Property wwwroot As Directory

    ReadOnly virtualMaps As New Dictionary(Of String, FileObject)

    ''' <summary>
    ''' Create a new filesystem proxy for http web services
    ''' </summary>
    ''' <param name="wwwroot"></param>
    Sub New(wwwroot As String)
        Me.wwwroot = New Directory(directory:=wwwroot)
    End Sub

    ''' <summary>
    ''' 这个函数只适用于小文件的缓存
    ''' </summary>
    ''' <param name="resourceUrl$"></param>
    ''' <param name="file$"></param>
    ''' <param name="mime"></param>
    ''' <returns></returns>
    Public Function AddCache(resourceUrl$, file$, Optional mime As ContentType = Nothing) As FileObject
        Return AddCache(resourceUrl, file.ReadBinary, mime)
    End Function

    Public Function AddCache(resourceUrl$, data As Byte(), Optional mime As ContentType = Nothing) As FileObject
        Dim resource As New MemoryCachedFile(resourceUrl.FileName, data, mime)
        Dim key$ = FileSystem.resourceUrl(resourceUrl)

        ' add new cache resource or update current 
        ' existed resource
        virtualMaps(key) = resource

        Return resource
    End Function

    Private Shared Function resourceUrl(ByRef pathRelative As String) As String
        pathRelative = pathRelative.Trim("."c, "/"c, "\"c)
        Return pathRelative
    End Function

    Public Function FileExists(pathRelative As String) As Boolean
        ' test of the physical file at first
        If resourceUrl(pathRelative).FileExists Then
            Return True
        Else
            ' and then test for the logical file
            If virtualMaps.ContainsKey(pathRelative) Then
                If TypeOf virtualMaps(pathRelative) Is VirtualMappedFile Then
                    Return DirectCast(virtualMaps(pathRelative), VirtualMappedFile).isValid
                Else
                    Return True
                End If
            End If
        End If

        Return False
    End Function
End Class
