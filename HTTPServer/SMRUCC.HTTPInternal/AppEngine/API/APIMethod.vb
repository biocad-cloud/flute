Imports System.Drawing
Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq

Namespace AppEngine.APIMethods

    ''' <summary>
    ''' In plain English, that means that GET is used for viewing something, without changing it, 
    ''' while POST is used for changing something. 
    ''' For example, a search page should use GET, while a form that changes your password should use POST.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public MustInherit Class APIMethod : Inherits Attribute

        Public ReadOnly Property Response As Type

        Sub New(responseExample As Type)
            Me.Response = responseExample
        End Sub

        Public Function GetExample() As String
            Dim description As String = APIMethod.Description(Response)

            If description.LastIndexOf(vbCr) = -1 Then
                Return description
            End If

            Dim example As String = "{" & vbCrLf
            example &= description & vbCrLf
            example &= "}"
            Return example
        End Function

        Public MustOverride Function GetMethodHelp(EntryPoint As System.Reflection.MethodInfo) As String

        Protected Function __getParameters(EntryPoint As System.Reflection.MethodInfo) As String
            Dim attrs As Object() = EntryPoint.GetCustomAttributes(GetType(Microsoft.VisualBasic.CommandLine.Reflection.ParameterInfo), True)
            If attrs.IsNullOrEmpty Then
                Return ""
            Else
                Dim sbr As StringBuilder = New StringBuilder("<strong>Parameters:</strong><br /><table>")

                For Each param In attrs.ToArray(Function(value) DirectCast(value, Microsoft.VisualBasic.CommandLine.Reflection.ParameterInfo))
                    Call sbr.AppendLine($"  <tr>
    <td>{param.Name}</td>
<td>{If(param.Optional, "<i>Optional</i>", "")}</td>
    <td>{param.Description}</td>
  </tr>
")
                Next
                Call sbr.AppendLine("</table>")

                Return sbr.ToString
            End If
        End Function

        Private Shared Function __isValueType(typeDef As Type) As Boolean
            If Not typeDef.IsClass OrElse
            typeDef.Equals(GetType(String)) OrElse
            typeDef.Equals(GetType(Image)) OrElse
            String.Equals("System.Collections.Generic", typeDef.Namespace) OrElse
            typeDef.IsInheritsFrom(GetType(System.Array)) Then

                Return True
            Else
                Return False
            End If
        End Function

        Private Shared Function __getFullName(typeDef As Type) As String
            If String.Equals("System.Collections.Generic", typeDef.Namespace) Then

                Dim generics = typeDef.GetGenericArguments
                Return $"{typeDef.Namespace}.{typeDef.Name}(Of {String.Join(", ", generics.ToArray(Function(type) __getFullName(type)))})"

            Else
                Return typeDef.FullName
            End If
        End Function

        Public Shared Function Description(typeDef As Type) As String
            If __isValueType(typeDef) Then
                Return __getFullName(typeDef)
            End If
            Return __description(typeDef, indent:="  ").Value
        End Function

        ''' <summary>
        ''' 递归的直到找到最简单的数据类型
        ''' </summary>
        ''' <param name="typeDef"></param>
        ''' <returns></returns>
        Private Shared Function __description(typeDef As Type, indent As String) As KeyValuePair(Of Boolean, String)
            If __isValueType(typeDef) Then
                Return New KeyValuePair(Of Boolean, String)(False, __getFullName(typeDef))
            End If

            Dim sbr As StringBuilder = New StringBuilder
            Dim props As PropertyInfo() = typeDef.GetProperties
            Dim values = (From prop As PropertyInfo In props
                          Select prop.PropertyType,
                          prop.Name,
                          descr = EmitReflection.Description(prop).TrimVBCrLf,
                          innerClass = __description(prop.PropertyType, indent & "  ")).ToArray

            For Each prop In values
                If prop.innerClass.Key Then
                    Call sbr.AppendLine(indent & "// TypeOf @" & __getFullName(prop.PropertyType))
                    Call sbr.Append(indent & $"""{prop.Name}"":")
                    Call sbr.AppendLine(" {    // " & prop.descr)
                    Call sbr.AppendLine(prop.innerClass.Value)
                    Call sbr.AppendLine(indent & "},")
                Else
                    Call sbr.Append(indent & $"""{prop.Name}"":")
                    Call sbr.Append($" ""{prop.innerClass.Value}""")

                    If Not String.IsNullOrEmpty(prop.descr) Then
                        Call sbr.AppendLine(",  // " & prop.descr)
                    Else
                        Call sbr.AppendLine(",")
                    End If
                End If
            Next

            Return New KeyValuePair(Of Boolean, String)(True, sbr.ToString)
        End Function
    End Class
End Namespace