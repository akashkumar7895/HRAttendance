using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RealtimeSyncTool;

public class BiometricRecord
{
    public string UserId { get; set; } = string.Empty;
    public DateTime PunchTime { get; set; }
    public int VerifyMode { get; set; }
    public int InOutMode { get; set; }
}

public class ZkClient : IDisposable
{
    private const int DefaultPort = 4370;
    private const ushort CMD_CONNECT = 1000;
    private const ushort CMD_EXIT = 1001;
    private const ushort CMD_ATTLOG_RRQ = 13;
    private const ushort CMD_ACK_OK = 2000;
    private const ushort CMD_ACK_DATA = 2001;
    private const ushort CMD_PREPARE_DATA = 1500;
    private const ushort CMD_DATA = 1501;

    private readonly string _ip;
    private readonly int _port;
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private UdpClient? _udpClient;
    private IPEndPoint? _remoteEndPoint;
    private bool _isTcp = true;

    public ushort SessionId { get; private set; } = 0;
    public ushort ReplyNumber { get; private set; } = 0;
    public bool IsConnected { get; private set; } = false;

    public ZkClient(string ip, int port = DefaultPort)
    {
        _ip = ip;
        _port = port;
    }

    public bool Connect()
    {
        if (TryConnectTcp())
        {
            _isTcp = true;
            return true;
        }

        if (TryConnectUdp())
        {
            _isTcp = false;
            return true;
        }

        return false;
    }

    private bool TryConnectTcp()
    {
        try
        {
            _tcpClient = new TcpClient();
            var result = _tcpClient.BeginConnect(_ip, _port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
            if (!success || !_tcpClient.Connected)
            {
                _tcpClient.Close();
                _tcpClient = null;
                return false;
            }

            _tcpClient.ReceiveTimeout = 5000;
            _tcpClient.SendTimeout = 5000;
            _stream = _tcpClient.GetStream();

            var packet = BuildPacket(CMD_CONNECT, 0, 0, Array.Empty<byte>(), true);
            _stream.Write(packet, 0, packet.Length);

            var resp = ReadResponseTcp();
            if (resp != null && resp.Length >= 8)
            {
                var cmd = BitConverter.ToUInt16(resp, 0);
                if (cmd == CMD_ACK_OK)
                {
                    SessionId = BitConverter.ToUInt16(resp, 4);
                    ReplyNumber = BitConverter.ToUInt16(resp, 6);
                    IsConnected = true;
                    return true;
                }
            }
        }
        catch
        {
            // Ignore and fallback
        }

        return false;
    }

    private bool TryConnectUdp()
    {
        try
        {
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            _udpClient = new UdpClient();
            _udpClient.Client.ReceiveTimeout = 4000;
            _udpClient.Client.SendTimeout = 4000;

            var packet = BuildPacket(CMD_CONNECT, 0, 0, Array.Empty<byte>(), false);
            _udpClient.Send(packet, packet.Length, _remoteEndPoint);

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            var resp = _udpClient.Receive(ref ep);
            if (resp != null && resp.Length >= 8)
            {
                var cmd = BitConverter.ToUInt16(resp, 0);
                if (cmd == CMD_ACK_OK)
                {
                    SessionId = BitConverter.ToUInt16(resp, 4);
                    ReplyNumber = BitConverter.ToUInt16(resp, 6);
                    IsConnected = true;
                    return true;
                }
            }
        }
        catch
        {
            // Ignore
        }

        return false;
    }

    public List<BiometricRecord> ReadAttendanceLogs()
    {
        var records = new List<BiometricRecord>();
        if (!IsConnected) return records;

        try
        {
            ReplyNumber++;
            var reqPacket = BuildPacket(CMD_ATTLOG_RRQ, SessionId, ReplyNumber, Array.Empty<byte>(), _isTcp);
            SendPacket(reqPacket);

            byte[]? resp = ReadResponse();
            if (resp == null || resp.Length < 8) return records;

            var cmd = BitConverter.ToUInt16(resp, 0);
            if (cmd == CMD_ACK_OK)
            {
                ParseAttendanceData(resp, 8, records);
            }
            else if (cmd == CMD_PREPARE_DATA)
            {
                int totalSize = BitConverter.ToInt32(resp, 8);
                var rawData = ReadLargeDataStream(totalSize);
                ParseAttendanceData(rawData, 0, records);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Log read warning: {ex.Message}");
        }

        return records;
    }

    private byte[] ReadLargeDataStream(int expectedSize)
    {
        using var ms = new MemoryStream();
        while (ms.Length < expectedSize)
        {
            var chunk = ReadResponse();
            if (chunk == null || chunk.Length == 0) break;

            if (chunk.Length >= 8)
            {
                var subCmd = BitConverter.ToUInt16(chunk, 0);
                if (subCmd == CMD_DATA)
                {
                    ms.Write(chunk, 8, chunk.Length - 8);
                }
                else if (subCmd == CMD_ACK_OK)
                {
                    break;
                }
            }
        }
        return ms.ToArray();
    }

    private void ParseAttendanceData(byte[] data, int offset, List<BiometricRecord> records)
    {
        if (data == null || data.Length <= offset) return;

        int recordLength = 40;
        int remaining = data.Length - offset;

        if (remaining % 40 == 0)
        {
            recordLength = 40;
        }
        else if (remaining % 14 == 0)
        {
            recordLength = 14;
        }

        for (int i = offset; i + recordLength <= data.Length; i += recordLength)
        {
            try
            {
                if (recordLength == 40)
                {
                    string userId = Encoding.ASCII.GetString(data, i, 24).Trim('\0', ' ');
                    if (string.IsNullOrEmpty(userId)) continue;

                    uint encodedTime = BitConverter.ToUInt32(data, i + 24);
                    DateTime punchTime = DecodeZkTime(encodedTime);
                    int verifyMode = data[i + 28];
                    int inOutMode = data[i + 29];

                    records.Add(new BiometricRecord
                    {
                        UserId = userId,
                        PunchTime = punchTime,
                        VerifyMode = verifyMode,
                        InOutMode = inOutMode
                    });
                }
                else if (recordLength == 14)
                {
                    ushort userIdNum = BitConverter.ToUInt16(data, i);
                    uint encodedTime = BitConverter.ToUInt32(data, i + 2);
                    DateTime punchTime = DecodeZkTime(encodedTime);
                    int verifyMode = data[i + 6];
                    int inOutMode = data[i + 7];

                    records.Add(new BiometricRecord
                    {
                        UserId = userIdNum.ToString(),
                        PunchTime = punchTime,
                        VerifyMode = verifyMode,
                        InOutMode = inOutMode
                    });
                }
            }
            catch { }
        }
    }

    private static DateTime DecodeZkTime(uint t)
    {
        try
        {
            int second = (int)(t % 60);
            t /= 60;
            int minute = (int)(t % 60);
            t /= 60;
            int hour = (int)(t % 24);
            t /= 24;
            int day = (int)(t % 31) + 1;
            t /= 31;
            int month = (int)(t % 12) + 1;
            t /= 12;
            int year = (int)(t + 2000);

            if (year >= 2020 && year <= 2035 && month >= 1 && month <= 12 && day >= 1 && day <= 31)
            {
                return new DateTime(year, month, day, hour, minute, second);
            }
        }
        catch { }
        return DateTime.Now;
    }

    private void SendPacket(byte[] packet)
    {
        if (_isTcp && _stream != null)
        {
            _stream.Write(packet, 0, packet.Length);
        }
        else if (_udpClient != null && _remoteEndPoint != null)
        {
            _udpClient.Send(packet, packet.Length, _remoteEndPoint);
        }
    }

    private byte[]? ReadResponse()
    {
        return _isTcp ? ReadResponseTcp() : ReadResponseUdp();
    }

    private byte[]? ReadResponseTcp()
    {
        if (_stream == null) return null;
        byte[] header = new byte[8];
        int read = 0;
        while (read < 8)
        {
            int r = _stream.Read(header, read, 8 - read);
            if (r <= 0) return null;
            read += r;
        }

        if (header[0] == 0x50 && header[1] == 0x50 && header[2] == 0x82 && header[3] == 0x7D)
        {
            int payloadLen = (int)BitConverter.ToUInt32(header, 4);
            byte[] payload = new byte[payloadLen];
            int pRead = 0;
            while (pRead < payloadLen)
            {
                int r = _stream.Read(payload, pRead, payloadLen - pRead);
                if (r <= 0) break;
                pRead += r;
            }
            return payload;
        }

        return header;
    }

    private byte[]? ReadResponseUdp()
    {
        if (_udpClient == null) return null;
        try
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            return _udpClient.Receive(ref ep);
        }
        catch
        {
            return null;
        }
    }

    private static byte[] BuildPacket(ushort cmd, ushort sessionId, ushort replyNum, byte[] data, bool isTcp)
    {
        byte[] inner = new byte[8 + data.Length];
        BitConverter.GetBytes(cmd).CopyTo(inner, 0);
        BitConverter.GetBytes((ushort)0).CopyTo(inner, 2);
        BitConverter.GetBytes(sessionId).CopyTo(inner, 4);
        BitConverter.GetBytes(replyNum).CopyTo(inner, 6);
        if (data.Length > 0)
        {
            data.CopyTo(inner, 8);
        }

        ushort chk = CalculateChecksum(inner);
        BitConverter.GetBytes(chk).CopyTo(inner, 2);

        if (!isTcp) return inner;

        byte[] tcpPacket = new byte[8 + inner.Length];
        tcpPacket[0] = 0x50;
        tcpPacket[1] = 0x50;
        tcpPacket[2] = 0x82;
        tcpPacket[3] = 0x7D;
        BitConverter.GetBytes((uint)inner.Length).CopyTo(tcpPacket, 4);
        inner.CopyTo(tcpPacket, 8);
        return tcpPacket;
    }

    private static ushort CalculateChecksum(byte[] buffer)
    {
        uint chk = 0;
        for (int i = 0; i < buffer.Length - 1; i += 2)
        {
            chk += BitConverter.ToUInt16(buffer, i);
        }
        if (buffer.Length % 2 != 0)
        {
            chk += buffer[^1];
        }
        chk = ~((chk & 0xFFFF) + (chk >> 16));
        return (ushort)(chk & 0xFFFF);
    }

    public void Disconnect()
    {
        if (IsConnected)
        {
            try
            {
                ReplyNumber++;
                var exitPacket = BuildPacket(CMD_EXIT, SessionId, ReplyNumber, Array.Empty<byte>(), _isTcp);
                SendPacket(exitPacket);
            }
            catch { }
            IsConnected = false;
        }

        _stream?.Dispose();
        _tcpClient?.Dispose();
        _udpClient?.Dispose();
    }

    public void Dispose()
    {
        Disconnect();
    }
}
