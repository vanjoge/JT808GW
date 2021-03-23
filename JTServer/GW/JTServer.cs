using SQ.Base;
using SQ.Base.GW;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Network;
using System.Linq;

namespace JTServer.GW
{
    public class JTServer : Server<JTClient, JTCheji>
    {
        public override void Start()
        {
            this.mServer = new TCPServer("0.0.0.0", task.Config.PortClient);

            mServer.ChannelConnect += OnConnected;
            mServer.ChannelDispose += OnDisposed;
            if (mServer.Start())
            {
                Log.WriteLog4("启动808网关服务完成 0.0.0.0:" + task.Config.PortClient);
            }
            else
            {
                Log.WriteLog4("启动808监听失败 0.0.0.0:" + task.Config.PortClient);
            }
        }

        public override void Stop()
        {
            try
            {
                mServer.Stop();
                Log.WriteLog4("808网关服务已停止");
            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("JTTCPServer.StopServer", ex);
            }
        }

        protected override string GetClientKey()
        {
            return task.getClientKey();
        }




        public JTServer(JTTask task)
        {
            this.task = task;
        }


        public JTTask task;

        public TCPServer mServer;

        void OnConnected(object sender, ChannelConnectArg arg)
        {
            try
            {
                JTClient CnUser;

                Log.WriteLog4("[" + arg.Channel.RemoteHost + ":" + arg.Channel.RemotePort + "] Connected");
                CnUser = new JTClient(task, arg.Channel);
                CnUser.CNTTime = CnUser.LastDataTime = CnUser.LastSuccessTime = DateTime.Now;


                ClientPoolsAdd(CnUser);

                arg.Channel.Tag = CnUser;
                task.OnCJAdd(CnUser);
                arg.Channel.DataReceive = CnUser.OnReceive;

                arg.Channel.StartReceiveAsync();

            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("ChannelConnected", ex);
            }
        }

        void OnDisposed(object sender, ChannelDisposeArg arg)
        {
            try
            {
                Log.WriteLog4("[" + arg.Channel.RemoteHost + ":" + arg.Channel.RemotePort + "] Disposed");
                if (arg.Channel.Tag != null)
                {
                    var mcj = (JTClient)arg.Channel.Tag;
                    if (!mcj.IsDispose)
                    {
                        RemoveClient(mcj, LogOutReason.ShutDown);
                    }
                }
                arg.Channel.Dispose();
            }
            catch (Exception exChannelDisposed)
            {
                Log.WriteLog4Ex("ChannelDisposed", exChannelDisposed);
            }
        }
        #region 业务逻辑
        protected override void OnCJRemove(JTClient mCj)
        {
            try
            {
                task.OnCJRemove(mCj);

            }
            catch (Exception ex)
            {
                Log.WriteLog4Ex("OnCJRemove", ex);
            }
        }

        #endregion

    }
}
