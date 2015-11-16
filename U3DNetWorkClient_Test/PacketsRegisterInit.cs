//using System;
//using System.Collections.Generic;
//using System.Text;
//using Cell.Core;


//namespace GameNet.Network {
//    internal static class CS_PacketsInitialize {
//        /// <summary>
//        /// 注册客户端 包 的构造器
//        /// </summary>
//        public static void RegisterPacketsCreator(PacketCreatorManager packetMgr) {
//            //---login
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Login, new CS_LoginCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Login_GetSoftUrlVer, new CS_GetSoftUrlVerCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Login_GetSysInfo, new CS_GetRemoteSystemInfoCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Login_HallList, new CS_GetHallListCreator());
//            //--chat
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Chat, new CS_ChatCreator());
//            //--- user
//            packetMgr.RegistePacket(CS_PacketIds.MSG_User_GetMyPlayedMession, new CS_GetMyPlayedMessionsCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Room_GetUserBaseInfoById, new CS_GetUserInfoByIdCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Room_GetUserBaseInfoByName, new CS_GetUserInfoByNameCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_User_LoginOut, new CS_LoginOutCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_User_Register, new CS_UserRegisterCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_User_UserUnLine, new CS_UserUnLineCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_User_UserStatusChange, new CS_UserStatusChangeCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_User_UserPCPerformance, new CS_UserPCPerformanceCreator());
//            //--hall
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Hall_PlayerCount, new CS_GetHallPlayerCountCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Hall_GetRoomMessionURL, new CS_RoomMessionVerURLCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Hall_GetRoomList, new CS_GetRoomListInHallCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Hall_HeatBeat, new CS_HeatBeatCreator());
//            //---room
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Room_Enter, new CS_EnterRoomCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Room_GetBFList, new CS_GetBFieldInfosCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Room_GetRoomUsersCount, new CS_GetRoomUsersCountCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Room_GetUserList, new CS_GetRoomUsersInfoCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Room_Leave, new CS_LeaveRoomCreator());
//            //packetMgr.RegistePacket(CS_PacketIds.MSG_Room_Close, new CS_RoomCloseCreator());
//            //-- bfield
//            //packetMgr.RegistePacket(CS_PacketIds.MSG_BF_HostLANIPInfo, new CS_BFHostLANIPInfoCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_BC_Room_BFState, new CS_BFiledStatusChangeCreator());
//            //packetMgr.RegistePacket(CS_PacketIds.MSG_BF_TestResult, new CS_BFTestHostResultCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_BF_UserReady, new CS_BFUserReadyCreator());
//            //packetMgr.RegistePacket(CS_PacketIds.MSG_BF_ReadyTimeOut, new CS_BFUserReadyTimeOutCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_BF_Enter, new CS_EnterBFieldCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_BF_Leave, new CS_LeaveBFieldCreator());
//            //---game
//            //packetMgr.RegistePacket(CS_PacketIds.MSG_BF_Game_Result, new CS_Game_ResultCreator());
//            // packetMgr.RegistePacket(CS_PacketIds.MSG_BF_Game_SubResult, new CS_Game_SubResultCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_BF_GameOver, new CS_GameOverCreator());
//            //packetMgr.RegistePacket(CS_PacketIds.MSG_BF_GameSccore, new CS_GameSccoreCreator());
//            //packetMgr.RegistePacket(CS_PacketIds.MSG_BF_GameStart, new CS_GameStartCreator());
//            //--bc --room
//            packetMgr.RegistePacket(CS_PacketIds.MSG_BF_GameStartLoad, new CS_GameStartLoadCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_BF_GameLoad, new CS_GameLoadCreator());
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Game_GameData, new CS_Game_GameDataCreator());
//            //packetMgr.RegistePacket(CS_PacketIds.MSG_Game_GameNotify,new CS_Game
//#if 压力测试
//            packetMgr.RegistePacket(CS_PacketIds.MSG_Game_SynPose, new CS_GameSyn_PoseCreator());
//#endif
//            //
//#if 机房版
//            packetMgr.RegistePacket(JF_PacketIds.CS_Login, new JF_LoginResultPacketCreator());
//#endif
//        }
//    }
//    internal static class SC_PacketsInitialize {
//        /// <summary>
//        /// 注册服务端 包 的构造器
//        /// </summary>
//        public static void RegisterPacketsCreator(PacketCreatorManager packetMgr) {
//            //---login
//            packetMgr.RegistePacket(SC_PacketIds.MSG_LoginRet, new SC_LoginResultPacketCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Login_GetSoftUrlVerRet, new SC_GetSoftUrlVerCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Login_GetSysInfoRet, new SC_GetRemoteSystemInfoCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Login_HallListRet, new SC_GetHallListCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Login_UserReLogin, new SC_UserReLoginPacketCreator());
//            //chat
//            packetMgr.RegistePacket(SC_PacketIds.MSG_ChatRet, new SC_ChatCreator());
//            //---user
//            packetMgr.RegistePacket(SC_PacketIds.MSG_User_GetMyPlayedMessionRet, new SC_GetMyPlayedMessionsCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Room_GetUserBaseInfoByIdResult, new SC_GetUserInfoByIdCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Room_GetUserBaseInfoByNameResult, new SC_GetUserInfoByNameCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_User_LoginOutRet, new SC_LoginOutCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_User_RegisterRet, new SC_UserRegisterCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_User_UserUnLineRet, new SC_UserUnLineCreator());

//            //--hall
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Hall_PlayerCountRet, new SC_GetHallPlayerCountCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_Hall_GetRoomMessionURLRet, new SC_RoomMessionVerURLCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Hall_GetRoomListRet, new SC_GetRoomsInHallCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Hall_HeatBeatRet, new SC_HeatBeatCreator());
//            //---room
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Room_EnterRet, new SC_EnterRoomCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Room_GetBFListRet, new SC_GetBFieldInfosCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Room_GetRoomUsersCountRet, new SC_GetRoomUsersCountCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Room_GetUserListRet, new SC_GetRoomUsersInfoCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_Room_LeaveRet, new SC_LeaveRoomCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_Room_CloseRet, new SC_RoomCloseCreator());

//            //--bfield
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BF_HostLANIPInfoRet, new SC_BFHostLANIPInfoCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BC_Room_BFState, new SC_BFiledStatusChangeCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BF_TestResultRet, new SC_BFTestHostResultCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BF_UserReadyRet, new SC_BFUserReadyCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BF_EnterRet, new SC_EnterBFieldCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BF_LeaveRet, new SC_LeaveBFieldCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BF_ReadyTimeOutRet, new SC_BFUserReadyTimeOutCreator());

//            //---game
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BF_Game_ResultRet, new SC_BCBF_GameResultCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BF_Game_SubResultRet, new SC_Game_SubResultCreator());
//            //--bc --room
//            //PacketHandles[SC_PacketIds.MSG_BC_Room_Reset] = RoomService.OnRecv_ResetRoom;
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_Room_Reset, new SC_BC_RoomResetCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_Enter, new SC_BC_BFiledUserEnterCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_Leave, new SC_BC_BFiledUserLeaveCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_Room_BFState, new SC_BC_BFiledStatusChangeCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BC_Room_Close, new SC_BC_RoomCloseCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_Room_Enter, new SC_BC_RoomHasUserEnterCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_Room_Leave, new SC_BC_RoomHasUserLeaveCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BC_User_UserStatusRet, new SC_BC_UserStatusChangeCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_User_UserStatusRet, new SC_BC_RoomUserStatusChangeCreator());

//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_AllUserReady, new SC_BCBF_AllUserReadyCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_P2PInfo, new SC_BCBF_P2PInfoCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_Host, new SC_BCBF_SelectHostOkCreator());
//            //packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_UserStatus, new SC_BC_BFiledStatusChangeCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_GameResult, new SC_BCBF_GameResultCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_SpecHost, new SC_BCBF_SpecHostCreator());

//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_GameStart, new SC_BCBF_GameStartCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_GameLoad, new SC_BCBF_GameLoadCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_BC_BF_GameStartLoad, new SC_BCBF_GameStartLoadCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Game_Data, new SC_Game_GameDataCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Game_Notify, new SC_Game_GameNotifyCreator());
//            //
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Room_GameSaveInfo, new SC_Game_SaveInfoCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Room_GameSaveInfoLite, new SC_Game_SaveInfoLiteCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Game_LittleOverOK, new SC_GameOver_LittleOKCreator());
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Room_GameSaveOK, new SC_Game_SaveInfoOKCreator());
//#if 压力测试
//            packetMgr.RegistePacket(SC_PacketIds.MSG_Game_SynPose, new SC_GameSyn_PoseCreator());
//#endif
//#if 机房版
//            packetMgr.RegistePacket(JF_PacketIds.SC_LoginResult, new JF_LoginResultPacketCreator());
//#endif
//        }
//    }
//}
