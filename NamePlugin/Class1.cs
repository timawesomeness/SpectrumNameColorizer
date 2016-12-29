using System;
using Spectrum.API;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;

namespace NamePlugin {
    public class Entry : IPlugin {
        public string FriendlyName => "Name Colorizer";
        public string Author => "timawesomeness";
        public string Contact => "moo@timawesomeness.com";
        public APILevel CompatibleAPILevel => APILevel.InfraRed;

        private string username;
        private float[] colorData;

        private bool alreadySet = false;
        private ClientLogic cl;

        public void Initialize(IManager manager) {
            FileSystem fs = new FileSystem(typeof(Entry));
            try {
                System.IO.StreamReader sr = new System.IO.StreamReader(fs.OpenFile("name.txt"));
                colorData = new float[] { Convert.ToSingle(sr.ReadLine().Split(new string[] { "hue start: " }, StringSplitOptions.None)[1]) / 360f, Convert.ToSingle(sr.ReadLine().Split(new string[] { "hue end: " }, StringSplitOptions.None)[1]) / 360f, Convert.ToSingle(sr.ReadLine().Split(new string[] { "saturation: " }, StringSplitOptions.None)[1]) / 100, Convert.ToSingle(sr.ReadLine().Split(new string[] { "value: " }, StringSplitOptions.None)[1]) / 100 };
                sr.Dispose();
            } catch (Exception ex) {
                fs.CreateFile("name.txt");
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs.OpenFile("name.txt"));
                sw.WriteLine("hue start: 0");
                sw.WriteLine("hue end: 360");
                sw.WriteLine("saturation: 100");
                sw.WriteLine("value: 100");
                sw.WriteLine("Instructions: Change hue start and hue end to a hue value between 0 and 360. Change saturation and value to a value between 0 and 100.");
                sw.Dispose();

                colorData = new float[] { 0f, 1f, 1f, 1f };
            }

            cl = G.Sys.PlayerManager_.GetComponent<ClientLogic>();
            username = G.Sys.GameManager_.GetOnlineProfileName(0);

            Events.ChatWindow.ChatVisibilityChanged.Subscribe(data => {
                if (!data.isShowing_) {
                    cl.GetLocalPlayerInfo().GetType().GetField("username_", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(cl.GetLocalPlayerInfo(), username);
                    alreadySet = false;
                } else if (data.isShowing_) {
                    UpdateName();
                }
            });
            Events.Network.DisconnectedFromServer.Subscribe(data => {
                alreadySet = false;
            });
        }

        private void UpdateName() {
            if (!alreadySet) {
                try {
                    cl.GetLocalPlayerInfo().GetType().GetField("username_", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(cl.GetLocalPlayerInfo(), Colorize(username, colorData[0], colorData[1], colorData[2], colorData[3]));
                    alreadySet = true;
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private string Colorize(string str, float hueStart, float hueEnd, float sat, float val) {
            string newStr = "";
            for (int i = 0; i < str.Length; i++) {
                newStr += "[" + ColorEx.ColorToHexNGUI(new ColorHSB(((hueEnd - hueStart) / str.Length) * i + hueStart, sat, val, 1f).ToColor()) + "]" + str[i] + "[-]";
            }
            return newStr;
        }

        public void Shutdown() {

        }
    }
}
    