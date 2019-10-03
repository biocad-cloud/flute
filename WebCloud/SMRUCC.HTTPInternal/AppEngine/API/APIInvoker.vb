#Region "Microsoft.VisualBasic::db303801ba67233f659a4c04adb23d9a, WebCloud\SMRUCC.HTTPInternal\AppEngine\API\APIInvoker.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class APIInvoker
    ' 
    '         Properties: EntryPoint, Help, Method, Name
    ' 
    '         Function: __handleERROR, __invoke, __invokePOST, Fakes, Invoke
    '                   InvokePOST, ToString, VirtualPath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.WebCloud.HTTPInternal.Core

Namespace AppEngine.APIMethods

    Public Class APIInvoker : Implements INamedValue

        Public Property Name As String Implements INamedValue.Key
        Public Property EntryPoint As MethodInfo
        Public Property Help As String
        Public Property Method As APIMethod

        Public Overrides Function ToString() As String
            Return Name
        End Function

        <POST(GetType(Boolean))>
        Public Function InvokePOST(App As Object, request As HttpPOSTRequest, response As HttpResponse) As Boolean
            Try
                Return __invokePOST(App, request, response)
            Catch ex As Exception
                Return __handleERROR(ex, request.URL, response)
            End Try
        End Function

        ''' <summary>
        ''' 在API的函数调用的位置，就只需要有args这一个参数
        ''' </summary>
        ''' <returns></returns>
        <[GET](GetType(Boolean))>
        Public Function Invoke(App As Object, request As HttpRequest, response As HttpResponse) As Boolean
            Try
                Return __invoke(App, request, response)
            Catch ex As Exception
                Return __handleERROR(ex, request.URL, response)
            End Try
        End Function

        Private Function __handleERROR(ex As Exception, url As String, ByRef response As HttpResponse) As Boolean
            Dim result As String
            ex = New Exception("Request page: " & url, ex)

#If DEBUG Then
            result = ex.ToString
#Else
            result = APIMethods.Fakes(ex)
#End If
            Call App.LogException(ex)
            Call ex.PrintException
            Call response.WriteError(500, result)

            Return False
        End Function

        Private Function __invokePOST(App As Object, request As HttpPOSTRequest, response As HttpResponse) As Boolean
            Dim value As Object = EntryPoint.Invoke(App, {request, response})
            Return DirectCast(value, Boolean)
        End Function

        Private Function __invoke(App As Object, request As HttpRequest, response As HttpResponse) As Boolean
            Dim value As Object = EntryPoint.Invoke(App, {request, response})
            Return DirectCast(value, Boolean)
        End Function
    End Class
End Namespace
