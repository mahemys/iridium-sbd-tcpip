''TCPServer.vb
''created by mahemys
''Update : 07.11.2017 22:30 US
''Update : 06.11.2017 02:20 US

''Server_Socket >> Receive Data over TCP/IP
''Server_Socket >> Multiple Clients, Multithreading
''127.0.0.1     >> Same PC, Ethernet Disabled, No LAN
''192.168.0.101 >> Same PC, Ethernet Enabled

''No UI, simply runs in background...
''ASCII to UTF8 >> changed from ASCII to UTF8, get String, Unicode Characters
''TCPListnerLog >> changed from ASCII to UTF8, save file, Unicode Characters
''Binary        >> SBD TCP Data is Binary...

''No Messages, Only WriteLog...
''IntruderLog is seperate...
''Allow only Selected IP

''TCPIntrusionLog   >> Folder >> New file every Month
''TCPDataLogger     >> Folder >> New file every Day
''TCPDataDecode     >> Folder >> New file every Day

Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports System.Threading

Module TCPServer

    Sub Main()
        Dim Port As Integer = 443 ''TCP Port
        Dim serverSocket As New TcpListener(IPAddress.Any, Port)
        Dim clientSocket As TcpClient
        Dim counter As Integer
        Dim IntruderCount As Integer

        Dim aModuleName As String = Process.GetCurrentProcess.MainModule.ModuleName
        Dim aProcName As String = Path.GetFileNameWithoutExtension(aModuleName)
        If Process.GetProcessesByName(aProcName).Length > 1 Then
            '"Process is running"
            MsgBox(aProcName + " is already running...")
            Exit Sub
        Else
            '"Process is not running"
        End If

        Try
            serverSocket.Start()
            Dim IPHost As IPHostEntry
            IPHost = Dns.GetHostByName(Dns.GetHostName())
            'Msg("0" + vbTab + "0" + vbTab + "Server Started >> Server IP >> [" + IPHost.AddressList(0).ToString() + ":" + Port.ToString + "]")
        Catch ex As Exception
            Msg("0" + vbTab + "0" + vbTab + "Exception >> Unable to Start Server")
        End Try

        Try
            counter = 0

            ''New conter Every Day...
            Dim ProcessStartTime As DateTime
            Dim ProcessStartTimeString As String
            ProcessStartTimeString = Date.Now.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture)   '' Date.Now to String
            ProcessStartTime = Date.ParseExact(ProcessStartTimeString, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture)  ''String Parse to Specific format

            While (True)
                counter += 1
                clientSocket = serverSocket.AcceptTcpClient()

                Try
                    ''New conter Every Day...
                    Dim ProcessEndTime As DateTime
                    Dim ProcessEndTimeString As String = Date.Now.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture) '' Date.Now to String
                    ProcessEndTime = Date.ParseExact(ProcessEndTimeString, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture)  ''String Parse to Specific format
                    Dim ProcessTotalTime As TimeSpan
                    If ProcessEndTime > ProcessStartTime Then
                        ProcessTotalTime = ProcessEndTime - ProcessStartTime
                    Else
                        ProcessTotalTime = ProcessStartTime - ProcessEndTime
                    End If
                    If ProcessTotalTime.Days = 0 Then
                        ''same Day, continue...
                    Else
                        ''next Day, reset counter, reset StartDate
                        counter = 1
                        ProcessStartTimeString = Date.Now.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture)   '' Date.Now to String
                        ProcessStartTime = Date.ParseExact(ProcessStartTimeString, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture)  ''String Parse to Specific format
                    End If
                Catch ex As Exception
                End Try

                ''after accepting
                Dim ClientRemoteIPPort As String = clientSocket.Client.RemoteEndPoint.ToString 'IP:Port
                Dim ClientRemoteIP As String = ClientRemoteIPPort.Remove(ClientRemoteIPPort.IndexOf(":")) 'IP
                Dim ClientRemotePort As String = ClientRemoteIPPort.Substring(ClientRemoteIPPort.LastIndexOf(":") + 1) 'Port

                ''Accept Only Iridium, local IP
                If ClientRemoteIP = "12.47.179.11" OrElse ClientRemoteIP = "127.0.0.1" Then
                    'Msg(Convert.ToString(counter) + vbTab + "0" + vbTab + "Connected >> Client [" + Convert.ToString(counter) + "] >> IP [" + ClientRemoteIPPort + "]")
                    Dim client As New HandleClinet
                    client.startClient(clientSocket, Convert.ToString(counter))
                Else
                    ''close connection
                    IntruderCount += 1
                    WriteTCPIntrusionLogFile(Convert.ToString(counter) + vbTab + Convert.ToString(IntruderCount) + vbTab + "TCPIntrusion >> Intuder IP [" + ClientRemoteIPPort + "]")
                    clientSocket.Close()
                End If
            End While
        Catch ex As Exception
            Msg(Convert.ToString(counter) + vbTab + "0" + vbTab + "Exception >> Unable to Connect, Client [" + Convert.ToString(counter) + "]")
        End Try

        Try
            clientSocket.Close()
            serverSocket.Stop()
            'Msg("0" + vbTab + "0" + vbTab + "Exit >> Stop Server")
            'Console.ReadLine()
        Catch ex As Exception
            Msg("0" + vbTab + "0" + vbTab + "Exception >> Stop Server")
        End Try
    End Sub

    Public Class HandleClinet
        Dim clientSocket As TcpClient
        Dim clNo As String

        Public Sub startClient(ByVal inClientSocket As TcpClient, ByVal clinetNo As String)
            clientSocket = inClientSocket
            clNo = clinetNo
            Dim ctThread As Thread = New Thread(AddressOf MainWork)
            ctThread.SetApartmentState(ApartmentState.STA) 'important
            ctThread.Start()
        End Sub

        Private Sub MainWork()
            Dim requestCount As Integer
            Dim dataFromClient As String
            Dim sendBytes As [Byte]()
            Dim serverResponse As String
            requestCount = 0

            While (True)
                Try
                    requestCount = requestCount + 1
                    Dim networkStream As NetworkStream = clientSocket.GetStream()
                    Dim bytesFrom(10024) As Byte 'do not put outside
                    Dim thisRead As Integer = networkStream.Read(bytesFrom, 0, bytesFrom.Length)
                    ReDim Preserve bytesFrom(thisRead - 1)
                    If thisRead <> 0 Then
                        ''ASCII or UTF8
                        'dataFromClient = Encoding.ASCII.GetString(bytesFrom) 'ASCII
                        'dataFromClient = (dataFromClient.Replace(vbNullChar, "")).Trim
                        ''dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$")) 'No $ for SBD
                        'WriteTCPLogFile(clNo + vbTab + Convert.ToString(requestCount) + vbTab + thisRead.ToString + vbTab + dataFromClient)

                        ''Decimal
                        'Dim dataFromClient_Dec As String = Conv_ByteToDec(bytesFrom)
                        'WriteTCPLogFile(clNo + vbTab + Convert.ToString(requestCount) + vbTab + thisRead.ToString + vbTab + dataFromClient_Dec)

                        ''Binary >> SBD TCP Data is Binary...
                        Dim dataFromClient_Bin As String = Conv_ByteToBin(bytesFrom)
                        WriteTCPLogFile(clNo + vbTab + Convert.ToString(requestCount) + vbTab + thisRead.ToString + vbTab + dataFromClient_Bin)

                        BinaryData_SlNo = clNo
                        BinaryData = dataFromClient_Bin
                        Dim DecodeThread As Thread = New Thread(AddressOf BinaryData_Decode)
                        DecodeThread.SetApartmentState(ApartmentState.STA) 'important
                        DecodeThread.Start()

                        ''Hex
                        'Dim dataFromClient_Hex As String = Conv_ByteToHex(bytesFrom)
                        'WriteTCPLogFile(clNo + vbTab + Convert.ToString(requestCount) + vbTab + thisRead.ToString + vbTab + dataFromClient_Hex)

                        ''ServerResponse >> ASCII
                        'serverResponse = "Server response >> [" + clNo + "][" + Convert.ToString(requestCount) + "][" + thisRead.ToString + "]"
                        serverResponse = "OK"
                        sendBytes = Encoding.ASCII.GetBytes(serverResponse)
                        networkStream.Write(sendBytes, 0, sendBytes.Length)
                        networkStream.Flush()
                        'Msg(serverResponse)
                    Else
                        ''Empty/Closing
                        clientSocket.Close()
                        'WriteTCPLogFile(clNo + vbTab + Convert.ToString(requestCount) + vbTab + "Disconnected >> Client [" + clNo + "]")
                        Exit While
                    End If
                Catch ex As Exception
                    clientSocket.Close()
                    'WriteTCPLogFile(clNo + vbTab + Convert.ToString(requestCount) + vbTab + "Disconnected >> Client [" + clNo + "]")
                    Exit While
                End Try
            End While
            GC.Collect()
        End Sub
    End Class

    Dim BinaryData_SlNo As String
    Dim BinaryData As String
    Private Sub BinaryData_Decode()

        ''BinaryData >> Decode >> TCP_SBD_Decode.txt
        Dim BinaryData_Full As String = BinaryData
        Dim BinaryData_Split As String() = Enumerable.Range(0, BinaryData_Full.Length \ 8).[Select](Function(i) BinaryData_Full.Substring(i * 8, 8)).ToArray()
        Dim PayLoad_Present As Boolean

        If BinaryData_Split.Length = 34 Then
            ''PayLoad >> Not Present
            PayLoad_Present = False
        ElseIf BinaryData_Split.Length = 43 Then
            ''PayLoad >> Present
            PayLoad_Present = True
        ElseIf BinaryData_Split.Length = 44 Then
            ''PayLoad >> Present
            PayLoad_Present = True
        End If

        ''34 >> Upto Date, No PayLoad (in some cases)
        ''43 >> TCPDataLogger_06-11-2017.txt (regular)
        ''44 >> ends with 00000000 >> TCPDataLogger_05-11-2017.txt (only upto 05.11.2017)
        If BinaryData_Split.Length < 34 Then
            ''less than 34 >> do not process
            Exit Sub
        ElseIf BinaryData_Split.Length <> 43 Then
            'Exit Sub
        End If

        ''Total Length of SDB Data >> 43 (0 to 42)
        Dim Version As String                   '0  - 1 Version (should always = 1)
        Dim TotalMessageLength_1 As String      '1  - 1 2nd+3rd = unsigned short value >> total message length
        Dim TotalMessageLength_2 As String      '2  - 2 2nd+3rd = unsigned short value >> total message length
        Dim MOHeaderIEI As String               '3  - 1 MO Header IEI (should always = 1)
        Dim UnsignedShortForLength As String    '4  - 1 with next byte is unsigned short for length
        Dim MODirectIP As String                '5  - 1 MO DirectIP head length (should always = 28)
        Dim CDRReference_1 As String            '6  - 1 CDR Reference (an automatic ID number)
        Dim CDRReference_2 As String            '7  - 2 CDR Reference (an automatic ID number)
        Dim CDRReference_3 As String            '8  - 3 CDR Reference (an automatic ID number)
        Dim CDRReference_4 As String            '9  - 4 CDR Reference (an automatic ID number)
        Dim IMEI_Number As String               '10-24 - 01 to 15, IMEI First byte of a 15 byte sequence >> "300xxxxxxxxxxx0"
        Dim SessionStatus As String             '25  - 1 Session status - 0 = success, 1&2 also mostly ok. 10, 12 - 15 = problem
        Dim MOMSN_1 As String                   '26  - 1 MOMSN -Message Originate Message Sequence Number
        Dim MOMSN_2 As String                   '27  - 2 MOMSN -Message Originate Message Sequence Number
        Dim MTMSN_1 As String                   '28  - MTMSN - This Is Not an MT, so no MT Message Sequence Number
        Dim MTMSN_2 As String                   '29  - MTMSN - This Is Not an MT, so no MT Message Sequence Number
        Dim TimeOfSession_1 As String           '30  - 1 Time of session in epoch time (4 bytes)
        Dim TimeOfSession_2 As String           '31  - 2 Time of session in epoch time (4 bytes)
        Dim TimeOfSession_3 As String           '32  - 3 Time of session in epoch time (4 bytes)
        Dim TimeOfSession_4 As String           '33  - 4 Time of session in epoch time (4 bytes)
        Dim StartofPayload As String            '34  - 1 'Start of Payload IEI type - MO payload IEI (actual) (should always = 2)
        Dim LengthofPayload_1 As String         '35  - 1 Length of this payload (2 bytes)
        Dim LengthofPayload_2 As String         '36  - 2 Length of this payload (2 bytes)
        Dim Payload_1 As String                 '37  - Payload 1
        Dim Payload_2 As String                 '38  - Payload 2
        Dim Payload_3 As String                 '39  - Payload 3
        Dim Payload_4 As String                 '40  - Payload 4
        Dim Payload_5 As String                 '41  - Payload 5
        Dim Payload_6 As String                 '42  - Payload 6

        ''Version (should always = 1)
        ''00000001	01	1	0	Version (should always = 1)
        Version = Convert.ToInt32(BinaryData_Split(0), 2).ToString()

        ''Total Message Length
        ''00000000    00	0	1	2nd+3rd = unsigned short value
        ''00101000    28	40	2	total message length
        ''00000000 00101000 = 40
        TotalMessageLength_1 = BinaryData_Split(1)
        TotalMessageLength_2 = BinaryData_Split(2)
        Dim TotalMessageLength_Build As String = TotalMessageLength_1 + TotalMessageLength_2
        Dim TotalMessageLength As String = Convert.ToInt32(TotalMessageLength_Build, 2).ToString()

        ''MO Header IEI (should always = 1)
        ''00000001	01	1	3	MO Header IEI (should always = 1)
        MOHeaderIEI = Convert.ToInt32(BinaryData_Split(3), 2).ToString()

        ''MO DirectIP head length (should always = 28)
        ''00000000    00	0	4	with next byte Is unsigned short for length
        UnsignedShortForLength = Convert.ToInt32(BinaryData_Split(4), 2).ToString()

        ''00011100    1C	28	5	MO DirectIP head length (should always = 28)
        MODirectIP = Convert.ToInt32(BinaryData_Split(5), 2).ToString()

        ''CDR Reference (an automatic ID number)
        ''01010001    51	81	6	- 1
        ''00101101    2D	45	7	- 2
        ''01000010    42	66	8	- 3
        ''11001010    CA	202	9 	- 4
        ''01010001 00101101 01000010 11001010 = 1,361,920,714
        CDRReference_1 = BinaryData_Split(6)
        CDRReference_2 = BinaryData_Split(7)
        CDRReference_3 = BinaryData_Split(8)
        CDRReference_4 = BinaryData_Split(9)
        Dim CDRReference_Build As String = CDRReference_1 + CDRReference_2 + CDRReference_3 + CDRReference_4
        Dim CDRReference As String = Convert.ToInt32(CDRReference_Build, 2).ToString()

        ''IMEI First byte of a 15 byte sequence >> "300xxxxxxxxxxx0"
        ''00110011    33	10	-  1	'3'
        ''00110000    30	11	-  2	'0'
        ''00110000    30	12	-  3	'0'
        ''00110000    30	13	-  4	'0'
        ''00110000    30	14	-  5	'0'
        ''00110000    30	15	-  6	'0'
        ''00110000    30	16	-  7	'0'
        ''00110000    30	17	-  8	'0'
        ''00110000    30	18	-  9	'0'
        ''00110000    30	19	- 10	'0'
        ''00110000    30	20	- 11	'0'
        ''00110000    30	21	- 12	'0'
        ''00110000    30	22	- 13	'0'
        ''00110000    30	23	- 14	'0'
        ''00110000    30	24	- 15	'0'
        ''0011 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000
        ''3    0    0    0    0    0    0    0    0    0    0    0    0    0    0
        ''00110011 >> Split in half >> 0011 0011 >> IMEI(01-15) is second half
        Dim IMEI_String As String = Nothing
        Dim IMEI_FBS_Array(14) As String
        For k = 0 To 14
            IMEI_FBS_Array(k) = BinaryData_Split(k + 10)
            Dim IMEI_FBS_Split As String() = Enumerable.Range(0, IMEI_FBS_Array(k).Length \ 4).[Select](Function(i) IMEI_FBS_Array(k).Substring(i * 4, 4)).ToArray()
            IMEI_String += Convert.ToInt32(IMEI_FBS_Split(1), 2).ToString
        Next
        IMEI_Number = IMEI_String

        ''Session status - 0=success, 1&2 also mostly ok. 10,12-15=Problem
        ''00000000	00	0	25	- 0=success
        SessionStatus = Convert.ToInt32(BinaryData_Split(25), 2).ToString

        ''MOMSN - Message Originate Message Sequence Number
        ''01010000    50	80	26	- 1
        ''01011000    58	88	27	- 2
        ''01010000 01011000 = 20,568
        MOMSN_1 = BinaryData_Split(26)
        MOMSN_2 = BinaryData_Split(27)
        Dim MOMSN_Build As String = MOMSN_1 + MOMSN_2
        Dim MOMSN As String = Convert.ToInt32(MOMSN_Build, 2).ToString()

        ''MTMSN - Not message terminated so… (should always = 0)
        ''00000000    00	0	28	- 1
        ''00000000    00	0	29	- 2
        ''00000000 00000000 = 0
        MTMSN_1 = BinaryData_Split(28)
        MTMSN_2 = BinaryData_Split(29)
        Dim MTMSN_Build As String = MTMSN_1 + MTMSN_2
        Dim MTMSN As String = Convert.ToInt32(MTMSN_Build, 2).ToString()

        ''UNIX timestamp >> Unix's epoch >> elapsed seconds since January 1st 1970 00:00:00 UTC
        ''Time of session in epoch time (4 bytes) >> GMT
        ''01011001    59	89	30	- 1
        ''11111101    FD	253	31	- 2
        ''00111000    38	56	32	- 3
        ''11111110    FE	254	33	- 4
        ''01011001 11111101 00111000 11111110 = 1509767422
        ''Local >> Friday, November 3, 2017 10:50:42 PM GMT-05:00 DST
        ''Email >> Time of Session (UTC): Sat Nov 4 03:50:22 2017
        TimeOfSession_1 = BinaryData_Split(30)
        TimeOfSession_2 = BinaryData_Split(31)
        TimeOfSession_3 = BinaryData_Split(32)
        TimeOfSession_4 = BinaryData_Split(33)
        Dim TimeOfSession_Build As String = TimeOfSession_1 + TimeOfSession_2 + TimeOfSession_3 + TimeOfSession_4
        Dim TimeOfSession_String As String = Convert.ToInt32(TimeOfSession_Build, 2).ToString()

        ''EpochTime to DateTime (UTC)
        'TimeOfSession_String = "1509767422" '<< Test purpose only
        Dim EpochDateTime As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
        Dim SessionDateTime As DateTime = EpochDateTime.AddSeconds(TimeOfSession_String)
        Dim TimeOfSession As String = SessionDateTime.ToString("ddd MMM dd HH:mm:ss yyyy") 'Sat Nov 4 03:50:22 2017
        'MsgBox("Time of Session (UTC): " + TimeOfSession)

        ''PayLoad
        Dim LengthofPayload As String = Nothing
        Dim SupplyVoltage As String = Nothing
        Dim ADCChannel_2 As String = Nothing
        Dim ADCChannel_3 As String = Nothing
        Dim GPIOChannel_0 As String = Nothing
        Dim GPIOChannel_1 As String = Nothing

        ''43 >> TCPDataLogger_06-11-2017.txt (regular)
        If PayLoad_Present = True Then


            ''Start of Payload IEI type - MO payload IEI (actual) (should always = 2)
            ''00000010	02	2	34
            StartofPayload = Convert.ToInt32(BinaryData_Split(34), 2).ToString()

            ''Length of this payload (2 bytes)
            ''00000000    00	0	35	- 1
            ''00000110    6     6	36	- 2
            ''00000000 00000110 = 6
            LengthofPayload_1 = BinaryData_Split(35)
            LengthofPayload_2 = BinaryData_Split(36)
            Dim LengthofPayload_Build As String = LengthofPayload_1 + LengthofPayload_2
            'Dim LengthofPayload As String
            LengthofPayload = Convert.ToInt32(LengthofPayload_Build, 2).ToString()

            ''DATA First byte >> Data Last Byte, where N Is 33 plus the value of part 33 (i.e. payload length)
            ''00111100    3C	60	37	- Payload 1
            ''00110011    33	51	38	- Payload 2
            ''01001111    4F	79	39	- Payload 3
            ''01101010    6A	106	40	- Payload 4
            ''10001101    8D	141	41	- Payload 5
            ''00000110     6	6	42	- Payload 6
            Payload_1 = BinaryData_Split(37)
            Payload_2 = BinaryData_Split(38)
            Payload_3 = BinaryData_Split(39)
            Payload_4 = BinaryData_Split(40)
            Payload_5 = BinaryData_Split(41)
            Payload_6 = BinaryData_Split(42)
            Dim Payload_Build As String = Payload_1 + Payload_2 + Payload_3 + Payload_4 + Payload_5 + Payload_6
            Dim Payload_Binary As String = Payload_Build

            ''Binary to Decimal
            Dim Payload_DecimalArray(5) As String
            Dim Payload_BinaryArray(5) As String
            For k = 0 To 5
                Payload_BinaryArray(k) = BinaryData_Split(k + 37)
                Payload_DecimalArray(k) = Convert.ToInt32(Payload_BinaryArray(k), 2).ToString
            Next
            totalBits = 8 'Reset every time...
            bytesRead = Payload_DecimalArray

            ''bytesRead = {60, 139, 32, 73, 142, 4} '<< Test purpose only
            ''SupplyVoltage   5.375
            ''ADCChannel_2    0.941
            ''ADCChannel_3    0.917
            ''GPIOChannel_0   0
            ''GPIOChannel_1   1

            'Dim SupplyVoltage As String = Nothing
            'Dim ADCChannel_2 As String = Nothing
            'Dim ADCChannel_3 As String = Nothing
            'Dim GPIOChannel_0 As String = Nothing
            'Dim GPIOChannel_1 As String = Nothing

            Dim v As Double
            Dim bitval As Integer
            Dim supplyvbits = 9, adc1bits = 12, adc2bits = 12, gpio1bits = 1, gpio2bits = 1

            ''If (bytesRead[0] & 1) = 1		'GPS Informations >> Lat, Long
            ''If (bytesRead[0] & 2) = 2		'GPS Informations >> Year, Day, Hour, Minutes
            ''If (bytesRead[0] & 4) = 4		'Supply Voltage (9 bits)
            ''If (bytesRead[0] & 8) = 8		'ADC2 (12 bits)
            ''If (bytesRead[0] & 16) = 16   'ADC3 (12 bits)
            ''If (bytesRead[0] & 32) = 32   'GPIO 0 And 1 (1 bit each)
            ''If (bytesRead[0] & 64) = 64   'Accelelometer Data X Y, Z, And Temperature
            ''If (bytesRead[0] & 128) = 128 'Gyroscope Data X, Y, Z

            If (bytesRead(0) And 4) = 4 Then
                ''Supply Voltage (9 bits)
                bitval = ReadBitValues(supplyvbits)
                v = CSng(bitval * 0.038672)
                v = Math.Round(v, 3)
                SupplyVoltage = v.ToString
            Else
                SupplyVoltage = "NA"
            End If

            If (bytesRead(0) And 8) = 8 Then
                ''ADC2 (12 bits)
                bitval = ReadBitValues(adc1bits)
                v = CSng(3.3 * bitval / 4096)
                v = Math.Round(v, 3)
                ADCChannel_2 = v.ToString
            Else
                ADCChannel_2 = "NA"
            End If

            If (bytesRead(0) And 16) = 16 Then
                ''ADC3 (12 bits)
                bitval = ReadBitValues(adc2bits)
                v = CSng(3.3 * bitval / 4096)
                v = Math.Round(v, 3)
                ADCChannel_3 = v.ToString
            Else
                ADCChannel_3 = "NA"
            End If

            If (bytesRead(0) And 32) = 32 Then
                ''GPIO 0 And 1(1 bit each)
                ''GPIO 0 (1 bit)
                bitval = ReadBitValues(gpio1bits)
                GPIOChannel_0 = bitval.ToString

                ''GPIO 1 (1 bit)
                bitval = ReadBitValues(gpio2bits)
                GPIOChannel_1 = bitval.ToString
            Else
                GPIOChannel_0 = "NA"
                GPIOChannel_1 = "NA"
            End If

            ''#	IMEI    SessionStatus	MessageSequenceNumber	TimeofSession   LengthofPayload	SupplyVoltage   Float1	Float2	Float3	Float4	Float5	Float6	Float7	Float8	Level1	Level2	Level3	Level4
            WriteDecodeLogFile(BinaryData_SlNo + vbTab + IMEI_Number + vbTab + SessionStatus + vbTab + MOMSN + vbTab + TimeOfSession + vbTab + LengthofPayload + vbTab +
                           SupplyVoltage + vbTab + ADCChannel_2 + vbTab + ADCChannel_3 + vbTab + GPIOChannel_0 + vbTab + GPIOChannel_1)

            ''Version                   1
            ''TotalMessageLength        40
            ''MOHeaderIEI               1
            ''UnsignedShortForLength    0
            ''MODirectIP                28
            ''CDRReference              1374135862
            ''IMEI_Number               300xxxxxxxxxxx0
            ''SessionStatus             0
            ''MOMSN                     21479
            ''MTMSN                     0
            ''TimeOfSession             Sun Nov 05 17: 06:52 2017
            ''StartofPayload            2
            ''LengthofPayload           6
            ''Payload_Binary            001111001010011100110110100010011010000000000110
            ''[05-11-2017 17:56:35.204]	1	40	1	0	28	1374135862	300xxxxxxxxxxx0	0	21479	0	Sun Nov 05 17:06:52 2017	2	6	001111001010011100110110100010011010000000000110
            'WriteDecodeLogFile(Version + vbTab + TotalMessageLength + vbTab + MOHeaderIEI + vbTab +
            '                   UnsignedShortForLength + vbTab + MODirectIP + vbTab + CDRReference + vbTab +
            '                   IMEI_Number + vbTab + SessionStatus + vbTab + MOMSN + vbTab + MTMSN + vbTab +
            '                   TimeOfSession + vbTab + StartofPayload + vbTab + LengthofPayload + vbTab + Payload_Binary)
        Else
            ''34 >> Upto Date, No PayLoad (in some cases)

            ''#	IMEI    SessionStatus	MessageSequenceNumber	TimeofSession   LengthofPayload	SupplyVoltage   Float1	Float2	Float3	Float4	Float5	Float6	Float7	Float8	Level1	Level2	Level3	Level4
            WriteDecodeLogFile(BinaryData_SlNo + vbTab + IMEI_Number + vbTab + SessionStatus + vbTab + MOMSN + vbTab + TimeOfSession + vbTab + LengthofPayload + vbTab +
                           SupplyVoltage + vbTab + ADCChannel_2 + vbTab + ADCChannel_3 + vbTab + GPIOChannel_0 + vbTab + GPIOChannel_1)
        End If
    End Sub

    Dim totalBits As Integer
    Dim bytesRead As String()
    Private Function ReadBitValues(bitstoread As Integer) As Integer
	''ReadBitValues part is from SBD Parser Tool
        Dim revrightbits As Byte() = {0, 1, 3, 7, 15, 31, 63, 127, 255}
        Dim revleftbits As Byte() = {0, 128, 192, 224, 240, 248, 252, 254, 255}
        Try
            Dim returnval As Integer = 0
            totalBits += bitstoread
            Dim q As Integer = Math.Truncate(totalBits / 8)     'Lower >> 2.15 >> 2
            Dim r As Integer = totalBits Mod 8
            Dim m As Integer = Math.Truncate(bitstoread / 8)    'Lower >> 2.15 >> 2

            If r = 0 Then
                For i As Integer = 0 To m - 1 Step i + 1
                    returnval = returnval << 8
                    returnval += bytesRead(q - 1 - i)
                Next
                Dim [rem] As Integer = bitstoread - (m * 8)
                returnval = returnval << [rem]

                Dim rembyte As Byte = bytesRead(q - m - 1) And (revleftbits([rem]))
                rembyte = CByte(rembyte >> (8 - [rem]))
                returnval += rembyte
                Return returnval
            Else
                If r < bitstoread Then
                    returnval = CByte(bytesRead(q) And revrightbits(r))
                Else
                    Dim returnbits As Byte = bytesRead(q) And revrightbits(r)
                    returnval = returnbits >> (r - bitstoread)
                    Return returnval
                End If

                m = Math.Truncate((bitstoread - r) / 8)    'Lower >> 0.15 >> 0

                For i As Integer = 0 To m - 1 Step i + 1
                    returnval = returnval << 8
                    returnval += bytesRead(q - 1 - i)
                Next
                Dim [rem] As Integer = bitstoread - ((m * 8) + r)
                returnval = returnval << [rem]
                Dim rembyte As Byte = bytesRead(q - m - 1) And (revleftbits([rem]))
                rembyte = rembyte >> (8 - [rem])
                returnval += rembyte
                Return returnval
            End If
        Catch
            Return 0
        End Try
    End Function

    Public Function Conv_ByteToDec(ByVal conv() As Byte) As String
        ''Bytes to Decimal
        Dim newDec As New StringBuilder
        For Each c In conv
            Dim Decb1 As String = Conversion.Int(c)
            Dim padb1 As String = Decb1.ToString.PadLeft(2, "0")
            newDec.Append(Convert.ToString(padb1).PadLeft(2, "0"))
        Next
        Return newDec.ToString
    End Function

    Public Function Conv_ByteToBin(ByVal conv() As Byte) As String
        ''Bytes to Binary
        Dim newBin As New StringBuilder
        For Each c In conv
            newBin.Append(Convert.ToString(c, 2).PadLeft(8, "0"))
        Next
        Return newBin.ToString
    End Function

    Public Function Conv_ByteToHex(ByVal conv() As Byte) As String
        ''Binary to Hex
        Dim newHex As New StringBuilder
        For Each c In conv
            Dim hexb1 As String = Conversion.Hex(c)
            Dim padb1 As String = hexb1.ToString.PadLeft(2, "0")
            'Dim IntNewHex = Val("&H" & hexb1)
            newHex.Append(Convert.ToString(padb1).PadLeft(2, "0"))
        Next
        Return newHex.ToString
    End Function

    Sub Msg(ByVal mesg As String)
        mesg.Trim()
        WriteTCPLogFile(mesg)
        'Console.WriteLine(" >> " + mesg)
        'Console.ReadLine()
    End Sub

    Sub WriteTCPLogFile(WriteTCPListner As String)
        '' Folder Not found... Create New Folder...
        Dim TCPDataLogger_FolderPath As String = System.IO.Directory.GetCurrentDirectory + "\TCPDataLogger"
        If (Not System.IO.Directory.Exists(TCPDataLogger_FolderPath)) Then
            Try
                System.IO.Directory.CreateDirectory(TCPDataLogger_FolderPath)
            Catch
                TCPDataLogger_FolderPath = System.IO.Directory.GetCurrentDirectory
            End Try
        End If

        ''New file Every Day...
        Dim DataTimeStamp As String = Date.Now.ToString("dd-MM-yyyy", Globalization.CultureInfo.InvariantCulture)
        Dim TCPListnerLog = TCPDataLogger_FolderPath + "\TCPDataLogger_" + DataTimeStamp + ".txt"
        Try
            ''TCPListnerLog >> changed from ASCII to UTF8, save file, Unicode Characters
            Dim TCPListnerLogfile As System.IO.StreamWriter = New StreamWriter(TCPListnerLog, True, Encoding.ASCII) 'ASCII
            Dim TimeStampNow As String = Date.Now.ToString("dd-MM-yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture)
            'TCPListnerLogfile = My.Computer.FileSystem.OpenTextFileWriter(TCPListnerLog, True)'ASCII
            TCPListnerLogfile.WriteLine("[" + TimeStampNow + "]" + vbTab + WriteTCPListner)
            TCPListnerLogfile.Close()
        Catch ex As Exception
        End Try
    End Sub

    Sub WriteTCPIntrusionLogFile(WriteTCPIntrusion As String)
        '' Folder Not found... Create New Folder...
        Dim TCPIntrusion_FolderPath As String = System.IO.Directory.GetCurrentDirectory + "\TCPIntrusionLog"
        If (Not System.IO.Directory.Exists(TCPIntrusion_FolderPath)) Then
            Try
                System.IO.Directory.CreateDirectory(TCPIntrusion_FolderPath)
            Catch
                TCPIntrusion_FolderPath = System.IO.Directory.GetCurrentDirectory
            End Try
        End If

        ''New file Every Month...
        Dim DataTimeStamp As String = Date.Now.ToString("MM-yyyy", Globalization.CultureInfo.InvariantCulture)
        Dim TCPIntrusionLog = TCPIntrusion_FolderPath + "\TCPIntrusionLog_" + DataTimeStamp + ".txt"
        Try
            Dim TCPIntrusionLogfile As System.IO.StreamWriter = New StreamWriter(TCPIntrusionLog, True, Encoding.ASCII) 'ASCII
            Dim TimeStampNow As String = Date.Now.ToString("dd-MM-yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture)
            TCPIntrusionLogfile.WriteLine("[" + TimeStampNow + "]" + vbTab + WriteTCPIntrusion)
            TCPIntrusionLogfile.Close()
        Catch ex As Exception
        End Try
    End Sub

    Sub WriteDecodeLogFile(WriteDataDecode As String)
        '' Folder Not found... Create New Folder...
        Dim DataDecode_FolderPath As String = System.IO.Directory.GetCurrentDirectory + "\TCPDataDecode"
        If (Not System.IO.Directory.Exists(DataDecode_FolderPath)) Then
            Try
                System.IO.Directory.CreateDirectory(DataDecode_FolderPath)
            Catch
                DataDecode_FolderPath = System.IO.Directory.GetCurrentDirectory
            End Try
        End If

        ''New file Every Day...
        Dim DataTimeStamp As String = Date.Now.ToString("dd-MM-yyyy", Globalization.CultureInfo.InvariantCulture)
        Dim DataDecodeLog = DataDecode_FolderPath + "\TCPDataDecode_" + DataTimeStamp + ".txt"
        Try
            ''DataDecodeLog >> changed from ASCII to UTF8, save file, Unicode Characters
            Dim DataDecodeLogfile As System.IO.StreamWriter = New StreamWriter(DataDecodeLog, True, Encoding.ASCII) 'ASCII
            Dim TimeStampNow As String = Date.Now.ToString("dd-MM-yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture)
            'DataDecodeLogfile = My.Computer.FileSystem.OpenTextFileWriter(DataDecodeLog, True)'ASCII
            DataDecodeLogfile.WriteLine("[" + TimeStampNow + "]" + vbTab + WriteDataDecode)
            DataDecodeLogfile.Close()
        Catch ex As Exception
        End Try
    End Sub

End Module