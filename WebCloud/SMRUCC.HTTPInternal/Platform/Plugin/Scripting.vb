Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace Platform.Plugins

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class ScriptingAttribute : Inherits Attribute

        Public ReadOnly Property FileTypes As String()

        Public Delegate Function ScriptHandler(wwwroot$, path$, encoding As Encodings) As String

        ''' <summary>
        ''' ``*.vbhtml`` etc.
        ''' </summary>
        ''' <param name="extensions$"></param>
        Sub New(ParamArray extensions$())
            If extensions.IsNullOrEmpty Then
                Throw New ArgumentNullException("No file type supports!")
            Else
                FileTypes = extensions
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return FileTypes.GetJson
        End Function
    End Class

    Public Module ScriptingExtensions

    End Module
End Namespace