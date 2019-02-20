using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Text;

using System.Net.Sockets;

//-------------------------------------------------------------------------------------
// Packet Layouts:
//-------------------------------------------------------------------------------------
// SignatureStart	(2 bytes little endian ushort) = 16018
// PacketLength		(2 bytes little endian ushort)
// PacketID         (2 bytes little endian ushort)
// Channel      	(4 bytes little endian int)
// Message			(PacketLength - 12 bytes google protocol buffer)
// SignatureEnd		(2 bytes little endian ushort) = 16108
//-------------------------------------------------------------------------------------

public class CHSocketReader
{
    Socket _socket = null;
    //ThreadSafeStopwatch watch = null;
    //PacketPool pool = new PacketPool();
    public const int MAX_BUFFER_SIZE = 8192;
    byte[] buffer = new byte[MAX_BUFFER_SIZE];
    public CHPacketParser parser;
    public int totalPackets = 0;
    public int totalBytes = 0;
    public delegate void EndCallback();
    EndCallback endCB = null;

    public CHSocketReader(Socket socket, EndCallback _endCB, CHSocket baseSocket)
    {
        endCB = _endCB;
        _socket = socket;
        parser = new CHPacketParser(baseSocket);
        BeginBackgroundRead();
    }

    void BeginBackgroundRead()
    {
        _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReadCallback, null);
    }

    // .Net Thread
    void ReadCallback(IAsyncResult result)
    {
        totalPackets++;
        if (_socket != null && _socket.Connected)
        {
            int read = 0;
            try
            {
                read = _socket.EndReceive(result);
                if (read <= 0)
                {
                    if (endCB != null)
                        endCB();
                    return;
                }
            }
            catch (Exception e)
            {
                int errorCode = 0;
                if (e is SocketException)
                {
                    errorCode = (e as SocketException).ErrorCode;
                }
                Debug.LogWarning("!!!Socket receive exception. Error msg: " + e.Message + "  |||Error code:" + errorCode);
                if (endCB != null)
                    endCB();
                return;
            }
            totalBytes += read;

            // parse bytes to packets
            parser.Parse(buffer, 0, read);

            // read recurse
            _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReadCallback, null);
        }
        else
        {
            Debug.LogWarning("!!!Socket receive Disconnected!");
        }
    }
}

public class CHPacketParser
{
    MemoryStream remain = new MemoryStream(CHSocketReader.MAX_BUFFER_SIZE);
    public List<CHMatchReceiveMsg> receiveList = new List<CHMatchReceiveMsg>();
    public CHSocket _baseSocket;   //对象传入修改
    bool ifcompress = true;

    public CHPacketParser(CHSocket baseSocket)
    {
        _baseSocket = baseSocket;
        ifcompress = baseSocket.ifCompress;
    }

    public int RemainBytes()
    {
        return (int)remain.Capacity - (int)remain.Position;
    }

    bool ErrorDetected(byte[] buffer, int offset, int count)
    {
        if (buffer.Length < offset + count)
            return true;

        return false;
    }

    public int Parse(byte[] buffer, int offset, int count)  //, CHPacketPool pool, ThreadSafeStopwatch watch
    {
        string msg = Encoding.UTF8.GetString(buffer, 0, count);
        _baseSocket._receiveList.Add(msg);

        //if (ErrorDetected(buffer, offset, count))
        //{
        //    //Debug.Log("PacketParser Critical Error");
        //    //Debug.Break();
        //}
        ////20160718 Buffer Overflow Check
        //int remainedBuffer = count - RemainBytes();
        //int bufferReadPosition = offset + count - remainedBuffer;
        //if (remainedBuffer > 0)
        //{
        //    count = RemainBytes();
        //    remain.Write(buffer, offset, RemainBytes());
        //    //Debug.Log("PacketParser MemoryStream Overflow");
        //}
        //else
        //{
        //    // add to last remaining bytes
        //    remain.Write(buffer, offset, count);
        //}

        //byte[] bytes = remain.ToArray();
        //count = bytes.Length;

        //// parse!
        //int processed = 0;
        //while (processed < count)
        //{
        //    CHMatchReceiveMsg packet = CHMatchReceiveMsg.Decode(bytes, processed, count - processed, ifcompress);
        //    if (packet == null)
        //        break;

        //    processed += packet.length;
        //    //packet.receiveTime = watch.ElapsedSeconds();
        //    if (_baseSocket != null)
        //    {
        //        lock (_baseSocket.lockObj)
        //        {
        //            _baseSocket._receiveList.Add(packet);
        //        }
        //    }
        //}

        //// clear remain stream
        //remain.Seek(0, SeekOrigin.Begin);
        //remain.SetLength(0);

        //// backup remaining bytes
        //if (processed < bytes.Length)
        //{
        //    remain.Write(bytes, processed, bytes.Length - processed);
        //}

        //if (remainedBuffer > 0)
        //{
        //    if (remainedBuffer > RemainBytes())
        //    {
        //        //Debug.Log("PacketParser Critical Error2");
        //        //Debug.Break();
        //    }

        //    remain.Write(buffer, bufferReadPosition, remainedBuffer);
        //}

        return 1;
    }

    public static string UnGzipString(byte[] bytes)
    {
        using (MemoryStream dms = new MemoryStream())
        {
            using (MemoryStream cms = new MemoryStream(bytes))
            {
                //using (GZipInputStream gizp = new GZipInputStream(cms))
                //{
                //    byte[] nbytes = new byte[4096];
                //    int len = 0;
                //    while ((len = gizp.Read(nbytes, 0, nbytes.Length)) != 0)
                //    {
                //        dms.Write(nbytes, 0, len);
                //    }
                //}
            }
            string str = WWW.UnEscapeURL(Encoding.UTF8.GetString(dms.ToArray()));
            //Debug.Log("[UnGzipString Result] ==> " + str);
            return str;
        }
    }
}

public class CHMatchReceiveMsg
{
    const ushort SIGNATURE_START = 16018;
    const ushort SIGNATURE_END = 16108;
    public ushort length;
    public ushort key;
    public string msg;
    public CHMatchReceiveMsg(ushort _key, string _msg)
    {
        key = _key;
        msg = _msg;
    }
    public CHMatchReceiveMsg() { }

    public static CHMatchReceiveMsg Decode(byte[] buffer , int offset, int count, bool ifcompress)
    {
        if (count < 8) // header + tail
            return null;

        CHMatchReceiveMsg _msg = new CHMatchReceiveMsg();
        // 开始标签
        int size = 2;
        ushort signature = LittleEndianBytesToUshort(buffer, offset);
        //Debug.Log("signature: " + signature);
        if (signature != CHMatchReceiveMsg.SIGNATURE_START)
            return null;
        offset += size;

        //消息长度
        size = 2;
        _msg.length = LittleEndianBytesToUshort(buffer, offset);
        //Debug.Log("_msg.length: " + _msg.length);
        if (count < _msg.length)
            return null;
        offset += size;

        //消息键值
        size = 2;
        _msg.key = LittleEndianBytesToUshort(buffer, offset);
        //Debug.Log("_msg.key: " + _msg.key);
        offset += size;

        //消息内容
        size = _msg.length - 8;
        _msg.msg = LittleEndianBytesToString(buffer, offset, size, ifcompress);
        //Debug.Log("_msg.msg: " + _msg.msg);
        offset += size;

        //结束标签
        size = 2;
        signature = LittleEndianBytesToUshort(buffer, offset);
        //Debug.Log("signature end: " + signature);
        if (signature != CHMatchReceiveMsg.SIGNATURE_END)
            return null;

        return _msg;        
    }

    static byte[] UshortToLittleEndianBytes(ushort val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes, 0, 2);
        return bytes;
    }

    static byte[] intToLittleEndianBytes(uint val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes, 0, 2);
        return bytes;
    }

    static ushort LittleEndianBytesToUshort(byte[] buffer, int start)
    {
        byte[] bytes = new byte[2] { buffer[start], buffer[start + 1] };
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes, 0, 2);
        return BitConverter.ToUInt16(bytes, 0);
    }

    static int LittleEndianBytesToInt(byte[] buffer, int start)
    {
        byte[] bytes = new byte[4] { buffer[start], buffer[start + 1], buffer[start + 2], buffer[start + 3] };
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes, 0, 4);
        return BitConverter.ToInt32(bytes, 0);
    }

    static string LittleEndianBytesToString(byte[] buffer, int start, int len, bool ifcompress)
    {
        //return System.Text.Encoding.Default.GetString(buffer, start, len);
        byte[] bytes = new byte[len];
        Array.Copy(buffer, start, bytes, 0, len);
        if (ifcompress)
        {
            return CHPacketParser.UnGzipString(DecodeMessage(bytes));
        }
        else
            return Encoding.UTF8.GetString(bytes);
    }

    private static uint[] xor_table = new uint[]
    {
        9u,
        12u,
        19u,
        83u,
        4u,
        93u,
        49u,
        7u,
        76u,
        10u,
        6u,
        56u,
        76u,
        89u,
        45u,
        87u,
        35u,
        68u,
        77u,
        1u,
        120u,
        55u,
        84u,
        102u,
        2u,
        87u,
        1u,
        98u,
        11u,
        119u,
        6u,
        124u,
        106u,
        9u,
        11u,
        71u,
        40u,
        23u,
        96u,
        115u,
        74u,
        117u,
        42u,
        99u,
        30u,
        6u,
        42u,
        51u,
        29u,
        41u,
        32u,
        76u,
        84u,
        101u,
        62u,
        85u,
        79u,
        27u,
        50u,
        13u,
        113u,
        41u,
        34u,
        50u,
        91u,
        77u,
        26u,
        30u,
        102u,
        39u,
        17u,
        105u,
        86u,
        46u,
        120u,
        31u,
        24u,
        56u,
        101u,
        43u,
        91u,
        35u,
        21u,
        105u,
        29u,
        63u,
        10u,
        29u,
        18u,
        122u,
        45u,
        82u,
        26u,
        61u,
        118u,
        24u,
        9u,
        81u,
        77u,
        6u
    };
    static byte[] DecodeMessage(byte[] bytes)
    {
        //return bytes;

        string encode = Encoding.UTF8.GetString(bytes);
        StringBuilder stringBuilder = new StringBuilder();
        int len = encode.Length;
        int table_len = xor_table.Length;
        for (int i = 0; i < len; i++)
        {
            uint num = xor_table[i % table_len];
            char value = (char)((uint)encode[i] ^ num);
            stringBuilder.Append(value);
        }
        return Convert.FromBase64String(stringBuilder.ToString());
    }
    public static byte[] EncodeMessage(string msg)
    {
        //return Encoding.UTF8.GetBytes(msg);

        char[] array = msg.ToCharArray();
        StringBuilder stringBuilder = new StringBuilder();
        int len = array.Length;
        int table_len = xor_table.Length;
        for(int i =0;i <len;i++ )
        {
            uint num = xor_table[i % table_len];
            char value = (char)((uint)array[i] ^ num);
            stringBuilder.Append(value);
        }
        return Encoding.UTF8.GetBytes(stringBuilder.ToString());
    }
}