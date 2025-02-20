using Newtonsoft.Json;
using System;
using System.Text;
using WatsonWebsocket;
using static LibKaseya.Enums;

namespace KLC_Hawk {
    public class WsY1 {

        private readonly LiveConnectSession Session;
        public int PortY { get; private set; }
        public string Module { get; private set; }

        private readonly WatsonWsClient WebsocketY;

        public WsY1(LiveConnectSession session, int portY) {
            //Type1 - is not started straight away
            Session = session;
            PortY = portY;
            Module = "controlagent";

            if (PortY == 0)
                throw new Exception("Port Y does not appear to be set.");

            WebsocketY = new WatsonWsClient(new Uri("ws://127.0.0.1:" + PortY + "/control/agent"));

            WebsocketY.ServerConnected += WebsocketY1_ServerConnected;
            WebsocketY.ServerDisconnected += WebsocketY1_ServerDisconnected;
            WebsocketY.MessageReceived += WebsocketY1_MessageReceived;
            WebsocketY.Start();
        }

        public void Stop()
        {
            WebsocketY.Stop();
        }

        private void WebsocketY1_ServerConnected(object sender, EventArgs e) {
            Session.Parent.LogText("Y1 Connected " + Module);
            Session.Parent.LogOld(Side.LiveConnect, PortY, Module, "Socket opened");
        }

        private void WebsocketY1_ServerDisconnected(object sender, EventArgs e) {
            Session.Parent.LogText("Y1 Disconnected " + Module);
            Session.Parent.LogOld(Side.LiveConnect, PortY, Module, "Socket closed");
        }

        private void WebsocketY1_MessageReceived(object sender, MessageReceivedEventArgs e) {
            //string messageY = Encoding.UTF8.GetString(e.Data);

            if (e.MessageType == System.Net.WebSockets.WebSocketMessageType.Text) {
                string messageY = Encoding.UTF8.GetString(e.Data);
                dynamic json = JsonConvert.DeserializeObject(messageY);
                Session.Parent.LogOld(Side.LiveConnect, PortY, Module, messageY);
                //Client.Send(messageY);

                if (json["type"] != null) {
                    string type = (string)(json["type"]);
                    if (type == "Task") {
                        string nextmodule = (string)(json["data"]["moduleId"]);
                        nextmodule = nextmodule.ToLower();

                        WsY2 y2 = new WsY2(Session, PortY, "/app/" + nextmodule);
                        Session.listY2Client.Add(y2);
                    } else if(type == "StaticImage") {
                        WsY2 y2 = new WsY2(Session, PortY, "/app/staticimage");
                        Session.listY2Client.Add(y2);
                    } else if (type == "RemoteControl") {
                        WsY2 y2 = new WsY2(Session, PortY, "/app/remotecontrol/lanner");
                        Session.listY2Client.Add(y2);
                    } else if (type == "RDP_StateRequest") {
                        //Forwarding
                    } else {
                        Console.WriteLine();
                    }
                } else {
                    Console.WriteLine();
                }

            } else if (e.MessageType == System.Net.WebSockets.WebSocketMessageType.Binary) {
                //Client.Send(e.Data);
                Session.Parent.LogOld(Side.LiveConnect, PortY, Module, e.Data);
            }
        }

        public void Send(byte[] data) {
            try {
                WebsocketY.SendAsync(data).Wait();
            } catch (Exception ex) {
                Session.Parent.LogText(ex.ToString());
            }
        }

        public void Send(string messageB) {
            try {
                WebsocketY.SendAsync(messageB).Wait();
            } catch (Exception ex) {
                Session.Parent.LogText(ex.ToString());
            }
        }
    }
}
