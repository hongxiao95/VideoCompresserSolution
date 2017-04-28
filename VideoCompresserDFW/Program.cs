using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace VideoCompresserDFW
{
    class Program
    {
        static void Main(string[] args)
        {
            MyVideo myVideo = new MyVideo(".\\SourceVideo2.mp4", 16);

            CvInvoke.Imshow("Name", myVideo.GetAverageFrame(0.2,true));
            CvInvoke.WaitKey();
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
