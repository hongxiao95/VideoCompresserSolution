using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.VideoStab;

namespace VideoCompresserDFW
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            Stopwatch sw = new Stopwatch();


            sw.Start();
            MyVideo myVideo = new MyVideo(".\\SourceVideo2.mp4", 16);
            Image<Bgr, Byte> averageFrame = myVideo.GetAverageFrame(0.2, true);
            sw.Stop();
            Console.WriteLine("Average Frame Total Running Time: " + sw.ElapsedMilliseconds + "ms");


            Console.ReadLine();
            sw.Reset();
            sw.Start();
            myVideo.ReCapVideo();
            for(int i = 0; i < 2500; i++)
            {
                myVideo.ReadVideoMat();
            }
            sw.Stop();
            Console.WriteLine("Reading All Total Running Time: " + sw.ElapsedMilliseconds + "ms");
            Console.ReadLine();

            //Console.ReadLine();

            sw.Reset();
            sw.Start();
            byte[] motionSides = new byte[myVideo.FrameCount];
            var videoImgs = myVideo.GetFrameList();
            ProcesserStaticMethods.detectAndSignMotions(videoImgs, averageFrame, myVideo, motionSides);
            sw.Stop();
            Console.WriteLine("Detecting Total Running Time: " + sw.ElapsedMilliseconds + "ms");

            sw.Reset();
            sw.Start();
            int[] newPoses = new int[2] { 0,0};

            for(int i = 0; i < myVideo.FrameCount; i++)
            {
                ProcesserStaticMethods.CutMoveSide(newPoses, videoImgs, i, videoImgs, motionSides[i], myVideo);
            }

            int leftLength = newPoses[0];
            int rightLength = newPoses[1];
            ArrayList averageFramList = new ArrayList();
            
            averageFramList.Add(averageFrame);

            if(leftLength > rightLength)
            {
                for(int i = rightLength; i < leftLength; i++)
                {
                    ProcesserStaticMethods.CutMoveSide(new int[] { 0, i }, videoImgs, 0, averageFramList, ProcesserStaticMethods.RIGHT_MOTION, myVideo);
                    
                }
            }
            else
            {
                for(int i = leftLength; i < rightLength; i++)
                {
                    ProcesserStaticMethods.CutMoveSide(new int[] { i, 0 }, videoImgs, 0, averageFramList, ProcesserStaticMethods.LEFT_MOTION, myVideo);
                }

            }

            VideoWriter writer = new VideoWriter("testOut.avi", VideoWriter.Fourcc('X', 'V', 'I', 'D'), (int)myVideo.Fps, myVideo.GetVideoSize(), true);

            for(int i =0; i < Math.Max(rightLength, leftLength); i++)
            {
                writer.Write((videoImgs[i] as Image<Bgr, Byte>).Mat);
            }
            sw.Stop();

            writer.Dispose();
            foreach (Image<Bgr, Byte> im in videoImgs)
            {
                im.Dispose();
            }

            Console.WriteLine("Finish, LastCost: " + sw.ElapsedMilliseconds + "ms");
            videoImgs.Clear();
            GC.Collect();

            myVideo = null;
            Console.ReadLine();
        }

        public static void test()
        {
            Console.WriteLine("Hello FrameWork");
            Mat a = CvInvoke.Imread(@".\test1.jpg");
            Image<Bgr, double> b = a.ToImage<Bgr, double>();


            Console.WriteLine("OK Mat Ready");
            Console.ReadLine();
            for (int i = 0; i < 4; i++)
            {
                CvInvoke.Add(a, a, a);
            }

            b = a.ToImage<Bgr, double>();


            Console.WriteLine("OK Convert");
            Console.ReadLine();

            Console.WriteLine(Convert.ToString(b[3, 4]));
            Console.WriteLine("OK");
            Console.WriteLine(Convert.ToString(b[3, 4]));
            Console.ReadLine();

            Image<Gray, Byte> c = new Image<Gray, byte>(b.Size);
            CvInvoke.CvtColor(b, c, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);


            CvInvoke.Imshow("Test", c);
            CvInvoke.WaitKey();
        }
    }
}
