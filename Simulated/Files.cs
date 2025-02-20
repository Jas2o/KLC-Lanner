using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace KLC_Hawk {
    public static class Files {

        public static void MessageReceived(WsY2 sender, dynamic json) {
            string action = (string)(json["action"]);

            switch (action) {
                case "GetDrives":
                    try {
                        JArray contentsList = new JArray();

                        DriveInfo[] allDrives = DriveInfo.GetDrives();
                        foreach (DriveInfo drive in allDrives) {
                            JObject jFolder = new JObject() {
                                ["name"] = drive.Name
                            };
                            contentsList.Add(jFolder);
                        }

                        JObject jList = new JObject {
                            ["action"] = "GetDrives",
                            ["success"] = true,
                            ["contentsList"] = contentsList
                        };

                        sender.Send(jList.ToString());
                    } catch (Exception) {
                    }
                    break;

                case "GetFolderContents":
                    try {
                        JArray contentsList2 = new JArray();

                        string path = "";
                        foreach (JValue part in json["path"]) { //JArray
                            path = path + (string)part + "\\";
                        }
                        //path = path.Replace("\\\\", "\\"); //Not required despite the drive letter slashes!

                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        DirectoryInfo[] dirs = directoryInfo.GetDirectories();
                        foreach (DirectoryInfo dir in dirs) {
                            JObject jFolder = new JObject() {
                                ["name"] = dir.Name,
                                ["type"] = "folder",
                                ["date"] = dir.LastWriteTime,
                                ["size"] = 0
                            };
                            contentsList2.Add(jFolder);
                        }
                        FileInfo[] files = directoryInfo.GetFiles();
                        foreach (FileInfo file in files) {
                            JObject jFolder = new JObject() {
                                ["name"] = file.Name,
                                ["type"] = "file",
                                ["date"] = file.LastWriteTime,
                                ["size"] = file.Length
                            };
                            contentsList2.Add(jFolder);
                        }

                        JObject jContents = new JObject {
                            ["action"] = "GetFolderContents",
                            ["success"] = true,
                            ["contentsList"] = contentsList2,
                            ["pathArray"] = json["path"]
                        };

                        sender.Send(jContents.ToString());
                    } catch(Exception) {
                    }
                    break;

                case "CreateFolder":
                case "RenameItem":
                case "GetFullDownloadItemList":
                case "DeleteItem":
                case "Download":
                case "Upload":
                case "Data":
                    break;

                default:
                    break;
            }
        }

    }
}
