using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;

namespace VideoCompresserDFW
{
    class ProcesserStaticMethods
    {

        public static Byte LEFT_MOTION => 2;

        public static Byte RIGHT_MOTION => 1;

        public static Byte BOTH_MOTION => 3;

        public static Byte NO_MOTION => 0;

        /// <summary>
        /// 判断给定方格内是否存在运动的函数
        /// </summary>
        /// <param name="imgDiff">预处理过的存储着与平均帧差值的差值帧</param>
        /// <param name="rec">存储着给定方格的左上和右下坐标的tuple((ltx, lty), (rbx, rby))</param>
        /// <returns>Boolean, Whether motion exists in given rectangle</returns>
        public static bool JudgeMoving(Image<Bgr, Byte> imgDiff, int[,] rec, int skipPixCount = 3, int motionThreshold = 4)
        {

            int recTotalPixValue = 0;
            int allIndex = 0;
            for(int i = rec[0,1]; i < rec[1,1]; i+= skipPixCount)
            {
                for(int j = rec[0,0]; j < rec[1,0]; j+= skipPixCount)
                {
                    allIndex++;
                    switch(allIndex % 3)
                    {
                        case 0:
                            recTotalPixValue += (int)imgDiff[i, j].Blue;
                            break;
                        case 1:
                            recTotalPixValue += (int)imgDiff[i, j].Green;
                            break;
                        case 2:
                            recTotalPixValue += (int)imgDiff[i, j].Red;
                            break;
                        default:
                            break;
                    }
                    
                }
            }

            return recTotalPixValue > Math.Abs(rec[0, 1] - rec[1, 1]) * Math.Abs(rec[1, 0] - rec[0, 0]) * motionThreshold;
        }

        public static void detectAndSignMotions(ArrayList videoImgs, Image<Bgr, Byte> videoAverageImage, ArrayList videoDiffImages, MyVideo sourceVideo, Byte[] motionSides)
        {
            Console.WriteLine();

            //for (int i = 0; i < videoImgs.Count; i++)
            Parallel.For(0, videoImgs.Count, i =>
            {
                ArrayList moveRects = new ArrayList();
                var diffImage = (videoImgs[i] as Image<Bgr, Byte>).AbsDiff(videoAverageImage);
                videoDiffImages.Add(diffImage);

                for(int colIndex = 0; colIndex < sourceVideo.RectColCount; colIndex++)
                {
                    for(int rowIndex = 0; rowIndex < sourceVideo.RectRowCount; rowIndex++)
                    {
                        if(rowIndex == colIndex)
                        {
                            ;
                        }
                        if(JudgeMoving(diffImage, sourceVideo.GetRectsPosition(rowIndex, colIndex)))
                        {
                            //CvInvoke.Imshow("test", diffImage);
                            //CvInvoke.WaitKey();
                            moveRects.Add(new int[] {rowIndex, colIndex});
                            if(colIndex > sourceVideo.RectColCount / 2)
                            {
                                motionSides[i] |= RIGHT_MOTION;
                            }
                            else
                            {
                                motionSides[i] |= LEFT_MOTION;
                            }
                        }
                    }
                }

                foreach(int[] rec in moveRects)
                {
                    CvInvoke.Rectangle(videoImgs[i] as Image<Bgr, Byte>, sourceVideo.GetRectsPositionRectangle(rec[0], rec[1]), new MCvScalar(0, 0, 255), 1);
                }

                double finishRate = (double)i / videoImgs.Count * 100.0;
                if((i & 15) == 0)
                {
                    Console.Write(String.Format("\rMotion Detecting Processing : {0,4:F2}%\t", finishRate) +
                    new string('▋', (int)finishRate / 5));
                }               
            }
            );

            Console.WriteLine("\rMotion Detecting Processing :   100%\t" + new String('▋', 20) +
                    "\nMotion Detection Finished, Video Generating......");
        }

        public static void CutMoveSide(int[] newPoses, ArrayList intoImgs, int oldPos, ArrayList fromImgs, Byte side, MyVideo myVideo)
        {
            //Image<Bgr, Byte> tmpIm;
            if ((side & LEFT_MOTION) > 0)
            {
                //tmpIm = intoImgs[newPoses[0]] as Image<Bgr, Byte>;
                intoImgs[newPoses[0]] = (fromImgs[oldPos] as Image<Bgr, Byte>).GetSubRect(myVideo.FrameLeftRect).ConcateHorizontal((intoImgs[newPoses[0]] as Image<Bgr, Byte>).GetSubRect(myVideo.FrameRightRect));
                CvInvoke.PutText(intoImgs[newPoses[0]] as Image<Bgr, Byte>, "Original: " + ((int)((double)oldPos / myVideo.Fps / 60)).ToString()
                    + " : " + ((int)(((double)oldPos) / myVideo.Fps) % 60).ToString(), new Point(10, 20), FontFace.HersheySimplex, 0.75, new MCvScalar(0, 0, 255), 2);
                newPoses[0]++;
                //tmpIm.Dispose();
            }

            if((side & RIGHT_MOTION) > 0)
            {
                //tmpIm = intoImgs[newPoses[1]] as Image<Bgr, Byte>;
                intoImgs[newPoses[1]] = (intoImgs[newPoses[1]] as Image<Bgr, Byte>).GetSubRect(myVideo.FrameLeftRect).ConcateHorizontal((fromImgs[oldPos] as Image<Bgr, Byte>).GetSubRect(myVideo.FrameRightRect));
                CvInvoke.PutText(intoImgs[newPoses[1]] as Image<Bgr, Byte>, "Original: " + ((int)((double)oldPos / myVideo.Fps / 60)).ToString()
                    + " : " + ((int)(((double)oldPos) / myVideo.Fps) % 60).ToString(), new Point(myVideo.FrameWidth / 2 + 10, 20), FontFace.HersheySimplex, 0.75, new MCvScalar(0, 0, 255), 2);
                //tmpIm.Dispose();
                newPoses[1]++;
            }
        }
    }
}
