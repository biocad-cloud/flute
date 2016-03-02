Imports SMRUCC.REST
Imports Microsoft.VisualBasic.IEnumerations
Imports System.IO

Namespace AppEngine

    Public Class ArgsMgr : Implements IEnumerable(Of Content)

        ReadOnly __innerList As New List(Of Content)

        Default Public ReadOnly Property Value(name As String) As Content
            Get
                Return __innerList.GetItem(name, False)
            End Get
        End Property

        Public Function GetContent(name As String) As String
            Dim value As Content = Me(name)
            If value Is Nothing Then
                Return ""
            Else
                Return value.content
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name">可能会出现重名的现象</param>
        ''' <returns></returns>
        Public Function GetFile(name As String) As Content
            Dim File = (From x As Content In __innerList
                        Where String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) AndAlso
                        Not String.IsNullOrEmpty(x.FileName)
                        Select x).FirstOrDefault
            Return File
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="params">POST得到的数据</param>
        Sub New(params As StreamReader)
            Call Me.New(Content.ContentParser(params.ReadToEnd))
        End Sub

        Sub New(params As IEnumerable(Of Content))
            If params Is Nothing Then
                __innerList = New List(Of Content)
            Else
                __innerList = params.ToList
            End If
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of Content) Implements IEnumerable(Of Content).GetEnumerator
            For Each x In __innerList
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace