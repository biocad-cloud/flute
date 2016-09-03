Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports System.Net
Imports SMRUCC.WebCloud.GIS.MaxMind.geolite2
Imports SMRUCC.WebCloud.GIS.MaxMind.Views

Namespace MaxMind

    ''' <summary>
    ''' XML数据库查询
    ''' </summary>
    <Xml.Serialization.XmlRoot("geoip", Namespace:="http://www.siyu.com/API/dev.maxmind.com/geoip/")>
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
            Dim GeoBlocksCsv = New Task(Of String, geolite2_city_blocks_ipv4())(GeoLite2_City_Blocks,
                                                                   Handle:=Function(Path As String) Path.LoadCsv(Of geolite2_city_blocks_ipv4)(False).ToArray)
            Call Console.WriteLine("Start load CityLocations...")
            Dim CityLocations As geolite2_city_locations() = GeoLite2_City_Locations.LoadCsv(Of geolite2_city_locations)(False).ToArray
            Call Console.WriteLine("Start load GeoBlocks...")
            Dim GeoBlocks = (From Location In GeoBlocksCsv.GetValue.AsParallel Select Location Group By Location.geoname_id Into Group).ToArray.ToDictionary(Function(Location) Location.geoname_id, elementSelector:=Function(data) data.Group.ToArray)
            Call Console.WriteLine("Query country cities...")
            Dim CountryCities = (From City In CityLocations.AsParallel Select City Group By City.country_iso_code Into Group).ToArray
            Call Console.WriteLine("Generate countries data....")
            Dim CountryLQuery = (From Country In CountryCities.AsParallel Select InternalGenerateCountry(Country.Group.ToArray, GeoBlocks)).ToArray
            Call Console.WriteLine("Xml database compile job done!")
            Return New Database With {.Countries = CountryLQuery}
        End Function

        Private Shared Function InternalGenerateCountry(Cities As geolite2_city_locations(), GeoBlocks As Dictionary(Of Long, geolite2_city_blocks_ipv4())) As Views.Country
            Dim CountryData = Cities.First
            Dim Country As New Views.Country With {.continent_code = CountryData.continent_code, .continent_name = CountryData.continent_name, .country_iso_code = CountryData.country_iso_code, .country_name = CountryData.country_name}
            Country.CityLocations = (From City In Cities
                                     Let GeoBlocksData = If(GeoBlocks.ContainsKey(City.geoname_id), GeoBlocks(City.geoname_id), New geolite2_city_blocks_ipv4() {})
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

        Private Shared Function InternalGenerateGeographicLocation(data As geolite2_city_blocks_ipv4()) As Views.GeographicLocation()
            Dim LQuery = (From Location As geolite2_city_blocks_ipv4
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
End Namespace