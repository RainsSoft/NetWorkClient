using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetIOCPClient.Network
{
    public delegate void EnumPacketHandlerAttr(PktHandlerAttribute attr, Delegate handle);
    /// <summary>
    /// 遍历包处理函数的类型中的包处理函数和包构造器集合的辅助函数
    /// </summary>
    public static class PacketRegisteHelper
    {
        /// <summary>
        /// 遍历包含了包处理函数的类型中的包处理函数和包构造器集合
        /// </summary>
        /// <typeparam name="T">包处理委托类型</typeparam>
        /// <param name="type">包含了包处理函数的类型</param>
        /// <param name="callback">如果遇到PacketHandlerAttribute,则调用该回调函数</param>
        public static void EnumPacketHandlers<T>(Type type, EnumPacketHandlerAttr callback, params object[] target) {
            if (type == null) {
                throw new ArgumentNullException("type不能为空");
            }
            int ret = 0;
            List<T> ret1 = new List<T>();
            List<PacketCreator> ret2 = new List<PacketCreator>();
            MethodInfo[] methods = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var v in methods) {
                try {
                    object[] objs = v.GetCustomAttributes(typeof(PktHandlerAttribute), true);
                    if (objs != null && objs.Length > 0) {
                        if (callback != null) {

                            Delegate handle = null;
                            if (target != null && target.Length > 0) {
                                handle = Delegate.CreateDelegate(typeof(T), target[0], v.Name);
                            }
                            else {
                                handle = Delegate.CreateDelegate(typeof(T), v);
                            }
                            PktHandlerAttribute attr = objs[0] as PktHandlerAttribute;

                            callback(attr, handle);

                        }
                    }
                }
                catch (Exception ee) {
#if DEBUG
                    Console.WriteLine(v.ToString() + "\n" + ee.ToString());
#endif
                }
            }
        }
    }




}
