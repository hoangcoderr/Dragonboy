﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
namespace Mod
{
    public class Utilities : IActionListener
    {
        public const string ManifestModuleName = "Assembly-CSharp.dll";
        public const string PathChatCommand = @"ModData\chatCommands.json";
        public const string PathChatHistory = @"ModData\chat.txt";
        public const string PathHotkeyCommand = @"ModData\hotkeyCommands.json";

        public const sbyte ID_SKILL_BUFF = 7;
        public const int ID_ICON_ITEM_TDLT = 4387;

        private const BindingFlags PUBLIC_STATIC_VOID =
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.InvokeMethod;

        #region Singleton
        private Utilities() { }
        static Utilities() { }
        public static Utilities gI { get; } = new Utilities();
        #endregion

        public static int speedRun = 8;

        
        [ChatCommand("tdc")]
        [ChatCommand("cspeed")]
        public static void setSpeedRun(int speed)
        {
            speedRun = speed;

            GameScr.info1.addInfo("Tốc độ chạy: " + speed, 0);
        }

        [ChatCommand("speed")]
        public static void setSpeedGame(float speed)
        {
            Time.timeScale = speed;
            GameScr.info1.addInfo("Tốc độ game: " + speed, 0);
        }

        /// <summary>
		/// Sử dụng skill Trị thương của namec vào bản thân
		/// </summary>
		[ChatCommand("hsme")]
        [ChatCommand("buffme")]
        [HotkeyCommand('b')]
        public static void buffMe()
        {
            if (!canBuffMe(out Skill skillBuff))
            {
                GameScr.info1.addInfo("Không tìm thấy kỹ năng Trị thương", 0);
                return;
            }

            // Đổi sang skill hồi sinh
            Service.gI().selectSkill(ID_SKILL_BUFF);

            // Tự tấn công vào bản thân
            Service.gI().sendPlayerAttack(new MyVector(), getMyVectorMe(), -1);

            // Trả về skill cũ
            Service.gI().selectSkill(Char.myCharz().myskill.template.id);

            // Đặt thời gian hồi cho skill
            skillBuff.lastTimeUseThisSkill = mSystem.currentTimeMillis();
        }

        /// <summary>
        /// Kiểm tra khả năng sử dụng skill Trị thương vào bản thân.
        /// </summary>
        /// <param name="skillBuff">Skill trị thương.</param>
        /// <returns>true nếu có thể sử dụng skill trị thương vào bản thân.</returns>
        public static bool canBuffMe(out Skill skillBuff)
        {
            skillBuff = Char.myCharz().
                getSkill(new SkillTemplate { id = ID_SKILL_BUFF });

            if (skillBuff == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Dịch chuyển tới một toạ độ cụ thể trong map.
        /// </summary>
        /// <param name="x">Toạ độ x.</param>
        /// <param name="y">Toạ độ y.</param>
        public static void teleportMyChar(int x, int y)
        {
            Char.myCharz().cx = x;
            Char.myCharz().cy = y;
            Service.gI().charMove();

            if (isUsingTDLT())
                return;

            Char.myCharz().cx = x;
            Char.myCharz().cy = y + 1;
            Service.gI().charMove();
            Char.myCharz().cx = x;
            Char.myCharz().cy = y;
            Service.gI().charMove();
        }

        [HotkeyCommand('n')]
        public static void showMenuTeleNpc()
        {
            int vNpcSize = GameScr.vNpc.size();
            if (vNpcSize == 0)
            {
                GameScr.info1.addInfo("Không có NPC nào", 0);
                return;
            }

            MyVector myVector = new MyVector();
            for (int i = 0; i < vNpcSize; i++)
            {
                var npc = (Npc)GameScr.vNpc.elementAt(i);
                myVector.addElement(new Command(npc.template.name,
                    gI, (int)IdAction.MoveToNpc, npc));
            }
            GameCanvas.menu.startAt(myVector, 3);
        }

        /// <summary>
        /// Lấy MyVector chứa nhân vật của người chơi.
        /// </summary>
        /// <returns></returns>
        public static MyVector getMyVectorMe()
        {
            var vMe = new MyVector();
            vMe.addElement(Char.myCharz());
            return vMe;
        }

        /// <summary>
        /// Kiểm tra trạng thái sử dụng TĐLT.
        /// </summary>
        /// <returns>true nếu đang sử dụng tự động luyên tập</returns>
        public static bool isUsingTDLT() =>
            ItemTime.isExistItem(ID_ICON_ITEM_TDLT);

        /// <summary>
        /// Lấy danh sách các hàm trong theo tên của class.
        /// </summary>
        /// <remarks> Lưu ý:
        /// <list type="bullet">
        /// <item><description>Chỉ lấy các hàm public static void.</description></item>
        /// <item><description>Tên class phải bao gồm cả namespace.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="typeFullName"></param>
        /// <returns>Danh sách các hàm trong class.</returns>
        public static MethodInfo[] getMethods(string typeFullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .First(x => x.ManifestModule.Name == Utilities.ManifestModuleName)
                .GetTypes().FirstOrDefault(x => x.FullName.ToLower() == typeFullName.ToLower())
                .GetMethods(PUBLIC_STATIC_VOID);
        }

        /// <summary>
        /// Lấy danh sách tất cả các hàm của tệp Assembly-CSharp.dll.
        /// </summary>
        /// <remarks> Lưu ý:
        /// <list type="bullet">
        /// <item><description>Chỉ lấy các hàm public static void.</description></item>
        /// <item><description>Tên class phải bao gồm cả namespace.</description></item>
        /// </list>
        /// </remarks>
        /// <returns>Danh sách các hàm của tệp Assembly-CSharp.dll.</returns>
        public static IEnumerable<MethodInfo> GetMethods()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .First(x => x.ManifestModule.Name == Utilities.ManifestModuleName)
                .GetTypes().Where(x => x.IsClass)
                .SelectMany(x => x.GetMethods(PUBLIC_STATIC_VOID));
        }

        public void perform(int idAction, object p)
        {
            IdAction id = (IdAction)idAction;
            switch (id)
            {
                case IdAction.None:
                    break;
                case IdAction.MoveToNpc:
                    moveToNpc((Npc)p);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Dịch chuyển tới npc trong map.
        /// </summary>
        /// <param name="npc">Npc cần dịch chuyển tới</param>
        private static void moveToNpc(Npc npc)
        {
            teleportMyChar(npc.cx, npc.ySd - npc.ySd % 24);
            Char.myCharz().npcFocus = npc;
        }

        [ChatCommand("csb")]
        public static void useCapsule()
        {
            if (useItem(193, 194))
                return;

            GameScr.info1.addInfo("Không tìm thấy capsule", 0);
        }

        [ChatCommand("bt")]
        public static void usePorata()
        {
            if (useItem(921, 454))
                return;

            GameScr.info1.addInfo("Không tìm thấy bông tai", 0);
        }

        /// <summary>
        /// Sử dụng một item có id là một trong số các id truyền vào.
        /// </summary>
        /// <param name="templatesId">Mảng chứa các id của các item muốn sử dụng.</param>
        /// <returns>true nếu có vật phẩm được sử dụng.</returns>
        public static bool useItem(params short[] templatesId)
        {
            for (sbyte i = 0; i < Char.myCharz().arrItemBag.Length; i++)
            {
                var item = Char.myCharz().arrItemBag[i];
                if (item != null && templatesId.Contains(item.template.id))
                {
                    Service.gI().useItem(0, 1, i, -1);
                    return true;
                }
            }
            return false;
        }
        
        public static string getTextPopup(PopUp popUp)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < popUp.says.Length; i++)
            {
                stringBuilder.Append(popUp.says[i]);
                stringBuilder.Append(" ");
            }
            return stringBuilder.ToString().Trim();
        }

        public static void requestChangeMap(Waypoint waypoint)
        {
            if (waypoint.isOffline)
            {
                Service.gI().getMapOffline();
                return;
            }
            
            Service.gI().requestChangeMap();
        }

        public static Waypoint findWaypoint(int type)
        {
            var vGoSize = TileMap.vGo.size();
            if (vGoSize == 1)
            {
                return (Waypoint)TileMap.vGo.elementAt(0);
            }
            
            for (int i = 0; i < vGoSize; i++)
            {
                Waypoint waypoint = (Waypoint)TileMap.vGo.elementAt(i);
                var textPopup = getTextPopup(waypoint.popup);
                
                if (type == 0)
                {
                    if ((TileMap.mapID == 70 && textPopup == "Vực cấm") ||
                        (TileMap.mapID == 73 && textPopup == "Vực chết") ||
                        (TileMap.mapID == 110 && textPopup == "Rừng tuyết"))
                    {
                        return waypoint;
                    }
                    
                    if (waypoint.maxX < 60)
                    {
                        return waypoint;
                    }
                }
                
                if (type == 1)
                {
                    if (((TileMap.mapID is 106 or 107) && textPopup == "Hang băng") || 
                        ((TileMap.mapID is 105 or 108) && textPopup == "Rừng băng") || 
                        (TileMap.mapID == 109 && textPopup == "Cánh đồng tuyết"))
                    {
                        return waypoint;
                    }
                    
                    if (TileMap.mapID == 27)
                    {
                        return null;
                    }
                    
                    if (waypoint.minX < TileMap.pxw - 60 && waypoint.maxX >= 60)
                    {
                        return waypoint;
                    }
                }
                
                if (TileMap.mapID == 70 && textPopup == "Căn cứ Raspberry")
                {
                    return waypoint;
                }
                if (waypoint.minX > TileMap.pxw - 60)
                {
                    return waypoint;
                }
            }

            return null;
        }

        public static int getXWayPoint(Waypoint waypoint)
        {
            return waypoint.maxX < 60 ? 15 :
                waypoint.minX > TileMap.pxw - 60 ? TileMap.pxw - 15 :
                waypoint.minX + 30;
        }

        public static int getYWayPoint(Waypoint waypoint)
        {
            return waypoint.maxY;
        }

        [HotkeyCommand('j')]
        public static void changeMapLeft()
        {
            Waypoint waypoint = findWaypoint(0);
            if (waypoint != null)
            {
                teleportMyChar(getXWayPoint(waypoint), getYWayPoint(waypoint));
                requestChangeMap(waypoint);
            }
        }

        [HotkeyCommand('k')]
        public static void changeMapMiddle()
        {
            Waypoint waypoint = findWaypoint(1);
            if (waypoint != null)
            {
                teleportMyChar(getXWayPoint(waypoint), getYWayPoint(waypoint));
                requestChangeMap(waypoint);
            }
        }

        [HotkeyCommand('l')]
        public static void changeMapRight()
        {
            Waypoint waypoint = findWaypoint(2);
            if (waypoint != null)
            {
                teleportMyChar(getXWayPoint(waypoint), getYWayPoint(waypoint));
                requestChangeMap(waypoint);
            }
        }

        [HotkeyCommand('g')]
        public static void sendGiaoDichToCharFocusing()
        {
            var charFocus = Char.myCharz().charFocus;
            if (charFocus == null)
            {
                GameScr.info1.addInfo("Trỏ vào nhân vật để giao dịch", 0);
                return;
            }

            Service.gI().giaodich(0, charFocus.charID, -1, -1);
            GameScr.info1.addInfo("Đã gửi lời mời giao dịch đến " + charFocus.cName, 0);
        }

        [ChatCommand("k")]
        public static void changeZone(int zone)
        {
            Service.gI().requestChangeZone(zone, -1);
        }
    }
}