using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace tabletjoy
{
    class WsListener
    {
        public HttpServer wssv;
        public vjoyHandler vjoy;

        public void startServer(int id)
        {
            wssv = new HttpServer (id);

            //wssv.DocumentRootPath = ConfigurationManager.AppSettings["DocumentRootPath"];
            wssv.DocumentRootPath = "./";

            // Set the HTTP GET request event.
            wssv.OnGet += (sender, e) => {
                var req = e.Request;
                var res = e.Response;

                var path = req.RawUrl;

                if (path == "/")
                    path += "index.html";

                byte[] contents;

                if (!e.TryReadFile(path, out contents))
                {
                    res.StatusCode = (int)HttpStatusCode.NotFound;

                    return;
                }

                if (path.EndsWith(".html"))
                {
                    res.ContentType = "text/html";
                    res.ContentEncoding = Encoding.UTF8;
                }
                else if (path.EndsWith(".js"))
                {
                    res.ContentType = "application/javascript";
                    res.ContentEncoding = Encoding.UTF8;
                }

                res.ContentLength64 = contents.LongLength;

                res.Close(contents, true);
            };

            wssv.AddWebSocketService<Control>("/Control", s => { s.Vjoy = vjoy; });
            wssv.AddWebSocketService<Input>("/Input", s => { s.Vjoy = vjoy; });

            wssv.Start();
        }
    }

    class Control : WebSocketBehavior {

        private vjoyHandler _vjoy;
        public vjoyHandler Vjoy
        {
            get
            {
                return _vjoy;
            }
            set
            {
                _vjoy = value;
            }
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            System.Console.WriteLine(e.Data);
            string[] split = e.Data.Split('|');

            if (split[0] == "devicelist")
            {
                List<VjdStat> stats = _vjoy.getAllDeviceStatus();

                string ret = "";
                for (int i = 0; i< stats.Count; i++)
                {
                    VjdStat stat = stats[i];
                    ret += ((int)stat).ToString();

                    if (stat == VjdStat.VJD_STAT_OWN || stat == VjdStat.VJD_STAT_FREE)
                    {
                        int num = _vjoy.GetVJDButtonNumber((uint)i + 1);
                        ret += "_" + num.ToString();
                    }
                    ret += ",";
                }
                System.Console.WriteLine(ret);
                Send("devicelist|" + ret);

                return;
            }

            else if (split[0] == "grabdevice")
            {
                string[] ids = split[1].Split(',');

                foreach (string id in ids)
                {
                    uint uid = UInt32.Parse(id);
                    _vjoy.grabDevice(uid);
                    int nbtn = _vjoy.GetVJDButtonNumber(uid);
                    Send("grabdevice|" + id + "|" + nbtn.ToString());
                }
                return;
            }

            else if (split[0] == "releasedevice")
            {
                string[] ids = split[1].Split(',');
                foreach (string id in ids)
                {
                    _vjoy.RelinquishVJD(UInt32.Parse(id));
                    Send("releasedevice|" + id);
                }
                return;
            }
        }
    }


    class Input : WebSocketBehavior
    {
        private vjoyHandler _vjoy;

        public vjoyHandler Vjoy
        {
            get
            {
                return _vjoy;
            }
            set
            {
                _vjoy = value;
            }
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            System.Console.WriteLine(e.Data);
            string[] split = e.Data.Split('|');

            uint vid = UInt32.Parse(split[0]);
            uint btn = UInt32.Parse(split[1]);
            bool status = split[2] == "1" ? true : false;

            _vjoy.processButtonInput(vid, btn, status);
        }
    }
}
