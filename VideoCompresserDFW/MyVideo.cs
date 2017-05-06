using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace VideoCompresserDFW
{
    class MyVideo
    {
        //只读
        private readonly String sourceVideoFileName;
        private readonly int smallRectWidthInPix;

        //可以get
        private int frameWidth;
        private int frameHeight;
        private int frameCount;
        private double fps;
        private int currentFrameIndex;
        private int rectRowCount;
        private int rectColCount;
        private Rectangle[,] smallRects;
        private Rectangle frameLeftRect;
        private Rectangle frameRightRect;
        private Mat matObj;

        //纯私有
        private VideoCapture cap;
        private bool isLastColUncompleted;
        private bool isLastRowUncompleted;
        

        public MyVideo(String sourceVideoFileName, int smallRectWidthInPix)
        {
            this.sourceVideoFileName = sourceVideoFileName;
            this.smallRectWidthInPix = smallRectWidthInPix;
            matObj = new Mat();
            this.CaptureVideo();
            this.InitVideoInfo();
            this.InitRectsInfo();
        }

        public void InitVideoInfo()
        {
            this.frameCount = (int)cap.GetCaptureProperty(CapProp.FrameCount);
            this.frameHeight = (int)cap.GetCaptureProperty(CapProp.FrameHeight);
            this.frameWidth = (int)cap.GetCaptureProperty(CapProp.FrameWidth);
            this.fps = cap.GetCaptureProperty(CapProp.Fps);
            this.currentFrameIndex = 0;
            this.frameLeftRect = new Rectangle(0, 0, this.frameWidth / 2, this.frameHeight);
            this.frameRightRect = new Rectangle(this.frameWidth / 2, 0, this.frameWidth / 2, this.frameHeight);
        }

        public void CaptureVideo()
        {
            this.cap = new VideoCapture(sourceVideoFileName);
            this.currentFrameIndex = 0;
        }

        public Size GetVideoSize()
        {
            return new Size(this.frameWidth, this.frameHeight);
        }
        public void InitRectsInfo()
        {
            isLastColUncompleted = this.frameWidth % this.smallRectWidthInPix > 0;
            isLastRowUncompleted = this.frameHeight % this.smallRectWidthInPix > 0;

            this.rectColCount = this.frameWidth / this.smallRectWidthInPix + (isLastColUncompleted ? 1 : 0);
            this.rectRowCount = this.frameHeight / this.smallRectWidthInPix + (isLastRowUncompleted ? 1 : 0);

            smallRects = new Rectangle[this.rectRowCount, this.rectColCount];
            //Parallel.For(0, this.rectRowCount, i =>
            //{
            //    for (int j = 0; j < this.rectColCount; j++)
            //    {
            //        smallRects[i, j].X = this.smallRectWidthInPix * j;
            //        smallRects[i, j].Y = this.smallRectWidthInPix * i;

            //        smallRects[i, j].Width = Math.Min(this.frameWidth, this.smallRectWidthInPix * (j + 1)) - smallRects[i, j].X;
            //        smallRects[i, j].Height = Math.Min(this.frameHeight, this.smallRectWidthInPix * (i + 1)) - smallRects[i, j].Y;
            //    }
            //});
            for (int i = 0; i < this.rectRowCount; i++)
            {
                for (int j = 0; j < this.rectColCount; j++)
                {
                    smallRects[i, j].X = this.smallRectWidthInPix * j;
                    smallRects[i, j].Y = this.smallRectWidthInPix * i;

                    smallRects[i, j].Width = Math.Min(this.frameWidth, this.smallRectWidthInPix * (j + 1)) - smallRects[i, j].X;
                    smallRects[i, j].Height = Math.Min(this.frameHeight, this.smallRectWidthInPix * (i + 1)) - smallRects[i, j].Y;
                }
            }
        }

        public int[,] GetRectsPosition(int rowIndex, int colIndex)
        {
            int[,] retValue = new int[2, 2];
            try
            {
                retValue[0, 0] = SmallRects[rowIndex, colIndex].Left;
                retValue[0, 1] = SmallRects[rowIndex, colIndex].Top;
                retValue[1, 0] = SmallRects[rowIndex, colIndex].Right;
                retValue[1, 1] = SmallRects[rowIndex, colIndex].Bottom;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return retValue;
        }
        public Rectangle GetRectsPositionRectangle(int rowIndex, int colIndex)
        {
            try
            {
                return smallRects[rowIndex, colIndex];

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return smallRects[0,0];
        }

        public void ReCapVideo()
        {
            //this.cap.SetCaptureProperty(CapProp.FrameCount, 0.0); //wrong
            this.cap.Dispose();
            this.CaptureVideo();
            this.InitVideoInfo();
            //this.smallRectWidthInPix = Math.Min(this.smallRectWidthInPix, Math.Min(this.frameHeight, this.FrameWidth));
            this.InitRectsInfo();
        }

        public void Dispose()
        {
            cap.Dispose();
            matObj.Dispose();
        }

        public Image<Bgr, Byte> ReadVideoImage()
        {
            this.currentFrameIndex++;
            cap.Read(this.matObj);
            return this.matObj.ToImage<Bgr, Byte>();
        }
        public void ReadVideoMat()
        {
            this.currentFrameIndex++;
            cap.Read(this.matObj);
            //return mat;
        }

        public void Release()
        {
            this.cap.Dispose();
            currentFrameIndex = 0;
        }

        public Image<Bgr, Byte> GetAverageFrame(double calcRate, bool offOrOnPrintInfo = false)
        {
            if(this.currentFrameIndex != 0)
            {
                this.ReCapVideo();
            }

            calcRate = Math.Min(1.0, calcRate);
            int skippedEach = (int)(1.0 / calcRate);

            this.ReadVideoMat();
            Image<Bgr, int> averageFrame = this.MatObj.ToImage<Bgr, int>();
            int tempImageCount = 1;
            double finishRate = 1.0 / this.frameCount * 100;

            if (offOrOnPrintInfo)
            {
                Console.Write(String.Format("Getting Average Frame... {0,4:F2} %\t\t", finishRate) + 
                    new String('▋', (int)finishRate / 5));
            }

            for (int i = 1; i < this.frameCount; i++)
            {

                this.ReadVideoMat();
                if (i % skippedEach == 0)
                {
                    Image<Bgr, int> tmpImage = this.MatObj.ToImage<Bgr, int>();
                    averageFrame += tmpImage;
                    tempImageCount++;
                    tmpImage.Dispose();
                }
                if (offOrOnPrintInfo)
                {
                    finishRate = (double)i / this.frameCount * 100;
                    Console.Write("\r" + new String(' ', 70) + "\r" + String.Format("Getting Average Frame... {0,4:F2} %\t\t", finishRate) +
                    new String('▋', (int)finishRate / 5));
                }
            }

            #region WrongBlock
            //Wrong thought Seems Higher Efficiency
            //for (int i = 1; i < calcRate * this.frameCount; i++)
            //{
            //    cap.SetCaptureProperty(CapProp.FrameCount, this.currentFrameIndex + skippedEach);
            //    this.currentFrameIndex += skippedEach - 1;
            //    Image<Bgr, int> tmpImage = this.ReadVideoImageMat().ToImage<Bgr, int>();
            //    averageFrame += tmpImage;
            //    tmpImage.Dispose();
            //    tempImageCount++;

            //    if (offOrOnPrintInfo)
            //    {
            //        finishRate = (double)this.currentFrameIndex / this.frameCount * 100;
            //        Console.Write("\r" + new String(' ', 70) + "\r" + String.Format("Getting Average Frame... {0,4:F2} %\t\t", finishRate) +
            //        new String('▋', (int)finishRate / 5));
            //    }
            //}
#endregion WrongBlock
            Console.WriteLine();

            averageFrame /= tempImageCount;
            Image<Bgr, Byte> averageFrameInByte = averageFrame.Convert<Bgr, Byte>();
            averageFrame.Dispose();
            averageFrame.Mat.Dispose();
            return averageFrameInByte;
        }

        public ArrayList GetFrameList()
        {
            ArrayList frameList = new ArrayList();
            if(this.currentFrameIndex != 0)
            {
                this.ReCapVideo();
            }

            for(int i = 0; i < this.frameCount; i++)
            {
                frameList.Add(this.ReadVideoImage());
            }

            return frameList;
        }

        public string SourceVideoFileName => sourceVideoFileName;

        public int SmallRectWidthInPix => smallRectWidthInPix;

        public int FrameWidth { get => frameWidth; }
        public int FrameHeight { get => frameHeight;}
        public int FrameCount { get => frameCount; }
        public double Fps { get => fps; }
        public int CurrentFrameIndex { get => currentFrameIndex; }
        public int RectRowCount { get => rectRowCount;}
        public int RectColCount { get => rectColCount;}
        public Rectangle[,] SmallRects { get => smallRects;}
        public Rectangle FrameLeftRect { get => frameLeftRect;  }
        public Rectangle FrameRightRect { get => frameRightRect; }
        public Mat MatObj { get => matObj;}
    }
}
