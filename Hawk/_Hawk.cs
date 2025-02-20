using System;
using static LibKaseya.Enums;

namespace KLC_Hawk {

    public class Hawk {

        private WindowMain windowMain;

        private NamedPipeListener pipeListener;
        private LiveConnectSession lastSession;

        //private FormShark formSharkCapture;
        //private FormOverlay formOverlay;

        private Action<string> actionLog;
        public AsyncProducerConsumerQueue<string> queueLog;

        public Hawk(WindowMain window) {
            windowMain = window;

            actionLog = new Action<string>((x) => {
                windowMain.Dispatcher.Invoke((Action)delegate {
                    windowMain.txtLog.AppendText(x + "\r\n");

                    if (windowMain.IsActive) {
                        windowMain.txtLog.Focus();
                        windowMain.txtLog.CaretIndex = windowMain.txtLog.Text.Length;
                        windowMain.txtLog.ScrollToEnd();
                    }
                });
            });
            queueLog = new AsyncProducerConsumerQueue<string>(actionLog);

            pipeListener = new NamedPipeListener("KLCMITM", true);
            pipeListener.MessageReceived += PipeListener_MessageReceived;
            pipeListener.Error += PipeListener_Error;
            pipeListener.Start();

            //--

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 2) {
                lastSession = LiveConnectSession.Create(int.Parse(args[2]), this);
            } else {
            }
        }

        public void ActivateWindow() {
            windowMain.Dispatcher.Invoke((Action)delegate {
                windowMain.Activate();
            });
        }

        private void PipeListener_MessageReceived(object sender, NamedPipeListenerMessageReceivedEventArgs<string> e) {
            lastSession = LiveConnectSession.Create(int.Parse(e.Message), this);
        }

        private void PipeListener_Error(object sender, NamedPipeListenerErrorEventArgs e) {
            string error = string.Format("Pipe Error ({0}): {1}", e.ErrorType, e.Exception.ToString());
            LogText(error);
        }

        public void LogText(string message, string filterable = "") {
            queueLog.Produce(message);
        }

        public void LogOld(Side side, int port, string module, ArraySegment<byte> message) {
            if (windowMain.WindowSharkCapture == null || !windowMain.WindowSharkCapture.IsVisible || !windowMain.WindowSharkCapture.Shark.AllowCapture)
                return;

            windowMain.WindowSharkCapture.Shark.AddCapture(side, port, module, message);
        }

        public void LogOld(Side side, int port, string module, string message) {
            if (windowMain.WindowSharkCapture == null || !windowMain.WindowSharkCapture.IsVisible || !windowMain.WindowSharkCapture.Shark.AllowCapture)
            return;

            windowMain.WindowSharkCapture.Shark.AddCapture(side, port, module, message);
        }

        public void DropY()
        {
            foreach (LiveConnectSession session in LiveConnectSession.listSession)
            {
                foreach (WsY2 y in session.listY2Client)
                {
                    y.Stop();
                }

                foreach (WsY1 y in session.listY1Client)
                {
                    y.Stop();
                }
            }
        }

        public void DropZ()
        {
            foreach (LiveConnectSession session in LiveConnectSession.listSession)
            {
                session.WebsocketZ.Stop();
            }
        }

        public string GetWiresharkFiltersLiveConnect() {
            if (lastSession == null)
                return "";
            return lastSession.GetWiresharkFilterKLC();
        }

        public void Replay(CaptureMsg msg) {
            if (msg.Side != Side.AdminEndPoint || lastSession == null)
                return;

            bool sent = false;

            if (msg.Module == "controladmin") {
                lastSession.WebsocketZ.Send(msg.Text);
                sent = true;
            } else if (msg.Module == "controlagent") {
                foreach (WsY1 y in lastSession.listY1Client) {
                    if (y.Module == msg.Module) {
                        y.Send(msg.Data.ToArray());
                        sent = true;
                        break;
                    }
                }
            } else {
                foreach (WsY2 y in lastSession.listY2Client) {
                    if (y.Module == msg.Module) {
                        if(msg.Type == Datatype.String || msg.Type == Datatype.JSON)
                            y.Send(msg.Text);
                        else
                            y.Send(msg.Data.ToArray());
                        sent = true;
                        break;
                    }
                }
            }

            if (!sent)
                Console.WriteLine(msg.ToString());
        }

    }
}
