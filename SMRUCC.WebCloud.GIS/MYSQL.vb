Imports System.Text
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions

Public Module MYSQL

    Public Function LoadCsv(path As String) As Tables.GeoIP2.IPv4.geolite2_city_blocks_ipv4()
        Return path.LoadCsv(Of Tables.GeoIP2.IPv4.geolite2_city_blocks_ipv4)(False).ToArray
    End Function

    Public Function LoadCsvCityLocations(path As String) As Tables.GeoIP2.IPv4.geolite2_city_locations()
        Return path.LoadCsv(Of Tables.GeoIP2.IPv4.geolite2_city_locations)(False).ToArray
    End Function

    Public Function GenerateInsertSQL(data As Tables.GeoIP2.IPv4.geolite2_city_locations()) As String
        Return GenerateInsertSQLInternal(data)
    End Function

    ''' <summary>
    ''' 生成用于导入数据库的SQL插入脚本
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function GenerateInsertSQL(data As Tables.GeoIP2.IPv4.geolite2_city_blocks_ipv4()) As String
        Return GenerateInsertSQLInternal(data)
    End Function

    Private Function GenerateInsertSQLInternal(data As Generic.IEnumerable(Of Oracle.LinuxCompatibility.MySQL.Client.SQLTable)) As String
        Dim LQuery = (From entry In data.AsParallel Select entry.GetInsertSQL).ToArray
        Dim sBuilder As StringBuilder = New StringBuilder(2 * 2048)
        For Each Line As String In LQuery
            Call sBuilder.AppendLine(Line)
        Next

        Return sBuilder.ToString
    End Function
End Module
