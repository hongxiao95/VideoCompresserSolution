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
            //Console.ReadLine();
            Stopwatch sw = new Stopwatch();


            sw.Start();
            MyVideo myVideo = new MyVideo(".\\SourceVideo2.mp4", 16);
            Image<Bgr, Byte> averageFrame = myVideo.GetAverageFrame(0.2, true);
            sw.Stop();
            Console.WriteLine("Average Frame Total Running Time: " + sw.ElapsedMilliseconds + "ms");

            //Console.ReadLine();

            sw.Reset();
            sw.Start();
            byte[] motionSides = new byte[myVideo.FrameCount];
            var videoImgs = myVideo.GetFrameList();
            ArrayList videoDiffImages = new ArrayList();
            ProcesserStaticMethods.detectAndSignMotions(videoImgs, averageFrame, videoDiffImages, myVideo, motionSides);
            sw.Stop();
            Console.WriteLine("Detecting Total Running Time: " + sw.ElapsedMilliseconds + "ms");

            sw.Reset();
            sw.Start();
            int[] newPoses = new int[2] { 0,0};

            for(int i = 0; i < myVideo.FrameCount; i++)
            {
                ProcesserStaticMethods.CutMoveSide(newPoses, videoImgs, i, videoImgs, motionSides[i], myVideo);
            }

            VideoWriter writer = new VideoWriter("testOut.avi", VideoWriter.Fourcc('X', 'V', 'I', 'D'), (int)myVideo.Fps, myVideo.GetVideoSize(), true);

            for(int i =0; i < videoImgs.Count; i++)
            {
                writer.Write((videoImgs[i] as Image<Bgr, Byte>).Mat);
                (videoImgs[i] as Image<Bgr, Byte>).Mat.Dispose();
            }
            sw.Stop();

            writer.Dispose();

            Console.WriteLine("Finish, LastCost: " + sw.ElapsedMilliseconds + "ms");
            foreach(Image<Bgr, Byte> im in videoImgs)
            {
                if (im != null)
                {
                    im.Dispose();
                }
            }

            foreach(Image<Bgr, Byte> im in videoDiffImages)
            {
                if( im != null)
                {
                    im.Dispose();
                }
                
            }
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
