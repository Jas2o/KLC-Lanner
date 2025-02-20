﻿using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace KLC_Hawk {
    public class LiveConnectSession {

        public static List<LiveConnectSession> listSession = new List<LiveConnectSession>();

        public Hawk Parent;
        public int PortZ; //LiveConnect
        public WsZ WebsocketZ;
        //public WsA WebsocketA;
        public List<WsY1> listY1Client = new List<WsY1>();
        public List<WsY2> listY2Client = new List<WsY2>();
        //public List<WsB> listBsocket = new List<WsB>();
        public bool IsMac; //Detected by the name of the first listed screen.

        //--

        public LiveConnectSession(int portZ, Hawk parent) {
            Parent = parent;
            PortZ = portZ;

            WebsocketZ = new WsZ(this, portZ);
        }

        public void Restart()
        {
            //While this does make the disconnect screen appear, it also doesn't reconnect.
            //foreach (WsB b in listBsocket) b.Stop();
            foreach (WsY2 y2 in listY2Client) y2.Stop();
            foreach (WsY1 y1 in listY1Client) y1.Stop();
            
            //Apparently we should be able to reuse Z/A
            //WebsocketA.Stop();
            //WebsocketZ.Restart();
        }

        public string GetWiresharkFilterKLC() {
            string filter = string.Format("(tcp.srcport == {0}) || (tcp.dstport == {0})", PortZ);

            foreach (WsY1 y1 in listY1Client)
                filter += string.Format(" || (tcp.srcport == {0}) || (tcp.dstport == {0})", y1.PortY);

            return filter;
        }

        //--

        public static LiveConnectSession Create(int portZ, Hawk parent) {
            LiveConnectSession session = new LiveConnectSession(portZ, parent);
            listSession.Add(session);
            parent.LogText("LCS Create/Add");
            return session;
        }

        public static int GetNewPort() {
            TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();

            return port;
        }

    }
}
