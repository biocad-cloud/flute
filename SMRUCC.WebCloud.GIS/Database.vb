Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports System.Net

''' <summary>
''' XML数据库查询
''' </summary>
<Xml.Serialization.XmlRoot("geoip", namespace:="http://www.siyu.com/API/dev.maxmind.com/geoip/")>
Public Class Database

    <Xml.Serialization.XmlElement> Public Property Countries As Views.Country()

    Public Function FindLocation(s_IPAddress As String) As FindResult
        Dim IPAddress As IPAddress = IPAddress.Parse(s_IPAddress)
        Dim LQuery = (From Country In Countries.AsParallel
                      Let FindCity = (From City As Views.CityLocation
                                      In Country.CityLocations
                                      Let Location = City.IPLocatedAtCity(IPAddress)
                                      Where Not Location Is Nothing
                                      Select City, Location).ToArray Where Not FindCity.IsNullOrEmpty Select FoundCity = FindCity.First, Country).ToArray
        If LQuery.IsNullOrEmpty Then
            Return Nothing
        End If

        Dim FoundResult = LQuery.First
        Return New FindResult With
               {
                   .CIDR = FoundResult.FoundCity.Location.CIDR,
                   .city_name = FoundResult.FoundCity.City.city_name,
                   .continent_code = FoundResult.Country.continent_code,
                   .continent_name = FoundResult.Country.continent_name,
                   .country_iso_code = FoundResult.Country.country_iso_code,
                   .country_name = FoundResult.Country.country_name,
                   .geoname_id = FoundResult.FoundCity.City.geoname_id,
                   .latitude = FoundResult.FoundCity.Location.latitude,
                   .longitude = FoundResult.FoundCity.Location.longitude,
                   .metro_code = FoundResult.FoundCity.City.metro_code,
                   .postal_code = FoundResult.FoundCity.Location.postal_code,
                   .subdivision_1_iso_code = FoundResult.FoundCity.City.subdivision_1_iso_code,
                   .subdivision_1_name = FoundResult.FoundCity.City.subdivision_1_name,
                   .subdivision_2_iso_code = FoundResult.FoundCity.City.subdivision_2_iso_code,
                   .subdivision_2_name = FoundResult.FoundCity.City.subdivision_2_name,
                   .time_zone = FoundResult.FoundCity.City.time_zone}
        '.is_anonymous_proxy = FoundResult.FoundCity.Location.is_anonymous_proxy,
        '.is_satellite_provider = FoundResult.FoundCity.Location.is_satellite_provider,
    End Function

    Public Class FindResult : Inherits GeographicLocation

        <Xml.Serialization.XmlAttribute> Public Property continent_code As String
        <Xml.Serialization.XmlAttribute> Public Property continent_name As String
        <Xml.Serialization.XmlAttribute> Public Property country_iso_code As String
        <Xml.Serialization.XmlAttribute> Public Property country_name As String
        <Xml.Serialization.XmlAttribute> Public Property geoname_id As Long
        <Xml.Serialization.XmlAttribute> Public Property subdivision_1_iso_code As String
        <Xml.Serialization.XmlAttribute> Public Property subdivision_1_name As String
        <Xml.Serialization.XmlAttribute> Public Property subdivision_2_iso_code As String
        <Xml.Serialization.XmlAttribute> Public Property subdivision_2_name As String
        <Xml.Serialization.XmlAttribute> Public Property city_name As String
        <Xml.Serialization.XmlAttribute> Public Property metro_code As String
        <Xml.Serialization.XmlAttribute> Public Property time_zone As String

        Public Overrides Function ToString() As String
            Return String.Format("{0}, {1}/{2}", city_name, subdivision_1_name, subdivision_2_name) & ";      " & String.Format("({0}){1}", country_iso_code, country_name) & "      [" & MyBase.ToString() & "]"
        End Function

        Public Shared Function Null() As FindResult
            Return New FindResult With {.city_name = "null", .country_name = "null", .subdivision_1_name = "null", .subdivision_2_name = "null"}
        End Function
    End Class

    ''' <summary>
    ''' 都按照国家的代码进行分组
    ''' </summary>
    ''' <param name="GeoLite2_City_Blocks">GeoLite2-City-Blocks-IPv4.csv  161MB</param>
    ''' <param name="GeoLite2_City_Locations">GeoLite2-City-Locations-en.csv</param>
    ''' <param name="GeoLite2_Country_Blocks">GeoLite2-Country-Blocks-IPv4.csv</param>
    ''' <param name="GeoLite2_Country_Locations">GeoLite2-Country-Locations-en.csv</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function LoadFromCsv(GeoLite2_City_Blocks As String,
                                       GeoLite2_City_Locations As String,
                                       GeoLite2_Country_Blocks As String,
                                       GeoLite2_Country_Locations As String) As Database
        Call Console.WriteLine("Start GeoBlocksCsv task...")
        Dim GeoBlocksCsv = New Microsoft.VisualBasic.Parallel.Task(Of String, Tables.GeoIP2.IPv4.geolite2_city_blocks_ipv4())(GeoLite2_City_Blocks,
                                                                                                              Handle:=Function(Path As String) Path.LoadCsv(Of Tables.GeoIP2.IPv4.geolite2_city_blocks_ipv4)(False).ToArray)
        Call Console.WriteLine("Start load CityLocations...")
        Dim CityLocations As Tables.GeoIP2.IPv4.geolite2_city_locations() = GeoLite2_City_Locations.LoadCsv(Of Tables.GeoIP2.IPv4.geolite2_city_locations)(False).ToArray
        Call Console.WriteLine("Start load GeoBlocks...")
        Dim GeoBlocks = (From Location In GeoBlocksCsv.GetValue.AsParallel Select Location Group By Location.geoname_id Into Group).ToArray.ToDictionary(Function(Location) Location.geoname_id, elementSelector:=Function(data) data.Group.ToArray)
        Call Console.WriteLine("Query country cities...")
        Dim CountryCities = (From City In CityLocations.AsParallel Select City Group By City.country_iso_code Into Group).ToArray
        Call Console.WriteLine("Generate countries data....")
        Dim CountryLQuery = (From Country In CountryCities.AsParallel Select InternalGenerateCountry(Country.Group.ToArray, GeoBlocks)).ToArray
        Call Console.WriteLine("Xml database compile job done!")
        Return New Database With {.Countries = CountryLQuery}
    End Function

    Private Shared Function InternalGenerateCountry(Cities As Tables.GeoIP2.IPv4.geolite2_city_locations(), GeoBlocks As Dictionary(Of Long, Tables.GeoIP2.IPv4.geolite2_city_blocks_ipv4())) As Views.Country
        Dim CountryData = Cities.First
        Dim Country As New Views.Country With {.continent_code = CountryData.continent_code, .continent_name = CountryData.continent_name, .country_iso_code = CountryData.country_iso_code, .country_name = CountryData.country_name}
        Country.CityLocations = (From City In Cities
                                 Let GeoBlocksData = If(GeoBlocks.ContainsKey(City.geoname_id), GeoBlocks(City.geoname_id), New Tables.GeoIP2.IPv4.geolite2_city_blocks_ipv4() {})
                                 Select New Views.CityLocation With
                                        {
                                            .GeographicLocations = InternalGenerateGeographicLocation(GeoBlocksData),
                                            .city_name = City.city_name,
                                            .geoname_id = City.geoname_id,
                                            .metro_code = City.metro_code,
                                            .subdivision_1_iso_code = City.subdivision_1_iso_code,
                                            .subdivision_1_name = City.subdivision_1_name,
                                            .subdivision_2_iso_code = City.subdivision_2_iso_code,
                                            .subdivision_2_name = City.subdivision_2_name,
                                            .time_zone = City.time_zone}).ToArray
        Return Country
    End Function

    Private Shared Function InternalGenerateGeographicLocation(data As Tables.GeoIP2.IPv4.geolite2_city_blocks_ipv4()) As Views.GeographicLocation()
        Dim LQuery = (From Location As Tables.GeoIP2.IPv4.geolite2_city_blocks_ipv4
                      In data
                      Select New Views.GeographicLocation With
                             {
                                 .latitude = Location.latitude,
                                 .longitude = Location.longitude,
                                 .CIDR = Location.network,
                                 .postal_code = Location.postal_code}).ToArray
        '.is_anonymous_proxy = Location.is_anonymous_proxy,
        '.is_satellite_provider = Location.is_satellite_provider,
        Return LQuery
    End Function
End Class

Namespace Views

    Public Class Country
        <Xml.Serialization.XmlAttribute> Public Property continent_code As String
        <Xml.Serialization.XmlAttribute> Public Property continent_name As String
        <Xml.Serialization.XmlAttribute> Public Property country_iso_code As String
        <Xml.Serialization.XmlAttribute> Public Property country_name As String
        <Xml.Serialization.XmlElement("city")> Public Property CityLocations As CityLocation()

        Public Overrides Function ToString() As String
            Return String.Format("({0}){1}", country_iso_code, country_name)
        End Function
    End Class

    Public Class CityLocation
        <Xml.Serialization.XmlAttribute> Public Property geoname_id As Long
        <Xml.Serialization.XmlAttribute> Public Property subdivision_1_iso_code As String
        <Xml.Serialization.XmlAttribute> Public Property subdivision_1_name As String
        <Xml.Serialization.XmlAttribute> Public Property subdivision_2_iso_code As String
        <Xml.Serialization.XmlAttribute> Public Property subdivision_2_name As String
        <Xml.Serialization.XmlAttribute> Public Property city_name As String
        <Xml.Serialization.XmlAttribute> Public Property metro_code As String
        <Xml.Serialization.XmlAttribute> Public Property time_zone As String
        <Xml.Serialization.XmlElement("geoLoci")> Public Property GeographicLocations As GeographicLocation()

        Public Function IPLocatedAtCity(IPAddress As Net.IPAddress) As GeographicLocation
            If GeographicLocations.IsNullOrEmpty Then
                Return Nothing
            End If
            Dim LQuery = (From Location As GeographicLocation
                          In Me.GeographicLocations
                          Where Not Location Is Nothing AndAlso Location.Locating(IPAddress)
                          Select Location).ToArray

            Return LQuery.FirstOrDefault
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("{0}, {1}/{2}", city_name, subdivision_1_name, subdivision_2_name)
        End Function
    End Class

    Public Class GeographicLocation

        Dim _InternalCIDRValue As CIDR

        <Xml.Serialization.XmlAttribute("network")> Public Property CIDR As String
            Get
                If _InternalCIDRValue Is Nothing Then
                    Return ""
                End If
                Return _InternalCIDRValue.CIDR
            End Get
            Set(value As String)
                If String.IsNullOrEmpty(value) Then
                    _InternalCIDRValue = Nothing
                Else
                    _InternalCIDRValue = New CIDR(value)
                End If
            End Set
        End Property
        '<Xml.Serialization.XmlAttribute> Public Property is_anonymous_proxy As Integer
        '<Xml.Serialization.XmlAttribute> Public Property is_satellite_provider As Integer
        <Xml.Serialization.XmlAttribute> Public Property postal_code As String
        <Xml.Serialization.XmlAttribute> Public Property latitude As Double
        <Xml.Serialization.XmlAttribute> Public Property longitude As Double

        Public Overrides Function ToString() As String
            Return String.Format("[{0}, {1}]  {2}", latitude, longitude, CIDR)
        End Function

        ''' <summary>
        ''' 查看目标IP地址是否落于本地址段之内
        ''' </summary>
        ''' <param name="IP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Locating(IP As Net.IPAddress) As Boolean
            If Me._InternalCIDRValue Is Nothing OrElse _InternalCIDRValue.Invalid Then
                Return False
            End If
            Return Me._InternalCIDRValue.Locating(IP)
        End Function

    End Class

    Public Class CIDR

        Dim StartIP As IPAddress
        Dim EndIP As IPAddress
        Dim _InternalMaskBytes As Byte()
        Dim _CIDR_Mask As String

        Public ReadOnly Property CIDR As String
            Get
                Return _CIDR_Mask
            End Get
        End Property

        Shared ReadOnly _InternalZEROIP As IPAddress = IPAddress.Parse("0.0.0.0")
        Shared ReadOnly _Internal255IP As IPAddress = IPAddress.Parse("255.255.255.255")

        Public ReadOnly Property Invalid As Boolean
            Get
                Return StartIP.Equals(_InternalZEROIP) OrElse EndIP.Equals(_Internal255IP)
            End Get
        End Property

        Sub New(CIDR_Mask As String)
            Dim CIDR As String() = CIDR_Mask.Split("/"c)

            If CIDR.Length <> 2 Then Throw New Exception(String.Format("Target address value ""{0}"" is not a valid CIDR mask IPAddress value!", CIDR_Mask))

            'obviously some additional error checking would be delightful
            Dim ip As IPAddress = IPAddress.Parse(CIDR(0))
            Dim bits As Integer = CInt(CIDR(1))
            Dim mask As UInteger = Not (UInteger.MaxValue >> bits)
            Dim ipBytes() As Byte = ip.GetAddressBytes()
            _InternalMaskBytes = BitConverter.GetBytes(mask).Reverse().ToArray()
            Dim startIPBytes(ipBytes.Length - 1) As Byte
            Dim endIPBytes(ipBytes.Length - 1) As Byte

            For i As Integer = 0 To ipBytes.Length - 1
                startIPBytes(i) = CByte(ipBytes(i) And _InternalMaskBytes(i))
                endIPBytes(i) = CByte(ipBytes(i) Or (Not _InternalMaskBytes(i)))
            Next i

            ' You can remove first and last (Network and Broadcast) here if desired

            StartIP = New IPAddress(startIPBytes)
            EndIP = New IPAddress(endIPBytes)
            _CIDR_Mask = CIDR_Mask

            If Me.Invalid Then
                'Call Console.WriteLine(CIDR_Mask & " is not valid!")
                'StartIP = ip
            End If
        End Sub

        ''' <summary>
        ''' 查看目标IP地址是否落于本地址段之内
        ''' </summary>
        ''' <param name="IP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Locating(IP As String) As Boolean
            Return Locating(Net.IPAddress.Parse(IP))
        End Function

        ''' <summary>
        ''' 查看目标IP地址是否落于本地址段之内
        ''' </summary>
        ''' <param name="IP"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Locating(IP As Net.IPAddress) As Boolean
            Dim Tokens = IP.GetAddressBytes
            Dim Start = StartIP.GetAddressBytes
            Dim Ends = EndIP.GetAddressBytes

            For i As Integer = 0 To Tokens.Count - 1
                Dim [byte] As Byte = Tokens(i)
                If Not ([byte] >= Start(i) AndAlso [byte] <= Ends(i)) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Overrides Function ToString() As String
            Return StartIP.ToString & " - " & EndIP.ToString & "   [subnet_mask = " &
                _InternalMaskBytes(0).ToString & "." & _InternalMaskBytes(1).ToString & "." & _InternalMaskBytes(2).ToString & "." & _InternalMaskBytes(3).ToString & "]"
        End Function
    End Class
End Namespace