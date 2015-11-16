using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetIOCPClient.NetWork
{
    /// <summary>
    /// 计算机如何存储大数值的体系结构
    /// </summary>
    public enum Endian
    {
        /// <summary>
        ///  Intel x86，AMD64，DEC VAX
        /// </summary>
        LITTLE_ENDIAN = 0,
        /// <summary>
        /// Sun SPARC, Motorola 68000，Java Virtual Machine
        /// </summary>
        BIG_ENDIAN = 1,
    }
}
