﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class HandlePlayerMsg
{
    //获取服务器列表
    public void MsgGetServerList(Player player, ProtocolBase protoBase)
    {
        Lobby.Instance.SendServerList(player);
    }

    public void MsgGetAchieve(Player player, ProtocolBase protoBase)
    {

    }
}
