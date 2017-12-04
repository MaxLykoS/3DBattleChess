﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.Tip;

namespace UI.Panel
{
    public class LobbyPanel : PanelBase
    {
        private Transform ServerContent;

        private Button btnHostGame;
        private Button btnRefresh;
        private Button btnJoin;
        private Button btnBackToMenu;

        private GameObject prefabServer;

        private string joinServerID;

        #region 生命周期
        public override void Init(params object[] args)
        {
            base.Init(args);
            skinPath = "Panel/MainMenu/LobbyPanel";
            layer = PanelLayer.Panel;
        }

        public override void OnShowing()
        {
            base.OnShowing();

            Transform skinTrans = skin.transform;
            btnHostGame = skinTrans.Find("BtnHostGame").GetComponent<Button>();
            btnRefresh = skinTrans.Find("BtnRefresh").GetComponent<Button>();
            btnJoin = skinTrans.Find("BtnJoin").GetComponent<Button>();
            btnBackToMenu = skinTrans.Find("BtnBackToMenu").GetComponent<Button>();
            ServerContent = skinTrans.Find("Scroll View/Viewport/Content");
            prefabServer = Resources.Load<GameObject>("Prefabs/Lobby/ServerTag");

            btnHostGame.onClick.AddListener(BtnHostGame);
            btnRefresh.onClick.AddListener(BtnRefresh);
            btnJoin.onClick.AddListener(BtnJoin);
            btnBackToMenu.onClick.AddListener(BtnBackToMenu);
        }

        public override void OnClosing()
        {
            base.OnClosing();

            NetMgr.srvConn.msgDist.DelListener("GetServerList", RecvGetServerList);    //将刷新监听的方法删除
            Debug.Log(NetMgr.srvConn.msgDist.ShowEventDicElement()); 
            NetMgr.srvConn.msgDist.ClearEventDic();

            PanelMgr.Instance.ClosePanel("UI.Panel.LobbyPanel+AchieveTip");
        }
        #endregion

        #region 按钮监听

        private void BtnHostGame()
        {
            PanelMgr.Instance.OpenPanel<HostPanel>("");

            Close();
        }

        private void BtnRefresh()
        {
            ClearServerList();
            NetMgr.srvConn.msgDist.AddListener("GetServerList",RecvGetServerList);   //监听刷新事件

            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetServerList");
            NetMgr.srvConn.Send(protocol);
        }

        private void BtnJoin()
        {
            if (joinServerID == "")
            {
                PanelMgr.Instance.OpenPanel<WarningTip>("","请先选择要加入的服务器!");
                return;
            }

            //判断房间是否还能ping通

            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("JoinServer");
            protocol.AddString(joinServerID);
            NetMgr.srvConn.Send(protocol, RecvJoinServer);

        }

        private void BtnBackToMenu()
        {
            PanelMgr.Instance.OpenPanel<MenuButtonsPanel>("");

            Close();
        }

        private void BtnGetAchieve(string hostName)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetAchieve");
            protocol.AddString(hostName);
            NetMgr.srvConn.Send(protocol, RecvGetAchieve);
        }
        #endregion

        #region 网络监听
        private void RecvGetServerList(ProtocolBase protocol)
        {
            //解析协议
            ProtocolBytes proto = (ProtocolBytes)protocol;
            int start = 0;
            proto.GetString(start, ref start);   //“GetServerList”
            string protoDesc = proto.GetString(start, ref start);   //服务器描述
            string protoHostName = proto.GetString(start, ref start);//服务器房主名字
            int protoServerStatus = proto.GetInt(start, ref start);  //服务器是准备还是战斗

            joinServerID = protoHostName;

            CreateServerTag(protoDesc, protoHostName, protoServerStatus);
        }

        private void RecvGetAchieve(ProtocolBase protocol)
        {
            ProtocolBytes proto = (ProtocolBytes)protocol;
            int start = 0;
            proto.GetString(start, ref start);   //"GetAchieve"
            string protoHostName = proto.GetString(start, ref start);   //服务器房主名字
            string protoHostMapName = proto.GetString(start, ref start);  //服务器地图
            int protoWinTimes = proto.GetInt(start, ref start);   //胜利次数
            int protoFailTimes = proto.GetInt(start, ref start);   //失败次数

            PanelMgr.Instance.ClosePanel("UI.Panel.LobbyPanel+AchieveTip");
            PanelMgr.Instance.OpenPanel<AchieveTip>("", protoHostName, protoHostMapName, protoWinTimes.ToString(), protoFailTimes.ToString());
        }

        private void RecvJoinServer(ProtocolBase protocol)
        {
            ProtocolBytes proto = (ProtocolBytes)protocol;
            int start = 0;
            proto.GetString(start, ref start);  //"JoinServer"
            int playerCount = proto.GetInt(start, ref start);   //0代表失败，1代表成功

            if (playerCount == -1)
            {
                PanelMgr.Instance.OpenPanel<WarningTip>("", "加入服务器失败！");
            }
            else
            {
                for (int i = 0; i < playerCount; i++)
                {
                    昨晚上到这里,准备写读取该房间内玩家的信息，详细参数看服务器代码
                }

                PanelMgr.Instance.OpenPanel<RoomPanel>("");
                Close();
            }
        }
        #endregion

        #region 其他辅助函数
        private void ClearServerList()
        {
            NetMgr.srvConn.msgDist.DelListener("GetServerList", RecvGetServerList);
            for (int i = 0; i < ServerContent.childCount; i++)
            {
                Destroy(ServerContent.GetChild(i).gameObject);
            }
        }

        private void CreateServerTag(string serverDesc, string hostName,int serverStatus)
        {
            Transform tr= Instantiate(prefabServer, ServerContent).transform;

            Text textServerDesc = tr.Find("TextServerDesc").GetComponent<Text>();
            Text textHostName = tr.Find("TextHostName").GetComponent<Text>();
            Text textServerStatus = tr.Find("TextServerStatus").GetComponent<Text>();
            Text textServerPing = tr.Find("TextServerPing").GetComponent<Text>();

            textServerDesc.text = serverDesc;
            textHostName.text = hostName;

            textServerStatus.text = serverStatus == 1 ? "战斗中" : "等待中";
            tr.gameObject.GetComponent<Button>().onClick.AddListener(delegate() { BtnGetAchieve(hostName); });
        }
        #endregion

        class AchieveTip : PanelBase
        {
            private Text textHostName;
            private Text textHostMapName;
            private Text textWinTimes;
            private Text textFailTimes;

            private string hostName = "";
            private string hostMapName = "";
            private string winTimes = "";
            private string failTimes = "";

            #region 声明周期
            public override void Init(params object[] args)
            {
                base.Init(args);

                skinPath = "Tip/Lobby/AchieveTip";
                layer = PanelLayer.Tip;

                hostName = (string)args[0];
                hostMapName = (string)args[1];
                winTimes = (string)args[2];
                failTimes = (string)args[3];
            }

            public override void OnShowing()
            {
                base.OnShowing();

                Transform skinTrans = skin.transform;
                textHostName = skinTrans.Find("TextHostName").GetComponent<Text>();
                textHostMapName = skinTrans.Find("TextHostMapName").GetComponent<Text>();
                textWinTimes = skinTrans.Find("TextWinTimes").GetComponent<Text>();
                textFailTimes = skinTrans.Find("TextFailTimes").GetComponent<Text>();

                textHostName.text = hostName;
                textHostMapName.text = hostMapName;
                textWinTimes.text = winTimes;
                textFailTimes.text = failTimes;
            }
            #endregion
        }
    }
}
