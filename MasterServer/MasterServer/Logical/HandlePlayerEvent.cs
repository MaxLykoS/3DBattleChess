﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HandlePlayerEvent
{
    //上线
    public void OnLogin(Player player)
    {

    }

    //下线
    public void OnLogout(Player player)
    {
        LobbyMgr.Instance.DelServer(player.id);
    }
}
