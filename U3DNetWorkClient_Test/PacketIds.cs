using System;
using System.Collections.Generic;
using System.Text;

namespace IRobotQ.GameNet.Network {
    /// <summary>
    /// 客户端 所有包的 ID
    /// </summary>
    internal static class CS_PacketIds {
        //static Dictionary<ushort, string> MSG_STR;
        internal static void BuildMsgString() {
            Type t = typeof(CS_PacketIds);
            System.Reflection.FieldInfo[] buf = t.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            //MSG_STR = new Dictionary<ushort, string>();
            foreach (var v in buf) {
                if (v.Name.StartsWith("MSG")) {
                    //MSG_STR.Add((ushort)v.GetRawConstantValue(), v.Name);
                    m_IdStringMap[(ushort)v.GetRawConstantValue()] = v.Name;
                }

            }
        }
        static bool m_inited = false;
        private static string[] m_IdStringMap = new string[65536];

        public static bool Exists(ushort id) {
            if (m_inited == false) {
                BuildMsgString();
                m_inited = true;
            }
            return !string.IsNullOrEmpty(m_IdStringMap[id]);
        }
        public static string ToString(ushort id) {
            if (m_inited == false) {
                BuildMsgString();
                m_inited = true;
            }
            string ret = m_IdStringMap[id];
            if (ret == null) {
                ret = string.Empty;
            }
            return ret;
        }
        //public static string ToString(ushort packetId) {
        //    if (MSG_STR == null) {
        //        BuildMsgString();
        //    }
        //    string ret = string.Empty;
        //    if (MSG_STR.TryGetValue(packetId, out ret)) {
        //        return ret;
        //    }
        //    else {
        //        return "不存在" + packetId.ToString() + "对应的字符串";
        //    }
        //}

        //登陆类的1 
        public const byte CMD_Login = 1;
        public const byte CMD_Login_GetSoftUrlVer = 3;
        public const byte CMD_Login_GetSysInfo = 5;
        public const byte CMD_Login_HallList = 7;//登陆时下载好


        public const ushort MSG_Login = (ushort)(PacketType.Login << 8) + CMD_Login;
        public const ushort MSG_Login_GetSoftUrlVer = (ushort)(PacketType.Login << 8) + CMD_Login_GetSoftUrlVer;
        public const ushort MSG_Login_GetSysInfo = (ushort)(PacketType.Login << 8) + CMD_Login_GetSysInfo;
        public const ushort MSG_Login_HallList = (ushort)(PacketType.Login << 8) + CMD_Login_HallList;//登陆成功后下载好

        //大厅类的2
        public const byte CMD_Hall_HeatBeat = 1;
        public const byte CMD_Hall_PlayerCount = 3;
        public const byte CMD_Hall_GetRoomList = 5;
        public const byte CMD_Hall_GetRoomMessionURL = 7;


        public const ushort MSG_Hall_HeatBeat = (ushort)(PacketType.Hall << 8) + CMD_Hall_HeatBeat;
        public const ushort MSG_Hall_PlayerCount = (ushort)(PacketType.Hall << 8) + CMD_Hall_PlayerCount;
        public const ushort MSG_Hall_GetRoomList = (ushort)(PacketType.Hall << 8) + CMD_Hall_GetRoomList;
        public const ushort MSG_Hall_GetRoomMessionURL = (ushort)(PacketType.Hall << 8) + CMD_Hall_GetRoomMessionURL;
        //房间-场地类的3     
        public const byte CMD_Room_Enter = 1;
        public const byte CMD_Room_Leave = 3;
        public const byte CMD_Room_Chat = 4;
        public const byte CMD_Room_GetRoomUsersCount = 5;

        public const byte CMD_Room_GetUserList = 7;
        public const byte CMD_Room_GetBFList = 9;

        public const byte CMD_BF_ReadyTimeOut = 11;
        public const byte CMD_BF_UserReady = 13;

        public const byte CMD_BF_TestResult = 15;
        public const byte CMD_BF_HostLANIPInfo = 17;
        public const byte CMD_BF_Enter = 19;
        public const byte CMD_BF_Leave = 21;
        public const byte CMD_BF_SomeonDrop = 22;

        public const byte CMD_BC_Room_BFState = 29;


        public const byte CMD_BF_GameStart = 50;
        public const byte CMD_BF_GameSccore = 51;
        public const byte CMD_BF_GameOver = 52;
        public const byte CMD_BF_GameLoad = 53;
        public const byte CMD_BF_GameStartLoad = 54;
        public const byte CMD_BF_GameOverLittle = 55;
        public const byte CMD_Room_GameSaveInfo = 56;
        public const byte CMD_QueryGameSave = 57;
        //public const byte CMD_BF_GameData = 55;
        //public const byte CMD_BF_GameNotify = 56;



        public const byte CMD_Room_GetUserBaseInfoById = 31;
        public const byte CMD_Room_GetUserBaseInfoByName = 33;
        public const ushort MSG_Room_GetUserBaseInfoById = (ushort)(PacketType.Room << 8) + CMD_Room_GetUserBaseInfoById;
        public const ushort MSG_Room_GetUserBaseInfoByName = (ushort)(PacketType.Room << 8) + CMD_Room_GetUserBaseInfoByName;


        public const ushort MSG_Room_Enter = (ushort)(PacketType.Room << 8) + CMD_Room_Enter;
        public const ushort MSG_Room_Leave = (ushort)(PacketType.Room << 8) + CMD_Room_Leave;

        public const ushort MSG_Room_GetRoomUsersCount = (ushort)(PacketType.Room << 8) + CMD_Room_GetRoomUsersCount;

        public const ushort MSG_Room_GetUserList = (ushort)(PacketType.Room << 8) + CMD_Room_GetUserList;
        public const ushort MSG_Room_GetBFList = (ushort)(PacketType.Room << 8) + CMD_Room_GetBFList;
        //public const ushort MSG_BF_ReadyTimeOut = (ushort)(PacketType.Room << 8) + CMD_BF_ReadyTimeOut;
        public const ushort MSG_BF_UserReady = (ushort)(PacketType.Room << 8) + CMD_BF_UserReady;

        public const ushort MSG_BF_SomeoneDrop = (ushort)(PacketType.Room << 8) + CMD_BF_SomeonDrop;
        public const ushort MSG_BF_TestResult = (ushort)(PacketType.Room << 8) + CMD_BF_TestResult;
        public const ushort MSG_BF_HostLANIPInfo = (ushort)(PacketType.Room << 8) + CMD_BF_HostLANIPInfo;
        public const ushort MSG_BF_Leave = (ushort)(PacketType.Room << 8) + CMD_BF_Leave;
        public const ushort MSG_BF_Enter = (ushort)(PacketType.Room << 8) + CMD_BF_Enter;

        //public const ushort MSG_BF_GameStart = (ushort)(PacketType.Room << 8) + CMD_BF_GameStart;
        public const ushort MSG_BF_GameSccore = (ushort)(PacketType.Room << 8) + CMD_BF_GameSccore;
        public const ushort MSG_BF_GameOver = (ushort)(PacketType.Room << 8) + CMD_BF_GameOver;
        public const ushort MSG_BF_GameLoad = (ushort)(PacketType.Room << 8) + CMD_BF_GameLoad;
        public const ushort MSG_BF_GameStartLoad = (ushort)(PacketType.Room << 8) + CMD_BF_GameStartLoad;
        public const ushort MSG_BF_GameOverLittle = (ushort)(PacketType.Room << 8) + CMD_BF_GameOverLittle;
        public const ushort MSG_Room_GameSaveInfo = (ushort)(PacketType.Room << 8) + CMD_Room_GameSaveInfo;
        public const ushort MSG_QueyGameSave = (ushort)(PacketType.Room << 8) + CMD_QueryGameSave;
        //public const ushort MSG_BF_GameData = (ushort)(PacketType.Room << 8) + CMD_BF_GameData;
        //public const ushort MSG_BF_GameNotify = (ushort)(PacketType.Room << 8) + CMD_BF_GameNotify;


        public const ushort MSG_BC_Room_BFState = (ushort)(PacketType.Room << 8) + CMD_BC_Room_BFState;
        //聊天类的4
        public const ushort MSG_Chat = (ushort)(PacketType.Room << 8) + CMD_Room_Chat;

        //用户类的5

        public const byte CMD_User_UserStatusChange = 5;
        public const byte CMD_User_UserUnLine = 7;
        public const byte CMD_User_LoginOut = 9;
        public const byte CMD_User_Register = 11;
        public const byte CMD_User_GetMyPlayedMession = 13;
        public const byte CMD_User_PcPerformance = 15;


        public const ushort MSG_User_UserStatusChange = (ushort)(PacketType.UserInfo << 8) + CMD_User_UserStatusChange;
        public const ushort MSG_User_UserUnLine = (ushort)(PacketType.UserInfo << 8) + CMD_User_UserUnLine;
        public const ushort MSG_User_LoginOut = (ushort)(PacketType.UserInfo << 8) + CMD_User_LoginOut;
        public const ushort MSG_User_Register = (ushort)(PacketType.UserInfo << 8) + CMD_User_Register;
        public const ushort MSG_User_GetMyPlayedMession = (ushort)(PacketType.UserInfo << 8) + CMD_User_GetMyPlayedMession;    //登陆成功后下载好
        //用户PC性能
        public const ushort MSG_User_UserPCPerformance = (ushort)(PacketType.UserInfo << 8) + CMD_User_PcPerformance;


        #region 客户端同步
        //客户端同步类命令
        public const byte CMD_Game_SynPose = 1;
        public const byte CMD_Game_Data = 100;
        public const byte CMD_Game_Notify = 101;
        //客户端同步类消息 6
        public const ushort MSG_Game_SynPose = (ushort)(PacketType.Game << 8) + CMD_Game_SynPose;
        public const ushort MSG_Game_GameData = (ushort)(PacketType.Game << 8) + CMD_Game_Data;
        public const ushort MSG_Game_GameNotify = (ushort)(PacketType.Game << 8) + CMD_Game_Notify;
        #endregion

    }
    /// <summary>
    /// 服务器端 所有包ID
    /// </summary>
    internal static class SC_PacketIds {
        //static Dictionary<ushort, string> MSG_STR;

        internal static void BuildMsgString() {
            Type t = typeof(SC_PacketIds);
            System.Reflection.FieldInfo[] buf = t.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            // MSG_STR = new Dictionary<ushort, string>();
            foreach (var v in buf) {
                if (v.Name.StartsWith("MSG")) {
                    m_IdStringMap[(ushort)v.GetRawConstantValue()] = v.Name;
                }
            }
        }

        static bool m_inited = false;
        private static string[] m_IdStringMap = new string[65536];
        public static bool Exists(ushort id) {
            if (m_inited == false) {
                BuildMsgString();
                m_inited = true;
            }
            return !string.IsNullOrEmpty(m_IdStringMap[id]);
        }
        public static string ToString(ushort id) {
            if (m_inited == false) {
                BuildMsgString();
                m_inited = true;
            }
            string ret = m_IdStringMap[id];
            if (ret == null) {
                ret = string.Empty;
            }
            return ret;
        }

        //public static string ToString(ushort packetId) {
        //    if (MSG_STR == null) {
        //        BuildMsgString();
        //    }
        //   // Debug.Assert(MSG_STR != null, "尚未调用BuildMsgString");
        //    string ret = string.Empty;
        //    if (MSG_STR.TryGetValue(packetId, out ret)) {
        //        return ret;
        //    }
        //    else {
        //        return "不存在" + packetId.ToString() + "对应的字符串";
        //    }
        //}

        #region 登陆类的1
        public const byte CMD_LoginRet = 2;
        public const byte CMD_Login_GetSoftUrlVerRet = 4;
        public const byte CMD_Login_GetSysInfoRet = 6;
        public const byte CMD_Login_HallListRet = 8;
        public const byte CMD_ReLogin = 10;

        public const ushort MSG_LoginRet = (ushort)(SC_PacketType.Login << 8) + CMD_LoginRet;
        public const ushort MSG_Login_GetSoftUrlVerRet = (ushort)(SC_PacketType.Login << 8) + CMD_Login_GetSoftUrlVerRet;

        public const ushort MSG_Login_GetSysInfoRet = (ushort)(SC_PacketType.Login << 8) + CMD_Login_GetSysInfoRet;
        public const ushort MSG_Login_HallListRet = (ushort)(SC_PacketType.Login << 8) + CMD_Login_HallListRet;//登陆成功后下载好

        public const ushort MSG_Login_UserReLogin = (ushort)(SC_PacketType.Login << 8) + CMD_ReLogin;
        #endregion

        #region 大厅类的 2
        public const byte CMD_Hall_HeatBeatRet = 2;
        public const byte CMD_Hall_PlayerCountRet = 4;
        public const byte CMD_Hall_GetRoomListRet = 6;
        public const byte CMD_Hall_GetRoomMessionURLRet = 8;


        public const ushort MSG_Hall_HeatBeatRet = (ushort)(SC_PacketType.Hall << 8) + CMD_Hall_HeatBeatRet;
        public const ushort MSG_Hall_PlayerCountRet = (ushort)(SC_PacketType.Hall << 8) + CMD_Hall_PlayerCountRet;
        public const ushort MSG_Hall_GetRoomListRet = (ushort)(SC_PacketType.Hall << 8) + CMD_Hall_GetRoomListRet;
        //public const ushort MSG_Hall_GetRoomMessionURLRet = (ushort)(SC_PacketType.Hall << 8) + CMD_Hall_GetRoomMessionURLRet;
        #endregion

        #region 房间-场地类的3
        #region 命令
        public const byte CMD_Room_EnterRet = 2;
        public const byte CMD_Room_LeaveRet = 4;
        public const byte CMD_Room_ChatRet = 5;

        public const byte CMD_Room_GetRoomUsersCountRet = 6;

        public const byte CMD_Room_GetUserListRet = 8;
        public const byte CMD_Room_GetBFListRet = 10;
        public const byte CMD_BF_ReadyTimeOutRet = 12;
        public const byte CMD_BF_UserReadyRet = 14;

        public const byte CMD_BF_TestResultRet = 16;
        public const byte CMD_BF_HostLANIPInfoRet = 18;
        public const byte CMD_BF_EnterRet = 20;
        public const byte CMD_BF_LeaveRet = 22;

        public const byte CMD_BF_Game_SubResultRet = 24;
        public const byte CMD_BF_Game_ResultRet = 26;

        public const byte CMD_BC_Room_BFState = 28;
        public const byte CMD_BC_Room_Leave = 30;
        public const byte CMD_BC_Room_Enter = 32;//
        public const byte CMD_BC_Room_Reset = 33;
        public const byte CMD_BC_BF_Leave = 34;

        public const byte CMD_BC_BF_Enter = 36;
        public const byte CMD_BC_BF_AllUserReady = 38;
        public const byte CMD_BC_BF_P2PInfo = 40;

        public const byte CMD_BC_BF_SpecHost = 42;

        public const byte CMD_BC_BF_UserStatus = 44;


        public const byte CMD_BC_BF_GameStartLoad = 47;
        public const byte CMD_BC_BF_GameLoad = 48;
        public const byte CMD_BC_BF_GameStart = 49;
        public const byte CMD_BC_BF_GameResult = 50;
        

        public const ushort MSG_BC_BF_GameStartLoad = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_GameStartLoad;
        public const ushort MSG_BC_BF_GameLoad = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_GameLoad;

        public const ushort MSG_BC_BF_GameStart = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_GameStart;
        public const ushort MSG_BC_BF_GameResult = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_GameResult;


        public const byte CMD_User_GetUserBaseInfoByIdResult = 52;
        public const byte CMD_User_GetUserBaseInfoByNameResult = 54;

        public const byte CMD_Room_GameSaveInfoLite = 55;
        public const byte CMD_Room_GameSaveInfo = 56;
        public const byte CMD_Room_GameSaveInfoOK=57;


        //public static ushort MSG_ChatRet;
        public const ushort MSG_ChatRet = (ushort)(SC_PacketType.Room << 8) + CMD_Room_ChatRet;

        public const ushort MSG_Room_GetUserBaseInfoByIdResult = (ushort)(SC_PacketType.Room << 8) + CMD_User_GetUserBaseInfoByIdResult;
        public const ushort MSG_Room_GetUserBaseInfoByNameResult = (ushort)(SC_PacketType.Room << 8) + CMD_User_GetUserBaseInfoByNameResult;

        
        #endregion

        public const ushort MSG_Room_EnterRet = (ushort)(SC_PacketType.Room << 8) + CMD_Room_EnterRet;
        public const ushort MSG_Room_LeaveRet = (ushort)(SC_PacketType.Room << 8) + CMD_Room_LeaveRet;

        public const ushort MSG_Room_GetRoomUsersCountRet = (ushort)(SC_PacketType.Room << 8) + CMD_Room_GetRoomUsersCountRet;

        public const ushort MSG_Room_GetUserListRet = (ushort)(SC_PacketType.Room << 8) + CMD_Room_GetUserListRet;
        public const ushort MSG_Room_GetBFListRet = (ushort)(SC_PacketType.Room << 8) + CMD_Room_GetBFListRet;
        //public const ushort MSG_BF_ReadyTimeOutRet = (ushort)(SC_PacketType.Room << 8) + CMD_BF_ReadyTimeOutRet;
        //public const ushort MSG_BF_UserReadyRet = (ushort)(SC_PacketType.Room << 8) + CMD_BF_UserReadyRet;

        //public const ushort MSG_BF_TestResultRet = (ushort)(SC_PacketType.Room << 8) + CMD_BF_TestResultRet;
        public const ushort MSG_BF_HostLANIPInfoRet = (ushort)(SC_PacketType.Room << 8) + CMD_BF_HostLANIPInfoRet;
        public const ushort MSG_BF_EnterRet = (ushort)(SC_PacketType.Room << 8) + CMD_BF_EnterRet;
        public const ushort MSG_BF_LeaveRet = (ushort)(SC_PacketType.Room << 8) + CMD_BF_LeaveRet;


        public const ushort MSG_BF_Game_SubResultRet = (ushort)(SC_PacketType.Room << 8) + CMD_BF_Game_SubResultRet;
        public const ushort MSG_BF_Game_ResultRet = (ushort)(SC_PacketType.Room << 8) + CMD_BF_Game_ResultRet;

        public const ushort MSG_BC_Room_BFState = (ushort)(SC_PacketType.Room << 8) + CMD_BC_Room_BFState;

        public const ushort MSG_BC_Room_Leave = (ushort)(SC_PacketType.Room << 8) + CMD_BC_Room_Leave;
        public const ushort MSG_BC_Room_Enter = (ushort)(SC_PacketType.Room << 8) + CMD_BC_Room_Enter;//
        public const ushort MSG_BC_Room_Reset = (ushort)(SC_PacketType.Room << 8) + CMD_BC_Room_Reset;
        public const ushort MSG_BC_BF_Leave = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_Leave;

        public const ushort MSG_BC_BF_Enter = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_Enter;
        //public const ushort MSG_BC_BF_AllUserReady = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_AllUserReady;
        public const ushort MSG_BC_BF_P2PInfo = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_P2PInfo;
        public const ushort MSG_BC_BF_SpecHost = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_SpecHost;

        public const ushort MSG_Room_GameSaveInfoLite = (ushort)(SC_PacketType.Room << 8) + CMD_Room_GameSaveInfoLite;
        public const ushort MSG_Room_GameSaveInfo = (ushort)(SC_PacketType.Room << 8) + CMD_Room_GameSaveInfo;

        public const ushort MSG_Room_GameSaveOK = (ushort)(SC_PacketType.Room << 8) + CMD_Room_GameSaveInfoOK;
        ///// <summary>
        ///// 拥护状态改变 -场地
        ///// </summary>
        //public const ushort MSG_BC_BF_UserStatus = (ushort)(SC_PacketType.Room << 8) + CMD_BC_BF_UserStatus;
        #endregion

        #region Game4
        public const byte CMD_Game_SynPose = 1;
        public const byte CMD_Game_Data = 100;
        public const byte CMD_Game_Notify = 101;
        public const byte CMD_Game_LittleOverOK = 102;
        public const ushort MSG_Game_SynPose = (ushort)(SC_PacketType.Game << 8) + CMD_Game_SynPose;
        /// <summary>
        /// 脚本用到的所有数据包都采用该包结构。
        /// </summary>
        public const ushort MSG_Game_Data = (ushort)(SC_PacketType.Game << 8) + CMD_Game_Data;
        public const ushort MSG_Game_Notify = (ushort)(SC_PacketType.Game << 8) + CMD_Game_Notify;
        public const ushort MSG_Game_LittleOverOK = (ushort)(SC_PacketType.Game << 8) + CMD_Game_LittleOverOK;
        #endregion

        #region 用户类的5

        public const byte CMD_BC_User_UserStatusRet = 6;
        public const byte CMD_User_UserUnLineRet = 8;
        public const byte CMD_User_LoginOutRet = 10;
        public const byte CMD_User_RegisterRet = 12;
        public const byte CMD_User_GetMyPlayedMessionRet = 14;


        /// <summary>
        /// 用户状态改变 广播
        /// </summary>
        public const ushort MSG_BC_User_UserStatusRet = (ushort)(SC_PacketType.UserInfo << 8) + CMD_BC_User_UserStatusRet;
        public const ushort MSG_User_UserUnLineRet = (ushort)(SC_PacketType.UserInfo << 8) + CMD_User_UserUnLineRet;
        public const ushort MSG_User_LoginOutRet = (ushort)(SC_PacketType.UserInfo << 8) + CMD_User_LoginOutRet;
        public const ushort MSG_User_RegisterRet = (ushort)(SC_PacketType.UserInfo << 8) + CMD_User_RegisterRet;
        public const ushort MSG_User_GetMyPlayedMessionRet = (ushort)(SC_PacketType.UserInfo << 8) + CMD_User_GetMyPlayedMessionRet;//登陆成功后下载好
        #endregion



    }

    internal class PacketType {
        internal const byte Login = 1;
        internal const byte Hall = 2;
        internal const byte Room = 3;
        internal const byte Game = 4;
        internal const byte UserInfo = 5;//, 
        //Res = 6
    }

    internal class SC_PacketType {
        internal const byte Login = 100;
        internal const byte Hall = 101;
        internal const byte Room = 102;
        internal const byte Game = 103;
        internal const byte UserInfo = 104;//,
    }
}
