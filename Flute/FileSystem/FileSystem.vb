Imports Microsoft.VisualBasic.FileIO

''' <summary>
''' Physical file system combine with logical file mapping 
''' </summary>
Public Class FileSystem

    Public Property wwwroot As Directory

    ''' <summary>
    ''' Create a new filesystem proxy for http web services
    ''' </summary>
    ''' <param name="wwwroot"></param>
    Sub New(wwwroot As String)
        Me.wwwroot = New Directory(directory:=wwwroot)
    End Sub
End Class
