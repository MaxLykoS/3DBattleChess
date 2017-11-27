﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Lobby
{
    //单例
    public static Lobby Instance;

    public Lobby()
    {
        Instance = this;
    }

    private Dictionary<string, LobbyServer> serverDic = new Dictionary<string, LobbyServer>();

    //根据名字获取LobbyServer
    private LobbyServer GetLobbyServer(string hostName)
    {
        LobbyServer server=null;
        if (serverDic.TryGetValue(hostName, out server))
        {
            return server;
        }
        return null;
    }

    //添加服务器
    public void AddServer(string hostName)
    {
        lock (serverDic)
        {
            LobbyServer server = new LobbyServer(hostName);
            serverDic[hostName] = server;
        }
    }

    //删除服务器
    public void DelServer(string hostName)
    {
        lock (serverDic)
        {
            LobbyServer server = GetLobbyServer(hostName);
            if (server == null)
            {
                return;
            }
            else
            {
                serverDic.Remove(hostName);
            }
        }
    }

    //发送服务器列表
    public void SendServerList(Player player)
    {
        foreach (LobbyServer server in serverDic.Values)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetServerList");
            protocol.AddString(server.ServerDesc);
            protocol.AddString(server.HostName);
            protocol.AddString(server.HostOpponent);
            protocol.AddInt(server.WinTimes);
            protocol.AddInt(server.FailTimes);
            foreach (string serverDesc in server.SpectList)
            {
                protocol.AddString(serverDesc);
            }
            player.Send(protocol);
        }
    }
}
