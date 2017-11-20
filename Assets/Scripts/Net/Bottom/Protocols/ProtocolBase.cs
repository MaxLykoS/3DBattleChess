﻿//协议基类
public class ProtocolBase
{       
    /// <summary>
    /// 解码器，解码readbuff中从start开始length字节
    /// </summary>
    /// <param name="readbuff"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public virtual ProtocolBase Decode(byte[] readbuff, int start, int length)
    {
        return new ProtocolBase();
    }

    /// <summary>
    /// 编码器
    /// </summary>
    /// <returns></returns>
    public virtual byte[] Encode()
    {
        return new byte[] { };
    }

    /// <summary>
    /// 协议名称，用于消息分发
    /// </summary>
    /// <returns></returns>
    public virtual string GetName()
    {
        return "";
    }

    /// <summary>
    /// 描述
    /// </summary>
    /// <returns></returns>
    public virtual string GetDesc()
    {
        return "";
    }
}
