# iridium sbd tcpip
- TCPServer.vb
- created by mahemys 2017.11.02
- !perfect, but works!
- MIT; no license; free to use!
- update 2017-11-02; initial review
- update 2017-11-07; optimisation

**purpose**
- Iridium Short Burst Data (SBD) TCP/IP Socket Communication. Auto save data stream received via tcp/ip socket.
- Iridium SBD delivers data to the user to specified TCP/IP Socket, and or to specified Email address.
- This program uses TCP/IP Socket Communication protocol.

**how to use**
- Create a VB.net project using this TCPServer.vb module file.
- Complie with any changes you require and generate TCPServer.exe file.
- Copy TCPServer.exe to specified folder.
- Create shortcut to run this program at startup.

**highlights**
- basically a robust tcp/ip port listner built to last.
- no GUI, simply runs in the background silently.
- logs for each process for data backup and activity monitoring.
- handles multiple client connections and delegates decoding task with MultiThreading.
- inbuilt iridium sbd data parser to save incoming binary data in a human readable format.

**logs**
```
- TCPDataLogger   >> raw incoming stream of data from iridium sbd.
- TCPDataDecode   >> data decoded from binary to ASCII.
- TCPIntrusionLog >> log of unwanted connections.
```

**requirements**
- Iridium SBD Hardware with active Subscription or Plan.

**recommendations**
- Static IP Address.
- TCP Port for program to listen.
- Proper Firewall Configuration.

**Server Specifications**
- Minimum specs for the Server/Device running this program is not defined.
- This program was running on an <b>AWS Server; t2-micro instance</b> for more than a year without any issues.
```
- Test Server -> t2-micro instance -> 1 vCPU; 1 GiB RAM; 30GiB Disk -> Microsoft Windows Server 2012.
```

**Server IP; Port**
```
Private IP of Server
Server >> TCPServer.exe
IP     >> xxx.xxx.xxx.xxx
Port   >> 443
```

**Firewall - TCP port 443**
```
Windows Firewall >> wf.msc
Inbound Rules    >> New Rule
Predefined       >> Secure Socket Tunneling Protocol
Secure Socket Tunneling Protocol (SSTP-In)
Inbound rule to allow HTTPS traffic for Secure Socket Tunneling Protocol. [TCP 443]
```

**Run at Start**
```
Run >> shell:startup
[C:\Users\Administrator\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup]
Add >> New >> Shortcut >> C:\Iridium\TCPServer.exe
```

**SBD Parser**
- some part(s) of code is borrowed/inspired from other publicly shared programs.
- we give respective person(s) or organisation(s) due credit(s) as required.
- like ReadBitValues part is from SBD Parser Tool.

**footnote**
- let me know if you find any bugs!
- Thank you mahemys
