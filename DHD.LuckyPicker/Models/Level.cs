using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHD.LuckyPicker.Models
{
    internal class Level
    {
        /// <summary>
        /// 奖项级别
        /// </summary>
        public Int32 Number { get; set; }
        
        /// <summary>
        /// 奖项名称
        /// </summary>
        public String Name { get; set; }
        
        /// <summary>
        /// 最大中奖数
        /// </summary>
        public Int32 MaxNumber { get; set; }

        /// <summary>
        /// 单次抽取人数
        /// </summary>
        public Int32 PerSelect { get; set; }

    }
}
