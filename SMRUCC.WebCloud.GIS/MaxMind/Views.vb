Imports System.Net

Namespace MaxMind.Views

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