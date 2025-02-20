using Newtonsoft.Json.Linq;

namespace KLC_Hawk {
    public static class Dashboard {

        public static void MessageReceived(WsY2 sender, dynamic json) {
            string action = (string)(json["action"]);

            switch (action) {
                case "StartDashboardData":
                    //I think this expects all the responses
                    GetVolumes(sender);
                    GetCpuRam(sender);
                    break;

                case "GetCpuRam":
                    GetCpuRam(sender);
                    break;

                case "GetTopEvents":
                    //EventsData
                    //Not implemented in Finch
                    break;

                case "GetTopProcesses":
                    //TopProcData
                    //Not implemented in Finch
                    break;

                case "GetVolumes":
                    GetVolumes(sender);
                    break;

                default:
                    break;
            };
        }

        private static void GetCpuRam(WsY2 sender) {
            //CpuRamData
            int ramPercentage = sender.random.Next(10, 100);
            double cpuPercentage = 100 * sender.random.NextDouble();

            JObject jCpuRamData = new JObject {
                ["action"] = "CpuRamData",
                ["data"] = new JObject {
                    ["ram"] = ramPercentage,
                    ["cpu"] = cpuPercentage
                },
                ["errors"] = new JArray()
            };

            sender.Send(jCpuRamData.ToString());
        }

        private static void GetVolumes(WsY2 sender) {
            //VolumesData
            long total = 254956666880;
            long free = sender.random.NextInt64(0, 254956666880);

            JObject jVolumesData = new JObject {
                ["action"] = "VolumesData",
                ["data"] = new JArray() {
                    new JObject {
                        ["label"] = "C:\\",
                        ["free"] = free,
                        ["total"] = total,
                        ["type"] = "Fixed"
                    }
                },
                ["errors"] = new JArray()
            };

            sender.Send(jVolumesData.ToString());
        }

    }
}
