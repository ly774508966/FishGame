using LuaFramework;
using LuaInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SessionData
{

    /// <summary>
    /// 唯一ID，标识消息的编号
    /// 如果为0则标识消息为主动推送数据，需要根据ServiceCode寻找回调函数
    /// </summary>
    private int _id;
    public int Id
    {
        get{ return _id; }
        set{ _id = value; }
    }

    /// <summary>
    /// 消息服务ID，跟服务器交互的请求标识
    /// </summary>
    private int _serviceCode;
    public int ServiceCode {
        get { return _serviceCode; }
        set { _serviceCode = value; }
    }

    /// <summary>
    /// 回调函数
    /// </summary>
    private LuaFunction _luaFunction;
    public LuaFunction LuaFunction
    {
        get
        {
            return _luaFunction;
        }

        set
        {
            _luaFunction = value;
        }
    }
    
    /// <summary>
    /// 回调函数中需要用到的临时数据
    /// </summary>
    private object[] _args;
    public object[] Args
    {
        get
        {
            return _args;
        }

        set
        {
            _args = value;
        }
    }


    /// <summary>
    /// 服务器返回的byte数据
    /// </summary>
    private ByteBuffer _byteBuffer;
    public ByteBuffer ByteBuffer
    {
        get
        {
            return _byteBuffer;
        }

        set
        {
            _byteBuffer = value;
        }
    }

  

    public void clear()
    {
        this.Args = null;
        this.ByteBuffer = null;
        this.LuaFunction = null;
        this.Id = 0;
        this.ServiceCode = 0;
    }







}

