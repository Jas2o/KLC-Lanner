using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace KLC_Hawk {
    public static class KRegistry {

        private static Dictionary<int, string> LabelForKey = new Dictionary<int, string>() {
            {0, "HKEY_CLASSES_ROOT" },
            {1, "HKEY_CURRENT_USER" },
            {2, "HKEY_LOCAL_MACHINE" },
            {3, "HKEY_USERS" },
            {7, "HKEY_CURRENT_CONFIG" },
        };

        public static void MessageReceived(WsY2 sender, dynamic json) {
            string action = (string)(json["action"]);

            switch (action) {
                case "GetHives":
                    JArray hivesList = new JArray();
                    foreach(KeyValuePair<int, string> pair in LabelForKey) {
                        hivesList.Add(new JObject() {
                            ["type"] = "key",
                            ["name"] = pair.Key
                        });
                    }

                    JObject jHives = new JObject {
                        ["action"] = "GetHives",
                        ["success"] = true,
                        ["hives"] = hivesList
                    };

                    sender.Send(jHives.ToString());
                    break;

                case "GetSubKeys":
                    RegistryKey key = RegKeyIntToKey((int)json["hive"]);
                    string path = "";
                    foreach (JValue part in json["path"]) { //JArray
                        path = path + (string)part + "\\";
                    }
                    key = key.OpenSubKey(path);

                    JArray keysList = new JArray();
                    if (key != null) {
                        try {
                            string[] names = key.GetSubKeyNames();
                            foreach (string name in names) {
                                keysList.Add(new JObject() {
                                    ["name"] = name
                                });
                            }
                        } catch (Exception) {
                        }
                    }

                    JObject jSub = new JObject {
                        ["action"] = "GetSubKeys",
                        ["success"] = true,
                        ["keys"] = keysList
                    };

                    sender.Send(jSub.ToString());
                    break;

                case "GetKeyValues":
                    RegistryKey key2 = RegKeyIntToKey((int)json["hive"]);
                    string path2 = "";
                    foreach (JValue part in json["path"]) { //JArray
                        path2 = path2 + (string)part + "\\";
                    }
                    key2 = key2.OpenSubKey(path2);

                    JArray valList = new JArray();
                    if (key2 != null) {
                        try {
                            string[] names = key2.GetValueNames();
                            foreach (string name in names) {
                                RegistryValueKind kind = key2.GetValueKind(name);
                                dynamic regv = key2.GetValue(name);

                                if (kind == RegistryValueKind.Binary) {
                                    JArray jbin = new JArray();
                                    foreach (byte b in regv)
                                        jbin.Add(b);
                                    valList.Add(new JObject() {
                                        ["name"] = name,
                                        ["valueType"] = RegKindToString(kind),
                                        ["data"] = jbin
                                    });
                                } else {
                                    valList.Add(new JObject() {
                                        ["name"] = name,
                                        ["valueType"] = RegKindToString(kind),
                                        ["data"] = regv
                                    });
                                }
                            }
                        } catch (Exception) {
                        }
                    }

                    JObject jVal = new JObject {
                        ["action"] = "GetKeyValues",
                        ["success"] = true,
                        ["values"] = valList
                    };

                    sender.Send(jVal.ToString());
                    break;

                case "CreateKey":
                case "ModifyValue":
                case "RenameItem":
                case "DeleteItem":
                    break;

                default:
                    break;
            }

            try {
            } catch (Exception) {
            }
        }

        private static RegistryKey RegKeyIntToKey(int h) {
            string h2 = LabelForKey[h];
            //hive-int
            //path-array

            RegistryKey key = null;
            switch (h) {
                case 0:
                    key = Registry.ClassesRoot;
                    break;
                case 1:
                    key = Registry.CurrentUser;
                    break;
                case 2:
                    key = Registry.LocalMachine;
                    break;
                case 3:
                    key = Registry.Users;
                    break;
                case 7:
                    key = Registry.CurrentConfig;
                    break;
                default:
                    break;
            }

            return key;
        }

        private static string RegKindToString(RegistryValueKind kind) {
            switch(kind) {
                case RegistryValueKind.String:
                    return "REG_SZ";

                case RegistryValueKind.ExpandString:
                    return "REG_EXPAND_SZ";

                case RegistryValueKind.DWord:
                    return "REG_DWORD";

                case RegistryValueKind.QWord:
                    return "REG_QWORD";

                case RegistryValueKind.Binary:
                    return "REG_BINARY";

                default:
                    return "Unknown";
            }
        }

    }
}
