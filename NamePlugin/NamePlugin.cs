using System;
using Spectrum.API;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Configuration;
using System.Collections.Generic;

namespace NamePlugin {
    public class Entry : IPlugin {
        public string FriendlyName => "Name Colorizer";
        public string Author => "timawesomeness";
        public string Contact => "Steam/Discord: timawesomeness";
        public APILevel CompatibleAPILevel => APILevel.UltraViolet;

        private string username;

        private Settings settings = new Settings(typeof(Entry));
        private ClientLogic cl;
        private Random random = new Random();

        public void Initialize(IManager manager) {
            // Generate settings if they do not exist
            if (!settings.ContainsKey("Randomize Color")) {
                Console.WriteLine("Name Colorizer configuration not found. Generating new configuration.");

                settings.Add("Randomize Color", false);
                
                settings.Add("Use Single Color", false);
                settings.Add("Single Color Hue", 180);
                settings.Add("Single Color Saturation", 100);
                settings.Add("Single Color Value", 100);
                
                settings.Add("Gradient Hue Start", 0);
                settings.Add("Gradient Hue End", 360);
                settings.Add("Gradient Saturation", 100);
                settings.Add("Gradient Value", 100);

                settings.Save();
            }

            // Get game's ClientLogic and the player's username.
            cl = G.Sys.PlayerManager_.GetComponent<ClientLogic>();
            username = G.Sys.GameManager_.GetOnlineProfileName(0);

            // Update the name to be colored when the chat window is open, not colored otherwise.
            Events.ChatWindow.ChatVisibilityChanged.Subscribe(data => {
                if (!data.isShowing_) {
                    SetName(username);
                } else {
                    UpdateName();
                }
            });

            // Reload the colors from file
            Spectrum.API.Game.Network.Chat.MessageSent += (sender, args) => {
                if (args.Author == username && args.Message.Contains("!updatename"))
                    settings = new Settings(typeof(Entry));
            };
        }

        /// <summary>
        /// Update the player's name to be colored.
        /// </summary>
        private void UpdateName() {
            if (settings.GetItem<bool>("Randomize Color")) { 
                if (settings.GetItem<bool>("Use Single Color")) {
                    SetName(ColorizeFlat(username, (float)random.NextDouble(), Math.Max((float)random.NextDouble(), .33f), 1f));
                } else {
                    SetName(ColorizeGradient(username, (float)random.NextDouble(), (float)random.NextDouble(), Math.Max((float)random.NextDouble(), .33f), 1f));
                }
            } else {
                if (settings.GetItem<bool>("Use Single Color")) {
                    SetName(ColorizeFlat(username,
                        ConvertHueToSingle(settings.GetItem<int>("Single Color Hue")),
                        ConvertPercentToSingle(settings.GetItem<int>("Single Color Saturation")),
                        ConvertPercentToSingle(settings.GetItem<int>("Single Color Value"))));
                } else {
                    SetName(ColorizeGradient(username,
                        ConvertHueToSingle(settings.GetItem<int>("Gradient Hue Start")),
                        ConvertHueToSingle(settings.GetItem<int>("Gradient Hue End")),
                        ConvertPercentToSingle(settings.GetItem<int>("Gradient Saturation")),
                        ConvertPercentToSingle(settings.GetItem<int>("Gradient Value"))));
                }
            }
        }

        /// <summary>
        /// Converts a hue wheel value (0-360) to a float (0-1) because that's what ColorEx uses.
        /// </summary>
        /// <param name="hue">Hue to convert</param>
        /// <returns>Float from 0 to 1 that represents the hue.</returns>
        private float ConvertHueToSingle(int hue) {
            return Convert.ToSingle(hue / 360f);
        }

        /// <summary>
        /// Converts a percent (0-100) to a float from 0-1 because that's what ColorEx uses.
        /// </summary>
        /// <param name="percent">Percent to convert</param>
        /// <returns>Float from 0 to 1 that represents the hue.</returns>
        private float ConvertPercentToSingle(int percent) {
            return Convert.ToSingle(percent / 100f);
        }

        /// <summary>
        /// Sets the player's name to a string
        /// </summary>
        /// <param name="name">String to set name to</param>
        private void SetName(string name) {
            cl.GetLocalPlayerInfo().GetType().GetField("username_", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(cl.GetLocalPlayerInfo(), name);
        }

        /// <summary>
        /// Colorize a string based on a gradient on the hue wheel.
        /// </summary>
        /// <param name="str">String to colorize</param>
        /// <param name="hueStart">Hue to start at</param>
        /// <param name="hueEnd">Hue to end at</param>
        /// <param name="sat">Saturation of the color</param>
        /// <param name="val">Value of the color</param>
        /// <returns>Colorized string</returns>
        private string ColorizeGradient(string str, float hueStart, float hueEnd, float sat, float val) {
            string newStr = "";
            for (int i = 0; i < str.Length; i++) {
                newStr += "[" + ColorEx.ColorToHexNGUI(new ColorHSB(((hueEnd - hueStart) / str.Length) * i + hueStart, sat, val, 1f).ToColor()) + "]" + str[i] + "[-]";
            }
            return newStr;
        }

        /// <summary>
        /// Colorize a string based on a single color on the hue wheel.
        /// </summary>
        /// <param name="str">String to colorize</param>
        /// <param name="hue">Hue of the color</param>
        /// <param name="sat">Saturation of the color</param>
        /// <param name="val">Value of the color</param>
        /// <returns>Colorized string</returns>
        private string ColorizeFlat(string str, float hue, float sat, float val) {
            return "[" + ColorEx.ColorToHexNGUI(new ColorHSB(hue, sat, val, 1f).ToColor()) + "]" + str + "[-]";
        }

        public void Shutdown() {

        }
    }
}
    