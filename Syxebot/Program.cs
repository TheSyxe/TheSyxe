using ChatSharp;
using System;
using System.IO;


//[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Syxebot.Syxebot
{
    //Link Regex
    //string linksPattern0 = ".*http://.*"
    //string linksPattern1 = ".*https://.*";
    //string linksPattern2 = ".*[-A-Za-z0-9]+\\s?(\\.|\\(dot\\))\\s?(ac|ad|ae|aero|af|ag|ai|al|am|an|ao|aq|ar|as|asia|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|biz|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cat|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|com|coop|cr|cu|cv|cw|cx|cy|cz|de|dj|dk|dm|do|dz|ec|edu|ee|eg|er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gov|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|im|in|info|int|io|iq|ir|is|it|je|jm|jo|jobs|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|mil|mk|ml|mm|mn|mo|mobi|mp|mq|mr|ms|mt|mu|museum|mv|mw|mx|my|mz|na|name|nc|ne|net|nf|ng|ni|nl|no|np|nr|nu|nz|om|org|pa|pe|pf|pg|ph|pk|pl|pm|pn|post|pr|pro|ps|pt|pw|py|qa|re|ro|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|sk|sl|sm|sn|so|sr|st|su|sv|sx|sy|sz|tc|td|tel|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|travel|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|va|vc|ve|vg|vi|vn|vu|wf|ws|xxx|ye|yt|za|zm|zw)(\\W|$).*";
    //string linksPattern3 = ".*(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\\s+|:|/|$).*";

    class Program
    {
        //TODO: incorporate logging
        //    private static readonly log4net.ILog log = log4net.LogManager.GetLogger
        //(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Bot b = new Bot(new IrcConfig());
            b.Start();
            Console.ReadLine();
            //string path = @"D:\KSP\temp.txt";
            //if (!File.Exists(path))
            //{
            //    // Create a file to write to. 
            //    using (StreamWriter sw = File.CreateText(path))
            //    {
            //        sw.WriteLine("TOP");
            //    }
            //}
            //IrcConfig config = new IrcConfig();
            //IrcClient client = new IrcClient(config.server, new IrcUser(config.nick, config.nick, config.password));
            //client.ConnectionComplete += (s, e) =>
            //{
            //    client.JoinChannel(config.channel);
            //};
            //client.ChannelMessageRecieved += (s, e) =>
            //    {
            //        log("COMMAND = " + e.IrcMessage.Command);
            //        for (int i = 0; i < e.IrcMessage.Parameters.Length; i++)
            //        {
            //            log("Param " + i + " = " + e.IrcMessage.Parameters[i]);
            //        }
                    

            //    };
            //client.ConnectAsync();
            //while (true) {

            //}

        }


        static void log(string s)
        {
            string path = @"D:\KSP\temp.txt";

            Console.WriteLine(s);
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(s);
            }
        }

    }

}

