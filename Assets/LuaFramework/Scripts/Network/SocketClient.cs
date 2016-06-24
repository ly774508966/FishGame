using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;
using LuaInterface;

//public enum DisType {
//    Exception,
//    Disconnect,
//}

public class SocketClient {
    private byte[] recBuffer = new byte[1024];
    private TcpClient tcp;
    private SocketMessage msg;
    private MemoryStream tempBuffer;
    private int bufferOff;
    private const int handSize = 12;
    private const int MAX_READ = 1024 * 8;
    private bool policy = false;

    private void connect()
    {
        tcp = new TcpClient();
        tcp.Connect(AppConst.SocketAddress, AppConst.SocketPort);
        Debug.Log("is connect");
        asynRec();
    }

    /// <summary>
    /// 发送连接请求
    /// </summary>
    public void SendConnect()
    {
        connect();
    }

    //private IEnumerator sendData()
    //{
    //    yield return new WaitForSeconds(2);
    //    Debug.Log("is reade send msg");
    //    byte[] msg = serial("userId=123");
    //    byte[] data = new byte[12 + msg.Length];
    //    IntToBytes(msg.Length).CopyTo(data, 0);
    //    IntToBytes(3).CopyTo(data, 4);
    //    IntToBytes(3).CopyTo(data, 8);
    //    msg.CopyTo(data, 12);
    //    tcp.GetStream().Write(data, 0, data.Length);
    //    String dataArray = "";
    //    for (int i = 0; i < data.Length; i++)
    //    {
    //        dataArray += data[i];
    //    }
    //    Debug.Log("is send some data to sever");
    //    yield return null;
    //}
    /// <summary>
    /// 写数据
    /// </summary>

    void asynRec()
    {
        NetworkStream ns = tcp.GetStream();
        if (ns.CanRead)
        {
            msg = new SocketMessage();
            tempBuffer = new MemoryStream(1024 * 2);
            ns.BeginRead(recBuffer, 0, recBuffer.Length, RecCallBack, tcp);
        }


    }

    void RecCallBack(IAsyncResult iar)
    {
        TcpClient client = iar.AsyncState as TcpClient;
        NetworkStream ns = tcp.GetStream();
        int readCount = ns.EndRead(iar);
        if (readCount > 0){
            executData(readCount);
            ns.BeginRead(recBuffer, 0, recBuffer.Length, RecCallBack, tcp);
        }
    }

    void executData(int len)
    {
        Debug.Log("the data is come , data len is " + len);
        using (MemoryStream tempBuffStream = new MemoryStream(MAX_READ))
        {
            tempBuffStream.Position = 0;
            if (tempBuffer.CanWrite)
                tempBuffer.WriteTo(tempBuffStream);

            tempBuffStream.Write(recBuffer, (int)tempBuffStream.Length, len);
            tempBuffer.Dispose();
            Debug.Log("the data buffer length is " + tempBuffStream.Length);
            tempBuffStream.Position = 0;
            string ss = "";
            for (int i = 0; i < tempBuffStream.GetBuffer().Length; i++)
            {
                ss += tempBuffStream.GetBuffer()[i];
            }
            Debug.Log("======" + ss + "======");
            bufferOff = (int)tempBuffStream.Length;
            decpack(tempBuffStream);
            tempBuffer = new MemoryStream(1024 * 2);
            tempBuffer.Write(tempBuffStream.GetBuffer(), (int)tempBuffStream.Length - bufferOff, bufferOff);
            string aa = "";
            for (int i = 0; i < tempBuffer.GetBuffer().Length; i++)
            {
                aa += tempBuffer.GetBuffer()[i];
            }
            Debug.Log("tempBuff---->" + aa);
        }

    }

    void decpack(MemoryStream tempBuffStream)
    {

        if (msg.Len == 0 && bufferOff < handSize)
        {//wait next data..
            Debug.Log("1111111111>>>>" + bufferOff);
            return;
        }

        if (msg.Len == 0 && bufferOff >= handSize)
        {//set tcp hand
            byte[] d = new byte[4];
            tempBuffStream.Read(d, 0, 4);
            msg.Len = BytesToInt(d, 0);
            tempBuffStream.Read(d, 0, 4);
            msg.Rid = BytesToInt(d, 0);
            tempBuffStream.Read(d, 0, 4);
            msg.Sid = BytesToInt(d, 0);
            bufferOff -= handSize;
            Debug.Log("222222222222----" + bufferOff);
            Debug.Log(String.Format("--{0}--{1}--{2}--{3}", msg.Len, msg.Sid, msg.Rid, msg.IsZip));
        }

        if (msg.Len > 0 && bufferOff >= msg.Len){//dec data
            Debug.Log("333333333333----" + bufferOff);
            byte[] d = new byte[msg.Len];
            tempBuffStream.Read(d, 0, msg.Len);
            msg.Data = d;
            bufferOff -= msg.Len;
            Debug.Log(String.Format("--{0}--{1}--", msg.Rid, msg.Sid));
            NetworkManager.commandResponse(msg.Sid, msg.Rid, msg.IsZip, msg.Data);//finsh
            msg.clear();
            Debug.Log("44444444444----" + bufferOff);
            decpack(tempBuffStream);//check two 
        }



    }

    public void SendMessage(int serviceCode,int rid, LuaByteBuffer buffer)
    {
        MemoryStream ms = null;
        using (ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter writer = new BinaryWriter(ms);
            byte[] message = buffer.buffer;
            int msglen = message.Length;
            writer.Write(IntToBytes(msglen));
            writer.Write(IntToBytes(rid));
            writer.Write(IntToBytes(serviceCode));
            writer.Write(message);
            writer.Flush();
            if (tcp != null && tcp.Connected)
            {
                //NetworkStream stream = client.GetStream(); 
                byte[] payload = ms.ToArray();
                Debug.Log("service["+ serviceCode+"]rid["+ rid+"]");
                tcp.GetStream().BeginWrite(payload, 0, payload.Length, new AsyncCallback(OnWrite), null);
            }
            else
            {
                Debug.LogError("client.connected----->>false");
            }
        }
    }

    /// <summary>
    /// 向链接写入数据流
    /// </summary>
    void OnWrite(IAsyncResult r)
    {
        try
        {
            tcp.GetStream().EndWrite(r);
        }
        catch (Exception ex)
        {
            Debug.LogError("OnWrite--->>>" + ex.Message);
        }
    }


    //void finshData(int sid, int rid, int isZip, byte[] data)
    //{
    //    Debug.Log(String.Format("--{0}--{1}--{2}", sid, rid, isZip));
    //    string ss = "";
    //    for (int i = 0; i < data.Length; i++)
    //    {
    //        ss += data[i];
    //    }

    //    Debug.Log(ss);
    //}

    /// <summary>
    /// 注册代理
    /// </summary>
    public void OnRegister()
    {

    }

    /// <summary>
    /// 移除代理
    /// </summary>
    public void OnRemove()
    {
        tcp.Close();
        Debug.Log("tcp is close....");
    }

    void OnDestroy()
    {

    }

    ////将消息体编码成二进制流
    //private byte[] serial(string msg)
    //{
    //    using (MemoryStream ms = new MemoryStream())
    //    {
    //        Debug.Log(msg);
    //        Serializer.Serialize<string>(ms, msg);
    //        byte[] data = new byte[ms.Length];
    //        ms.Position = 0;
    //        ms.Read(data, 0, data.Length);
    //        data = System.Text.Encoding.Default.GetBytes(msg);
    //        string dataArray = "";
    //        for (int i = 0; i < data.Length; i++)
    //        {
    //            dataArray += data[i];
    //        }

    //        Debug.Log("serializable:" + dataArray);
    //        return data;
    //    }
    //}
    /// <summary]] >
    /// bytes转int
    /// </summary]] >
    /// <param name="data"]] > </param]] >
    /// <param name="offset"]] > </param]] >
    /// <returns]] > </returns]] >
    public static int BytesToInt(byte[] data, int offset)
    {
        int num = 0;
        for (int i = offset; i < offset + 4; i++)
        {
            num <<= 8;
            num |= (data[i] & 0xff);
        }
        return num;
    }

    /// <summary]] >
    /// int 转 bytes
    /// </summary]] >
    /// <param name="num"]] > </param]] >
    /// <returns]] > </returns]] >
    public static byte[] IntToBytes(int num)
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            bytes[i] = (byte)(num >> (24 - i * 8));
        }
        return bytes;
    }


    void Start()
    {
        //        connection();
        connect();
    }






































    //private TcpClient client = null;
    //private NetworkStream outStream = null;
    //private MemoryStream memStream;
    //private BinaryReader reader;


    //private byte[] byteBuffer = new byte[MAX_READ];
    //public static bool loggedIn = false;

    //// Use this for initialization
    //public SocketClient() {
    //}



    ///// <summary>
    ///// 连接服务器
    ///// </summary>
    //void ConnectServer(string host, int port) {
    //    client = null;
    //    client = new TcpClient();
    //    client.SendTimeout = 1000;
    //    client.ReceiveTimeout = 1000;
    //    client.NoDelay = true;
    //    try {
    //        client.BeginConnect(host, port, new AsyncCallback(OnConnect), null);
    //    } catch (Exception e) {
    //        Close(); Debug.LogError(e.Message);
    //    }
    //}

    ///// <summary>
    ///// 连接上服务器
    ///// </summary>
    //void OnConnect(IAsyncResult asr) {
    //    outStream = client.GetStream();
    //    client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
    //    NetworkManager.AddEvent(Protocal.Connect, new ByteBuffer());
    //}


    ///// <summary>
    ///// 读取消息
    ///// </summary>
    //void OnRead(IAsyncResult asr) {
    //    int bytesRead = 0;
    //    try {
    //        lock (client.GetStream()) {         //读取字节流到缓冲区
    //            bytesRead = client.GetStream().EndRead(asr);
    //        }
    //        Debug.LogWarning("bytesReadSize = "+ bytesRead);
    //        if (bytesRead < 1) {                //包尺寸有问题，断线处理
    //            OnDisconnected(DisType.Disconnect, "bytesRead < 1");
    //            return;
    //        }
    //        OnReceive(byteBuffer, bytesRead);   //分析数据包内容，抛给逻辑层
    //        lock (client.GetStream()) {         //分析完，再次监听服务器发过来的新消息
    //            Array.Clear(byteBuffer, 0, byteBuffer.Length);   //清空数组
    //            client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
    //        }
    //    } catch (Exception ex) {
    //        //PrintBytes();
    //        OnDisconnected(DisType.Exception, ex.Message);
    //    }
    //}

    ///// <summary>
    ///// 丢失链接
    ///// </summary>
    //void OnDisconnected(DisType dis, string msg) {
    //    Close();   //关掉客户端链接
    //    int protocal = dis == DisType.Exception ?
    //    Protocal.Exception : Protocal.Disconnect;

    //    ByteBuffer buffer = new ByteBuffer();
    //    buffer.WriteShort((ushort)protocal);
    //    NetworkManager.AddEvent(protocal, buffer);
    //    Debug.LogError("Connection was closed by the server:>" + msg + " Distype:>" + dis);
    //}

    ///// <summary>
    ///// 打印字节
    ///// </summary>
    ///// <param name="bytes"></param>
    //void PrintBytes() {
    //    string returnStr = string.Empty;
    //    for (int i = 0; i < byteBuffer.Length; i++) {
    //        returnStr += byteBuffer[i].ToString("X2");
    //    }
    //    Debug.LogError(returnStr);
    //}



    ///// <summary>
    ///// 接收到消息
    ///// </summary>
    //void OnReceive(byte[] bytes, int length) {
    //    memStream.Seek(0, SeekOrigin.End);
    //    memStream.Write(bytes, 0, length);
    //    //Reset to beginning
    //    memStream.Seek(0, SeekOrigin.Begin);
    //    while (RemainingBytes() > 2) {
    //        ushort messageLen = reader.ReadUInt16();
    //        if (RemainingBytes() >= messageLen) {
    //            MemoryStream ms = new MemoryStream();
    //            BinaryWriter writer = new BinaryWriter(ms);
    //            writer.Write(reader.ReadBytes(messageLen));
    //            ms.Seek(0, SeekOrigin.Begin);
    //            OnReceivedMessage(ms);
    //        } else {
    //            //Back up the position two bytes
    //            memStream.Position = memStream.Position - 2;
    //            break;
    //        }
    //    }
    //    //Create a new stream with any leftover bytes
    //    byte[] leftover = reader.ReadBytes((int)RemainingBytes());
    //    memStream.SetLength(0);     //Clear
    //    memStream.Write(leftover, 0, leftover.Length);
    //}

    ///// <summary>
    ///// 剩余的字节
    ///// </summary>
    //private long RemainingBytes() {
    //    return memStream.Length - memStream.Position;
    //}

    ///// <summary>
    ///// 接收到消息
    ///// </summary>
    ///// <param name="ms"></param>
    //void OnReceivedMessage(MemoryStream ms) {
    //    BinaryReader r = new BinaryReader(ms);
    //    byte[] message = r.ReadBytes((int)(ms.Length - ms.Position));
    //    //int msglen = message.Length;

    //    ByteBuffer buffer = new ByteBuffer(message);
    //    int mainId = buffer.ReadShort();
    //    NetworkManager.AddEvent(mainId, buffer);
    //}


    ///// <summary>
    ///// 会话发送
    ///// </summary>
    //void SessionSend(byte[] bytes) {
    //    WriteMessage(bytes);
    //}

    ///// <summary>
    ///// 关闭链接
    ///// </summary>
    //public void Close() {
    //    if (client != null) {
    //        if (client.Connected) client.Close();
    //        client = null;
    //    }
    //    loggedIn = false;
    //}



    ///// <summary>
    ///// 发送消息
    ///// </summary>
    //public void SendMessage(ByteBuffer buffer) {
    //    SessionSend(buffer.ToBytes());
    //    buffer.Close();
    //}
}
