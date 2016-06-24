using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace LuaFramework {
    public class NetworkManager : Manager {
        private SocketClient socket;
        private static int _rid;
        static Dictionary<int, SessionData> sEvents = new Dictionary<int, SessionData>();
        static Dictionary<int, SessionData> sListen = new Dictionary<int, SessionData>();
        private static int POOL_COUNT = 100;
        static SessionData[] sessionDataPool = new SessionData[POOL_COUNT];

        SocketClient SocketClient {
            get { 
                if (socket == null)
                    socket = new SocketClient();
                return socket;                    
            }
        }

        public static int Rid
        {
            get
            {
                if (_rid > 100000) { _rid = 0; }
                return _rid + 1;
            }
        }

        void Awake() {
            Init();
        }

        void Init() {
            for(int i = 0; i < POOL_COUNT; i++)
            {
                sessionDataPool[i] = new SessionData();
            }
            SocketClient.OnRegister();
        }

        public void OnInit() {
            CallMethod("Start");
        }

        public void Unload() {
            CallMethod("Unload");
        }

        /// <summary>
        /// ִ��Lua����
        /// </summary>
        public object[] CallMethod(string func, params object[] args) {
            return Util.CallMethod("Network", func, args);
        }

        ///----------------------��ӱ������͵ļ���--------------------------------------------------------------
        public void AddEvent(int serviceCode, LuaFunction function,object[] args) {
            SessionData sd = new SessionData();
            sd.LuaFunction = function;
            sd.Args = args;
            sd.ServiceCode = serviceCode;
            sListen.Add(serviceCode, sd);
        }

        public void sendMessage(int serviceCode, LuaFunction function, object[] functionArgs, LuaByteBuffer buffer)
        {
            int id = Rid;
            SessionData sd = sessionDataPool[id % POOL_COUNT];
            sd.clear();
            sd.Id = id;
            sd.ServiceCode = serviceCode;
            sd.LuaFunction = function;
            sd.Args = functionArgs;
            sEvents.Add(id, sd);
            SocketClient.SendMessage(serviceCode, id, buffer);
        }

        public static void commandResponse(int sid, int rid, int isZip, byte[] data)
        {
            SessionData sd = null;
            if (rid == 0)//�������Ͱ�
            {
                sd = sListen[sid];
            }
            else
            {//��Ӧ��
                sd = sEvents[rid];
            }
            sd.LuaFunction.Call(sd);
        }

        /// <summary>
        /// ����Command�����ﲻ����ķ���˭��
        /// </summary>
        void Update() {
            //if (sEvents.Count > 0) {
            //    while (sEvents.Count > 0) {
            //        KeyValuePair<int, SessionData> _event = sEvents.Dequeue();
            //        facade.SendMessageCommand(NotiConst.DISPATCH_MESSAGE, _event);
            //    }
            //}
        }

        /// <summary>
        /// ������������
        /// </summary>
        public void SendConnect() {
            SocketClient.SendConnect();
        }

        ///// <summary>
        ///// ����SOCKET��Ϣ
        ///// </summary>
        //public void SendMessage(ByteBuffer buffer) {
        //    SocketClient.SendMessage(buffer);
        //}

        /// <summary>
        /// ��������
        /// </summary>
        void OnDestroy() {
            SocketClient.OnRemove();
            Debug.Log("~NetworkManager was destroy");
        }
    }
}