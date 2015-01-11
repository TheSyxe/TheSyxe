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
using Newtonsoft.Json;
using Syxebot.Twitch;

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
        //string identifier, string message, int seconds
        private Dictionary<string, KeyValuePair<string, UInt64>> _timedMessages;
        private Dictionary<string, KeyValuePair<string, UInt64>> _disabledTimedMessages;
        private Dictionary<string, int> _userPoints;
        private System.Timers.Timer _timer;
        private string path;
        private UInt64 _ticks;
        private List<string> _chatters;
        private TwitchAPICaller twitchAPICaller;
        private bool _isChannelLive;

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
        #endregion

        #region Constructors

        public Bot(IrcConfig ircConfig)
        {
            _isChannelLive = false;
            twitchAPICaller = new TwitchAPICaller("nitemarephoenix");
            _ircConfig = ircConfig;
            path = ircConfig.debugPath;
            _moderators = new List<string>();
            _subscribers = new List<string>();
            _commands = new Dictionary<string, string>();
            _cmdFlood = new Dictionary<string, int>();
            _timedMessages = new Dictionary<string, KeyValuePair<string, UInt64>>();
            _disabledTimedMessages = new Dictionary<string, KeyValuePair<string, ulong>>();
            _userPoints = new Dictionary<string, int>();
            _chatters = new List<string>();
            _timer = new System.Timers.Timer(1000);
            _timer.AutoReset = true;
            _ticks = 0;
            _timer.Elapsed += (s, e) =>
            {
                _ticks++;
                pointsClock();
                checkStreamStatus();
                //Console.WriteLine(_ticks.ToString());
                //log("ticks" + _ticks.ToString());
                TimedMessageTimer();
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

        #region Private Methods

        private void MainThread()
        {
            #region Subscribed Events

            #region bot.ChannelMessageRecieved
            _client.ChannelMessageRecieved += (s, e) =>
            {
                parseMessage(new ChatMessage(_client.Channels[e.PrivateMessage.Source], e));
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

        private void doParseUserMessage(ChatMessage cm)
        {
            #region Display Logic
            string prefix = "";
            if (isOp(_moderators, cm.Nick))
                prefix += "@";
            if (_subscribers.Contains(cm.Nick))
                prefix += "(Sub)";
            log(prefix + cm.Nick + ": " + cm.Message);
            #endregion

            switch (cm.MessageParams[0])
            {
                case "!cmd":
                case "!command":
                    doParseCommand(cm);
                    break;

                case "!timed":
                case "!timer":
                    doParseTimerCommand(cm);
                    break;

                default:
                    #region Softcoded or no command
                    try
                    {
                        string val = "";
                        if (_commands.TryGetValue(cm.MessageParams[0], out val))
                        {

                            if (!_cmdFlood.ContainsKey(cm.MessageParams[0].ToLower()))
                            {
                                _client.SendAction(val, cm.Channel.Name);
                                _cmdFlood.Add(cm.MessageParams[0].ToLower(), _ircConfig.commandCooldown);

                            }
                        }
                    }
                    catch
                    {

                    }
                    #endregion
                    break;
            }
        }

        private void doParseCommand(ChatMessage cm)
        {
            //TODO: add userlevel switch
            if (cm.Identifiers >= 3 && (cm.MessageParams[1].ToLower() == "add" || cm.MessageParams[1].ToLower() == "edit"))
            {
                if (!_commands.ContainsKey(cm.MessageParams[2].ToLower()))
                {
                    string value = "";
                    for (int i = 3; i < cm.Identifiers; i++)
                    {
                        value += cm.MessageParams[i];
                        value += " ";
                    }
                    if (cm.MessageParams[1].ToLower() == "add")
                    {
                        _commands.Add(cm.MessageParams[2].ToLower(), value);
                        _client.SendAction(cm.MessageParams[2].ToLower() + " command added.", cm.Channel.Name);
                    }
                    else
                    {
                        _commands[cm.MessageParams[1].ToLower()] = value;
                        _client.SendAction(cm.MessageParams[2].ToLower() + " command updated.", cm.Channel.Name);
                    }
                }
                else
                {
                    _client.SendAction(cm.Nick + ", " + cm.MessageParams[1].ToLower() + " command not found.", cm.Channel.Name);
                }
            }
            else if (cm.MessageParams[1].ToLower() == "del" || cm.MessageParams[1].ToLower() == "rem" || cm.MessageParams[1].ToLower() == "remove" || cm.MessageParams[1].ToLower() == "delete")
            {
                if (_commands.ContainsKey(cm.MessageParams[1].ToLower()))
                {
                    _commands.Remove(cm.MessageParams[1]);
                    _client.SendAction(cm.MessageParams[1].ToLower() + "command deleted.", cm.Channel.Name);
                }
                else
                {
                    _client.SendAction(cm.Nick + ", " + cm.MessageParams[1].ToLower() + " command not found.", cm.Channel.Name);
                }
            }
        }

        private void doParseTimerCommand(ChatMessage cm)
        {
            //!timer add 3600 steam Come join my steam group!
            if (cm.MessageParams[1].ToLower() == "add" && cm.Identifiers >= 4)
            {
                UInt64 seconds;
                bool parseResult = UInt64.TryParse(cm.MessageParams[2], out seconds);
                if (!parseResult)
                {
                    _client.SendAction("Invalid parameter.", cm.Channel.Name);
                }
                else if (!_timedMessages.ContainsKey(cm.MessageParams[3].ToLower()))
                {
                    string timedMessage = "";

                    for (int i = 4; i < cm.Identifiers; i++)
                    {
                        timedMessage += cm.MessageParams[i];
                        timedMessage += " ";
                    }
                    _timedMessages.Add(cm.MessageParams[3].ToLower(), new KeyValuePair<string, UInt64>(timedMessage, seconds));

                    //TODO: better message to send
                    _client.SendAction("Timed message added", cm.Channel.Name);
                }
                else
                {
                    //TODO: better message to send
                    _client.SendAction("Already exists.", cm.Channel.Name);
                }
            }
            //!timer delete steam
            else if (cm.MessageParams[1].ToLower() == "del" || cm.MessageParams[1].ToLower() == "rem" || cm.MessageParams[1].ToLower() == "remove" || cm.MessageParams[1].ToLower() == "delete")
            {
                if (_timedMessages.ContainsKey(cm.MessageParams[2].ToLower()))
                {
                    _timedMessages.Remove(cm.MessageParams[2].ToLower());
                    //TODO: better message to send
                    _client.SendAction("Timed message deleted", cm.Channel.Name);
                }
                //TODO: better message to send
                else
                    _client.SendAction("No timed message found", cm.Channel.Name);
            }
            //!timer stop steam
            else if (cm.MessageParams[1].ToLower() == "stop")
            {
                if (_timedMessages.ContainsKey(cm.MessageParams[2].ToLower()))
                {
                    KeyValuePair<string, UInt64> kvp;
                    if (_timedMessages.TryGetValue(cm.MessageParams[2].ToLower(), out kvp))
                    {
                        _disabledTimedMessages.Add(cm.MessageParams[2].ToLower(), kvp);
                        _timedMessages.Remove(cm.MessageParams[2].ToLower());

                        //TODO: better message to send
                        _client.SendAction("Timed message stopped", cm.Channel.Name);

                    }
                }
            }
            //!timer start steam
            else if (cm.MessageParams[1].ToLower() == "start")
            {
                if (_disabledTimedMessages.ContainsKey(cm.MessageParams[2].ToLower()))
                {
                    KeyValuePair<string, UInt64> kvp;
                    if (_disabledTimedMessages.TryGetValue(cm.MessageParams[2].ToLower(), out kvp))
                    {
                        _timedMessages.Add(cm.MessageParams[2].ToLower(), kvp);
                        _disabledTimedMessages.Remove(cm.MessageParams[2].ToLower());

                        //TODO: better message to send
                        _client.SendAction("Timed message resumed", cm.Channel.Name);

                    }
                }
            }
        }

        private void pointsClock()
        {
            if (_isChannelLive)
            {
                UInt64 r = _ticks % _ircConfig.PayoutInterval;
                if (r == 0)
                {
                    Console.WriteLine("payout!");
                    RestClient rc = new RestClient("https://tmi.twitch.tv/group/user/");
                    RestRequest rr = new RestRequest("{channelName}/chatters", Method.GET);
                    rr.AddUrlSegment("channelName", _ircConfig.channel.Trim('#'));
                    RestResponse resp = (RestResponse)rc.Execute(rr);
                    string content = resp.Content;
                    log("CONTENT = " + content);
                    Chatters chatters = JsonConvert.DeserializeObject<Chatters>(content);
                    #region foreach chatter in chatters.chatters
                    foreach (var chatter in chatters.chatters.admins)
                    {
                        addUserPoints(chatter, 1);
                    }
                    foreach (var chatter in chatters.chatters.global_mods)
                    {
                        addUserPoints(chatter, 1);
                    }
                    foreach (var chatter in chatters.chatters.moderators)
                    {
                        addUserPoints(chatter, 1);
                    }
                    foreach (var chatter in chatters.chatters.staff)
                    {
                        addUserPoints(chatter, 1);
                    }
                    foreach (var chatter in chatters.chatters.viewers)
                    {
                        addUserPoints(chatter, 1);
                    }
                    #endregion
                }
            }
        }

        private void checkStreamStatus()
        {
            var r = _ticks % 60;
            if (r == 0)
            {
                _isChannelLive = twitchAPICaller.isChannelLive();
            }
        }



        #region Name Parse Methods
        //Find out the name of who sent it
        private void parseMessage(ChatMessage cm)
        {
            switch (cm.Nick.ToLower())
            {
                case "jtv":
                    doParseJtvMessage(cm);
                    break;
                case "twitchnotify":
                    doParseTwitchNotifyMessage(cm);
                    break;
                default:
                    doParseUserMessage(cm);
                    break;
            }
        }
        //twitchnotify sent it
        private void doParseTwitchNotifyMessage(ChatMessage cm)
        {
            #region Subscription Notification
            if (cm.Message.EndsWith("subscribed!"))
            {
                if (_subscribers.Contains(cm.MessageParams[0]))
                    _subscribers.Add(cm.MessageParams[0]);
                //_client.SendAction(cm.MessageParams[0] + " has just subscribed!", cm.Channel.Name);
                log(cm.MessageParams[0] + " has just subscribed!");
            }
            #endregion
        }
        //jtv sent it
        private void doParseJtvMessage(ChatMessage cm)
        {
            #region Jtv Message

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

            #endregion
        }
        #endregion

        private void TimedMessageTimer()
        {
            foreach (var timedmsg in _timedMessages)
            {
                var r = _ticks % timedmsg.Value.Value;
                if (r == 0)
                {
                    _client.SendAction(timedmsg.Value.Key, _ircConfig.channel);
                    log("timed message fired");

                }
            }
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

        private void getSubscribers()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://api.twitch.tv/kraken");
            client.Authenticator = new SimpleAuthenticator("ClientID", _ircConfig.ClientID, "ClientSecret", _ircConfig.ClientSecret);

        }

        #endregion

        #region Helpers

        private void addUserPoints(string chatter, int p)
        {
            Console.WriteLine("adding points to " + chatter);
            int q;
            if (_userPoints.TryGetValue(chatter, out q))
                _userPoints[chatter] = q + p;
            else
                _userPoints.Add(chatter, p);
        }

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
