using ChatSharp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Syxebot;
using System.Timers;
using Syxebot.Data;

namespace Syxebot.Syxebot
{
    internal class Bot
    {
        #region Private Fields
        private Thread _main;
        private IrcConfig _ircConfig;
        private List<string> _moderators;
        private List<string> _subscribers;
        private Dictionary<string, string> _commands;
        private IrcClient _client;
        private Dictionary<string, int> _cmdFlood;
        private bool _isConnected = false;
        private System.Timers.Timer _timer;
        private string path;


        #endregion

        #region Public Properties
        //TODO: Add/Remove Public Properties based on need.
        public IrcConfig IrcConfig
        {
            get { return _ircConfig; }
            private set { _ircConfig = value; }
        }
        public List<string> Moderators
        {
            get { return _moderators; }
            private set { _moderators = value; }
        }
        public List<string> Subscribers
        {
            get { return _subscribers; }
            private set { _subscribers = value; }
        }
        public bool IsConnected
        {
            get { return _isConnected; }
            private set { _isConnected = value; }
        }

        #endregion

        #region Constructors

        public Bot(IrcConfig ircConfig)
        {

            this._ircConfig = ircConfig;
            path = ircConfig.debugPath;
            _moderators = new List<string>();
            _subscribers = new List<string>();
            _commands = new Dictionary<string, string>();
            _cmdFlood = new Dictionary<string, int>();
            _timer = new System.Timers.Timer(1000);
            _timer.AutoReset = true;
            _timer.Elapsed += (s, e) =>
            {
                //TODO: Add timed messages and points (spawn new thread probably or Threading.Timer)
                FloodTimer();
            };
            _main = new Thread(new ThreadStart(MainThread));
            _main.IsBackground = false;
            _client = new IrcClient("irc.twitch.tv", new IrcUser(_ircConfig.nick, _ircConfig.nick, _ircConfig.password));

        }

        #endregion

        #region Public Methods
        public void Start()
        {
            //TODO: Add more threads as needed (points, timed thread, etc)
            _main.Start();
        }
        #endregion


        private void MainThread()
        {


            #region Subscribed Events

            #region bot.ChannelMessageRecieved
            _client.ChannelMessageRecieved += (s, e) =>
            {
                parseChannelMessage(new ChatMessage(_client.Channels[e.PrivateMessage.Source], e));
            };

            #endregion

            #region bot.ConnectionComplete
            _client.ConnectionComplete += (s, e) =>
            {
                log("Connected");
                _timer.Start();
                _client.SendRawMessage("twitchclient 3");
                _client.JoinChannel(_ircConfig.channel);
                _client.SendMessage("/mods", _ircConfig.channel);
            };
            #endregion

            #region bot.NetworkError
            _client.NetworkError += (s, e) =>
            {
                //TODO: log the error, setup reconnect, stuff
                _client.ConnectAsync();
            };
            #endregion

            #region bot.ChannelListRecieved
            ////channel list received. Populate our channelListOfViewers
            //bot.ChannelListRecieved += (s, e) =>
            //{
            //    //empty the list
            //    channelListOfViewers.Clear();
            //    //loop through the userlist
            //    foreach (var user in e.Channel.Users)
            //    {
            //        channelListOfViewers.Add(user.Nick);
            //    }
            //};
            #endregion

            #region bot.UserJoinedChannel
            //bot.UserJoinedChannel += (s, e) =>
            //{
            //    if (!channelListOfViewers.Contains(e.User.Nick))
            //        channelListOfViewers.Add(e.User.Nick);
            //};
            #endregion

            #region bot.UserPartedChannel
            //bot.UserPartedChannel += (s, e) =>
            //{
            //    if (channelListOfViewers.Contains(e.User.Nick))
            //        channelListOfViewers.Remove(e.User.Nick);
            //    //else Console.WriteLine("There was a problem. A user left that wasn't in the list.");
            //};
            #endregion

            #region bot.RawMessageRecieved
            //bot.RawMessageRecieved += (s, e) =>
            //{
            //    Console.WriteLine(e.Message);
            //    using (StreamWriter sw = File.AppendText(path))
            //    {
            //        sw.WriteLine(e.Message);
            //    }
            //};
            #endregion

            #region bot.MOTDRecieved
            //bot.MOTDRecieved += (s, e) =>
            //{
            //    Console.WriteLine("Message of the Day:");
            //    Console.WriteLine(e.MOTD);
            //    Console.WriteLine("End MoTD");
            //};
            #endregion

            #endregion

            _client.ConnectAsync();
            while (true) { }

        }
        #region Private Methods

        private void getSubscribers()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://api.twitch.tv/kraken");
            client.Authenticator = new SimpleAuthenticator("ClientID", _ircConfig.ClientID, "ClientSecret", _ircConfig.ClientSecret);

        }

        private void parseChannelMessage(ChatMessage cm)
        {
            #region Jtv Message
            if (cm.Nick.ToLower() == "jtv")
            {

                #region SpecialUser
                //twitch turbo, staff, or subscriber
                if (cm.Message.ToLower().StartsWith("specialuser"))
                {
                    #region Channel Subscriber
                    if (cm.MessageParams[2].ToLower() == "subscriber")
                    {
                        if (!_subscribers.Contains(cm.MessageParams[1]))
                            _subscribers.Add(cm.MessageParams[1]);
                    }
                    #endregion
                }
                #endregion

                #region UserColor
                else if (cm.Message.ToLower().StartsWith("usercolor"))
                {

                }
                #endregion

                #region EmoteSet
                else if (cm.Message.ToLower().StartsWith("emoteset"))
                {

                }
                #endregion

                #region ModeratorList
                if (cm.Message.StartsWith("The moderators of this room are: "))
                {
                    //remove "The moderators of this room are: "
                    string trim = cm.Message.Remove(0, 33);
                    //split list by ", " into array
                    string[] mods = trim.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                    //put into the channelModerators list
                    foreach (var mod in mods)
                    {
                        if (!_moderators.Contains(mod))
                            _moderators.Add(mod);
                    }

                }
                #endregion
            }
            #endregion

            #region Twitch Notify Message
            else if (cm.Nick.ToLower() == "twitchnotify")
            {
                #region Subscription Notification
                if (cm.Message.EndsWith("subscribed!"))
                {
                    if (_subscribers.Contains(cm.MessageParams[0]))
                        _subscribers.Add(cm.MessageParams[0]);
                    Console.WriteLine("{0} has just subscribed!", cm.MessageParams[0]);
                }
                #endregion
            }
            #endregion

            #region User Message
            else
            {
                #region Display Logic
                string prefix = "";
                if (isOp(_moderators, cm.Nick))
                    prefix += "@";
                if (_subscribers.Contains(cm.Nick))
                    prefix += "(Sub)";
                log(prefix + cm.Nick + ": " + cm.Message);
                #endregion

                #region Bot Logic

                #region Custom Commands

                try
                {
                    string val = "";
                    if (_commands.TryGetValue(cm.MessageParams[0], out val))
                    {

                        if (!_cmdFlood.ContainsKey(cm.MessageParams[0].ToLower()))
                        {
                            _client.SendAction(val, _ircConfig.channel);
                            _cmdFlood.Add(cm.MessageParams[0].ToLower(), _ircConfig.commandCooldown);

                        }
                    }
                }
                catch
                {

                }

                #region !addcom
                //check if enough identifiers are passed to be valid
                //TODO: add userlevel switch
                if (cm.MessageParams[0].ToLower() == "!addcom" && cm.Identifiers >= 2)
                {
                    //if command doesnt exist
                    if (!_commands.ContainsKey(cm.MessageParams[1].ToLower()))
                    {
                        string value = "";
                        for (int i = 2; i < cm.Identifiers; i++)
                        {
                            value += cm.MessageParams[i];
                            value += " ";
                        }
                        _commands.Add(cm.MessageParams[1].ToLower(), value);
                        _client.SendAction(cm.MessageParams[1].ToLower() + " command added.", _ircConfig.channel);
                    }

                }
                #endregion

                #region !delcom
                if (cm.MessageParams[0].ToLower() == "!delcom" && cm.Identifiers >= 1)
                {
                    if (_commands.ContainsKey(cm.MessageParams[1].ToLower()))
                    {
                        _commands.Remove(cm.MessageParams[1]);
                        _client.SendAction(cm.MessageParams[1].ToLower() + "command deleted.", _ircConfig.channel);
                    }
                    else
                    {
                        _client.SendAction(cm.Nick + ", " + cm.MessageParams[1].ToLower() + " command not found.", _ircConfig.channel);
                    }
                }
                #endregion

                #region !editcom
                if (cm.MessageParams[0].ToLower() == "!editcom" && cm.Identifiers >= 2)
                {
                    if (_commands.ContainsKey(cm.MessageParams[1].ToLower()))
                    {
                        string value = "";
                        for (int i = 2; i < cm.Identifiers; i++)
                        {
                            value += cm.MessageParams[i];
                            value += " ";
                        }
                        _commands[cm.MessageParams[1].ToLower()] = value;
                        _client.SendAction(cm.MessageParams[1].ToLower() + " command updated.", _ircConfig.channel);
                    }

                }
                #endregion

                #endregion

                #region Timed Messages

                //!timer add 3600 steam Come join my steam group!
                if (cm.Message.ToLower().StartsWith("!timer add") && cm.Identifiers >= 4)
                {
                    int minutes;
                    bool parseResult = Int32.TryParse(cm.MessageParams[2], out minutes);
                    if (!parseResult)
                    {
                        _client.SendAction("Invalid parameter.", IrcConfig.channel);
                    }
                    else
                    {
                        //TODO: continue work here...
                    }
                }
                #endregion

                #endregion

            }
            #endregion

        }

        private void FloodTimer()
        {
            var newDict = new Dictionary<string, int>();
            foreach (var flood in _cmdFlood)
            {
                if (flood.Value > 0)
                {
                    newDict.Add(flood.Key, flood.Value - 1);
                }
            }
            _cmdFlood = newDict;
        }

        #endregion

        #region Helpers

        bool isOp(List<string> modList, string name)
        {
            if (modList.Contains(name))
                return true;

            else
                return false;
        }

        private void log(string s)
        {
            Console.WriteLine(s);
#if(DEBUG)
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(s);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(s);
                }
            }
#endif
        }
        #endregion

    }
}
