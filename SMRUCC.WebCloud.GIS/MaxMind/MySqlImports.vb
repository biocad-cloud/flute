Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.Linq
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL
Imports SMRUCC.WebCloud.GIS.MaxMind.geolite2

Namespace MaxMind

    Public Module MySqlImports

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="mysql"></param>
        ''' <param name="df">GeoLite2-Country-Blocks-IPv4.csv</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ImportsGeoLite2CountryBlocksIPv4(mysql As MySQL, df As String) As Boolean
            Dim data As geolite2_country_blocks_ipv4() = df.LoadCsv(Of geolite2_country_blocks_ipv4)
            Dim SQL As New Value(Of String)

            If data.IsNullOrEmpty Then
                Return False
            End If

            Call mysql.Execute(DropTableSQL(Of geolite2_country_blocks_ipv4))
            If Not String.IsNullOrEmpty(SQL = GetCreateTableMetaSQL(Of geolite2_country_blocks_ipv4)()) Then
                Call mysql.Execute(SQL)
            End If

            For Each x As geolite2_country_blocks_ipv4 In data
                Call mysql.ExecInsert(x)
            Next

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="mysql"></param>
        ''' <param name="df">GeoLite2-Country-Blocks-IPv6.csv</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ImportsGeoLite2CountryBlocksIPv6(mysql As MySQL, df As String) As Boolean
            Dim SQL As New Value(Of String)

            Using reader As New DataStream(df,, 1024 * 1024 * 10)
                Call mysql.Execute(DropTableSQL(Of geolite2_country_blocks_ipv6))
                If Not String.IsNullOrEmpty(SQL = GetCreateTableMetaSQL(Of geolite2_country_blocks_ipv6)()) Then
                    Call mysql.Execute(SQL)
                End If

                Call reader.ForEach(Of geolite2_country_blocks_ipv6)(AddressOf mysql.ExecInsert)
            End Using

            Return True
        End Function

        <Extension>
        Public Function ImportsGeoLite2CountryLocations(mysql As MySQL, DIR As String) As Boolean
            Dim SQL As New Value(Of String)

            Call mysql.Execute(DropTableSQL(Of geolite2_country_locations))
            If Not String.IsNullOrEmpty(SQL = GetCreateTableMetaSQL(Of geolite2_country_locations)()) Then
                Call mysql.Execute(SQL)
            End If

            For Each df As String In ls - l - r - wildcards("GeoLite2-Country-Locations*.csv") <= DIR
                Dim data = df.LoadCsv(Of geolite2_country_locations)
                Dim trans As String = String.Join(vbLf, data.ToArray(Function(x) x.GetInsertSQL))
                Call mysql.CommitTransaction(trans)
            Next

            Return True
        End Function
    End Module
End Namespace