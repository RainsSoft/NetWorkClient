using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetIOCPClient.Util
{
    /*
     分布式ID生成方案（分布式数据库） 
背景：在互联网应用中，应用需要为每一个用户分配一个id，在使用分布式数据库情况下，已经不能依靠自增主键来生成唯一性id了。。。

根据特定算法生成唯一ID 
可重现的id生成方案：使用用户提供的特定的数据源（登录凭证），通过某种算法生成id，这个过程是可重现的，只要用户提供的数据源是唯一的，那么生成的id也是唯一的。 
例如通过用户注册的email+salt，使用摘要算法(md5/sha)生成128bit的数据，然后通过混合因子转变为一个long类型的数据是64bit，有264 个可用数据，理论上冲突几率极低，优点：可用保证id固定的，每次通过email登录，直接能得到id，不需要访问数据库查询id。

不可重现的方案： 
使用每个服务器环境中的如下参数：
1.服务器网卡MAC地址/IP地址（确保服务器之间不冲突）
2.每个生成ID的程序的唯一编号（确保同一服务器上的不同服务之间不冲突）
3.程序每次启动的唯一编号（确保程序的每次启停之间不冲突）
4.启动后内存里的序列号/系统当前时间（确保程序的一次运行期内不冲突）

以及其他的参数，混合生成id，保证多台服务器、多个线程生成的id不冲突。

例如：

UUID.randomUUID().toString() 生成的是length=32的16进制格式的字符串，如果回退为byte数组共16个byte元素，即UUID是一个128bit长的数字，一般用16进制表示。算法的核心思想是结合机器的网卡、当地时间、一个随即数来生成UUID。从理论上讲，如果一台机器每秒产生10000000个GUID，则可以保证（概率意义上）3240年不重复

例如：Instagram 的ID生成策略链接地址是http://www.cnblogs.com/yjl49/archive/2012/04/16/2452210.html（ＯＳＣＨＩＮＡ的编辑器不太会用．．．）

Twitter的 Snowflake—一个使用Apache ZooKeeper来整合所有节点然后生成64bit唯一ID的简洁的服务 

     */

  /**
  * 64位ID (42(毫秒)+5(机器ID)+5(业务编码)+12(重复累加))
  * From: https://github.com/twitter/snowflake
  * An object that generates IDs.
  * This is broken into a separate class in case
  * we ever want to support multiple worker threads
  * per process
  */
    public class UUID
    {
        private static long twepoch = 1288834974657L;
        // 机器标识位数
        private static int workerIdBits = 5;
        // 数据中心标识位数
        private static int datacenterIdBits = 5;
        // 机器ID最大值
        private static long maxWorkerId = -1L ^ (-1L << workerIdBits);
        // 数据中心ID最大值
        private static long maxDatacenterId = -1L ^ (-1L << datacenterIdBits);
        // 毫秒内自增位
        private static int sequenceBits = 12;
        // 机器ID偏左移12位
        private static int workerIdShift = sequenceBits;
        // 数据中心ID左移17位
        private static int datacenterIdShift = sequenceBits + workerIdBits;
        // 时间毫秒左移22位
        private static int timestampLeftShift = sequenceBits + workerIdBits + datacenterIdBits;

        private static long sequenceMask = -1L ^ (-1L << sequenceBits);

        private static long lastTimestamp = -1L;

        private long sequence = 0L;
        private long workerId;
        private long datacenterId;

        private static object m_locker = new object();
        public UUID(int workerId, int datacenterId) {
            if (workerId > maxWorkerId || workerId < 0) {
                throw new ArgumentException(string.Format("worker Id can't be greater than {0} or less than 0", maxWorkerId));
            }
            if (datacenterId > maxDatacenterId || datacenterId < 0) {
                throw new ArgumentException(string.Format("datacenter Id can't be greater than {0} or less than 0", maxDatacenterId));
            }
            this.workerId = workerId;
            this.datacenterId = datacenterId;
        }

        public long nextId() {
            lock (m_locker) {
                long timestamp = timeGen();
                if (timestamp < lastTimestamp) {
                    try {
                        throw new Exception("Clock moved backwards.  Refusing to generate id for " + (lastTimestamp - timestamp) + " milliseconds");
                    }
                    catch (Exception e) {
                        //e.printStackTrace();
                    }
                }

                if (lastTimestamp == timestamp) {
                    // 当前毫秒内，则+1
                    sequence = (sequence + 1) & sequenceMask;
                    if (sequence == 0) {
                        // 当前毫秒内计数满了，则等待下一秒
                        timestamp = tilNextMillis(lastTimestamp);
                    }
                }
                else {
                    sequence = 0;
                }
                lastTimestamp = timestamp;
                // ID偏移组合生成最终的ID，并返回ID
                long nextId = ((timestamp - twepoch) << timestampLeftShift)
                | (datacenterId << datacenterIdShift)
                | (workerId << workerIdShift) | sequence;

                return nextId;
            }
        }

        private long tilNextMillis(long lastTimestamp) {
            long timestamp = this.timeGen();
            while (timestamp <= lastTimestamp) {
                timestamp = this.timeGen();
            }
            return timestamp;
        }

        private long timeGen() {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}
