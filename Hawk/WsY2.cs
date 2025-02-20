using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using WatsonWebsocket;
using static LibKaseya.Enums;

namespace KLC_Hawk {
    public class WsY2 {

        private readonly LiveConnectSession Session;
        public int PortY { get; private set; }
        public string Module { get; private set; }

        private readonly WatsonWsClient WebsocketY;
        private bool hadStarted;

        private readonly string PathAndQuery;

        public Random random;

        public WsY2(LiveConnectSession session, int portY, string PathAndQuery) {
            random = new Random(); //Used for some Lanner data spoofs

            //Type 2 - is started
            Session = session;
            PortY = portY;
            this.PathAndQuery = PathAndQuery;
            Module = PathAndQuery.Split('/')[2];

            Session.Parent.LogText("New Y2 " + PathAndQuery);

            if (PortY == 0)
                throw new Exception("Port Y does not appear to be set.");

            WebsocketY = new WatsonWsClient(new Uri("ws://127.0.0.1:" + PortY + PathAndQuery + "?Y2"));

            WebsocketY.ServerConnected += WebsocketY2_ServerConnected;
            WebsocketY.ServerDisconnected += WebsocketY2_ServerDisconnected;
            WebsocketY.MessageReceived += WebsocketY2_MessageReceived;

            WebsocketY.Start();            
        }

        public void Stop()
        {
            WebsocketY.Stop();
        }

        private void WebsocketY2_ServerConnected(object sender, EventArgs e) {
            hadStarted = true;
            Session.Parent.LogText("Y2 Connect " + Module);
            Session.Parent.LogOld(Side.LiveConnect, PortY, Module, "Y2 Socket opened - " + PathAndQuery);

            if(Module == "staticimage") {
                StaticImage.ServerConnected(this);
            } else {
                JObject jScriptReady = new JObject { ["action"] = "ScriptReady" };
                Send(jScriptReady.ToString());
            }
        }

        private void WebsocketY2_ServerDisconnected(object sender, EventArgs e) {
            Session.Parent.LogText("Y2 Disconnected " + Module);
            Session.Parent.LogOld(Side.LiveConnect, PortY, Module, "Y2 Socket closed");
        }

        private void WebsocketY2_MessageReceived(object sender, MessageReceivedEventArgs e) {
            //Session.Parent.LogText("Y2 Message");

            if (e.Data.Count == 0)
                return; //This happens when closing remote control

            Session.Parent.LogOld(Side.LiveConnect, PortY, Module, e.Data); //Slow

            KaseyaMessageTypes kmtype = KaseyaMessageTypes.Unknown;
            string messageY;
            dynamic json = null;
            if (e.Data[0] == '{') {
                messageY = Encoding.UTF8.GetString(e.Data);
                json = JsonConvert.DeserializeObject(messageY);
            } else {
                kmtype = (KaseyaMessageTypes)e.Data[0];
                byte[] bLen = new byte[4];
                e.Data.Slice(1, 4).CopyTo(bLen);
                Array.Reverse(bLen); //Endianness
                int jLen = BitConverter.ToInt32(bLen, 0);
                messageY = Encoding.UTF8.GetString(e.Data.ToArray(), 5, jLen);
                //try {
                    json = JsonConvert.DeserializeObject(messageY);
                //} catch (Exception ex) {
                //Session.Parent.LogText("============\r\nEXCEPTION Y2: " + ex.ToString() + "\r\n============\r\n" + messageY + "\r\n============\r\n" + BitConverter.ToString(e.Data).Replace("-", "") + "\r\n============");
                //return;
                //}
            }

            switch (Module) {
                case "dashboard":
                    Dashboard.MessageReceived(this, json);
                    break;

                case "staticimage":
                    StaticImage.MessageReceived(this, json);
                    break;

                case "commandshell":
                case "terminal":
                    CommandTerminal.MessageReceived(this, json);
                    break;

                case "commandshellvt100":
                    CommandPowershell.MessageReceived(this, json);
                    break;

                case "files":
                    Files.MessageReceived(this, json);
                    break;

                case "registryeditor":
                    KRegistry.MessageReceived(this, json);
                    break;

                case "events":
                    Events.MessageReceived(this, json);
                    break;

                case "services":
                    Services.MessageReceived(this, json);
                    break;

                case "processes":
                    Processes.MessageReceived(this, json);
                    break;

                case "toolbox":
                    Toolbox.MessageReceived(this, json);
                    break;

                //case "forwarding":
                //break;

                case "remotecontrol":
                    RemoteControl.MessageReceived(this, json);
                    break;

                default:
                    break;
            }
        }

        public void Send(byte[] data) {
            while (!hadStarted) {
                Session.Parent.LogText("[!] Y2 " + Module + " waiting");
                Thread.Sleep(100);
            }

            //if (!WebsocketY.Connected)
                //return;

            try {
                WebsocketY.SendAsync(data).Wait();
            } catch (Exception ex) {
                Session.Parent.LogText(ex.ToString());
            }
        }

        public void Send(string messageB) {
            while(!hadStarted) {
                Session.Parent.LogText("[!] Y2 " + Module + " waiting");
                Thread.Sleep(100);
            }

            //if (!WebsocketY.Connected)
                //return;

            try {
                WebsocketY.SendAsync(messageB).Wait();
            } catch (Exception ex) {
                Session.Parent.LogText(ex.ToString());
            }
        }
    }
}
