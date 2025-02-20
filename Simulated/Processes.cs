using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace KLC_Hawk {
    public static class Processes {

        public static void MessageReceived(WsY2 sender, dynamic json) {
            string action = (string)(json["action"]);

            switch (action) {
                case "ListProcesses":
                    JArray contentsList = new JArray();

                    Process[] allProcesses = Process.GetProcesses();
                    foreach (Process process in allProcesses) {
                        if (process.Id == 0 && process.ProcessName == "Idle")
                            continue;

                        JObject jProcess = new JObject() {
                            ["PID"] = process.Id,
                            ["DisplayName"] = process.ProcessName,
                            ["UserName"] = "",
                            ["Memory"] = process.PrivateMemorySize64,
                            ["CPU"] = (sender.random.NextDouble() * 25.0).ToString(),
                            ["GpuUtilization"] = (sender.random.NextDouble() * 5.0).ToString(),
                            ["DiskUtilization"] = (Math.Clamp(sender.random.NextInt64(-1048576, 1048576), 0, 1048576)).ToString(),
                            ["Type"] = ""
                        };
                        contentsList.Add(jProcess);
                    }

                    JObject jList = new JObject {
                        ["action"] = "ListProcesses",
                        ["success"] = true,
                        ["PID"] = null,
                        ["displayName"] = null,
                        ["contentsList"] = contentsList
                    };

                    sender.Send(jList.ToString());
                    break;

                case "EndProcess":
                    break;

                default:
                    break;
            }
        }

    }
}
