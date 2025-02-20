using Newtonsoft.Json.Linq;
using System.ServiceProcess;

namespace KLC_Hawk {
    public static class Services {

        public static void MessageReceived(WsY2 sender, dynamic json) {

            string action = (string)(json["action"]);

            switch (action) {
                case "ListServices":
                    ServiceController[] services = ServiceController.GetServices();

                    JArray contentsList = new JArray();
                    foreach(ServiceController service in services) {
                        JObject jService = new JObject() {
                            ["ServiceStatus"] = (int)service.Status, //https://docs.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicecontrollerstatus?view=dotnet-plat-ext-5.0
                            ["DisplayName"] = service.DisplayName,
                            ["ServiceName"] = service.ServiceName,
                            ["Description"] = "",
                            ["StartupType"] = service.StartType.ToString(), //Automatic, Disabled, On demand, (blank)
                            ["StartName"] = "",
                        };
                        contentsList.Add(jService);
                    }

                    JObject jList = new JObject {
                        ["action"] = "ListServices",
                        ["success"] = true,
                        ["serviceName"] = null,
                        ["displayName"] = null,
                        ["contentsList"] = contentsList
                    };

                    sender.Send(jList.ToString());

                    break;

                case "StartService":
                case "StopService":
                case "RestartService":
                case "SetStartupType":
                    break;

                default:
                    break;
            }
        }

    }
}
