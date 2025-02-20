using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace KLC_Hawk {
    public static class Events {

        public static void MessageReceived(WsY2 sender, dynamic json) {
            

            string action = (string)(json["action"]);

            switch (action) {
                case "GetLogTypes":
                    EventLogSession eventSession = new EventLogSession();
                    string[] logNames = eventSession.GetLogNames().ToArray();
                    JArray jLogNames = new JArray(logNames);
                    //JArray jLogNames = new JArray() {"Application", "Security", "Setup", "System"};

                    JObject jGetLogTypes = new JObject {
                        ["action"] = "GetLogTypes",
                        //["errors"] = null,
                        ["logs"] = jLogNames
                    };

                    sender.Send(jGetLogTypes.ToString());

                    break;

                case "GetEvents":
                case "setLogType":
                case "SetLogType": //capital S
                    string logType = (string)(json["logType"]);
                    int numEvents = (int)(json["numEvents"]);
                    string direction = (string)(json["direction"]); //NewerFirst

                    JArray jEventsCollection = new JArray();
                    EventLog getLog = new EventLog(logType);
                    foreach (EventLogEntry entry in getLog.Entries) {
                        if (jEventsCollection.Count >= numEvents)
                            break;

                        JObject jEvent = new JObject() {
                            ["sourceName"] = entry.Source,
                            ["id"] = entry.InstanceId,
                            ["eventType"] = (int)entry.EntryType,
                            ["logType"] = logType,
                            ["category"] = entry.Category,
                            ["eventMessage"] = entry.Message,
                            ["eventGeneratedTime"] = entry.TimeGenerated,
                            ["recordNumber"] = entry.CategoryNumber,
                            ["user"] = entry.UserName,
                            ["computer"] = entry.MachineName
                        };
                        jEventsCollection.Add(jEvent);
                    }

                    JObject jEvents = new JObject {
                        ["action"] = action, //The client app will clear list if GetEvents, and append for others
                        //["errors"] = null,
                        ["events"] = jEventsCollection
                    };
                    sender.Send(jEvents.ToString());

                    break;

                default:
                    break;
            }
        }

    }
}
