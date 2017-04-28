using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace VideoCompresserDFW
{
    class ProcesserStaticMethods
    {
        private const int LEFT_MOTION = 1 << 1;
        private const int RIGHT_MOTION = 1;
        private const int BOTH_MOTION = 3;
        private const int NO_MOTION = 0;

        public static int LEFT_MOTION1 => LEFT_MOTION;

        public static int RIGHT_MOTION1 => RIGHT_MOTION;

        public static int BOTH_MOTION1 => BOTH_MOTION;

        public static int NO_MOTION1 => NO_MOTION;

        /// <summary>
        /// 判断给定方格内是否存在运动的函数
        /// </summary>
        /// <param name="imgDiff">预处理过的存储着与平均帧差值的差值帧</param>
        /// <param name="rec">存储着给定方格的左上和右下坐标的tuple((ltx, lty), (rbx, rby))</param>
        /// <returns>Boolean, Whether motion exists in given rectangle</returns>
        public static bool JudgeMoving(Image<Gray, Byte> imgDiff, int[,] rec, int skipPixCount = 3, int motionThreshold = 4)
        {

            int recTotalPixValue = 0;
            for(int i = rec[0,1]; i < rec[1,1]; i+= skipPixCount)
            {
                for(int j = rec[0,0]; j < rec[1,0]; i+= skipPixCount)
                {
                    recTotalPixValue += (int)imgDiff[i, j].Intensity;
                }
            }

            return recTotalPixValue > Math.Abs(rec[0, 1] - rec[1, 1]) * Math.Abs(rec[1, 0] - rec[1, 1]) * motionThreshold;
        }

        public static void detectAndSignMotions()
        {
            ;
        }


    }
}
