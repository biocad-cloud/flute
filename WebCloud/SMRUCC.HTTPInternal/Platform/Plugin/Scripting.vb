Imports System.Runtime.CompilerServices
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

        ReadOnly scripting As IReadOnlyDictionary(Of String, ScriptingAttribute.ScriptHandler)

        Sub New()
            scripting = App.HOME.LoadHandlers
        End Sub

        <Extension>
        Public Function LoadHandlers(dir$) As Dictionary(Of String, ScriptingAttribute.ScriptHandler)

        End Function

        <Extension>
        Public Function ReadHTML(wwwroot$, path$, Optional encoding As Encodings = Encodings.UTF8) As String

        End Function
    End Module
End Namespace