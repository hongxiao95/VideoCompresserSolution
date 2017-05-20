using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Collections;

namespace VideoCompresserDFW
{
    class VibeAve
    {
        public const int NUM_SAMPLES = 20;
        public const int MIN_MATCHES = 2;
        public const int RADIUS = 20;
        public const int SUBSAMPLE_FACTOR = 16;

        private readonly int[] c_xoff = {-1,0,1,-1,1,-1,0,1,0};
        private readonly int[] c_yoff = { -1, 0, 1, -1, 1, -1, 0, 1, 0 };


        public void init(Image<Bgr, Byte> _image)
        {
            //m_samples = new Image<Gray, byte>[NUM_SAMPLES];
            m_samples = new ArrayList();
            for (int i = 0; i < NUM_SAMPLES; i++)
            {
                m_samples.Add(new Byte[_image.Height, _image.Width, 1]);
                //m_samples[i] = new Image<Gray, Byte>(_image.Width, _image.Height, new Gray(0));
            }
            //m_mask = new Image<Gray, Byte>(_image.Width, _image.Height, new Gray(0));
            m_mask = new Byte[_image.Height, _image.Width, 1];

            //m_foreMatchCount = new Image<Gray, Byte>(_image.Width, _image.Height, new Gray(0));
            m_foreMatchCount = new Byte[_image.Height, _image.Width];
        }

        public void processFirstFrame(Image<Bgr, Byte> _image_bgr)
        {
            Image<Gray, Byte> _image = _image_bgr.Convert<Gray, Byte>();
            Random rd = new Random();

            int row = 0;
            int col = 0;

            for(int i =0; i < _image.Rows; i++)
            {
                for(int j = 0; j < _image.Cols; j++)
                {
                    for(int k =0; k < NUM_SAMPLES; k++)
                    {
                        int rdNum = rd.Next(0, 9);

                        row = i + c_yoff[rdNum];
                        if (row < 0) row = 0;
                        if (row >= _image.Rows) row = _image.Rows - 1;

                        col = j + c_xoff[rdNum];
                        if (col < 0) col = 0;
                        if (col >= _image.Cols) col = _image.Cols - 1;

                        (m_samples[k] as Byte[,,])[i, j, 0] = _image.Data[row, col, 0];
                    }
                }
            }
        }                   

        public void testAndUpdate(Image<Bgr, Byte> _image_bgr)
        {
            Image<Gray, Byte> _image = _image_bgr.Convert<Gray, Byte>();
            
            Random rd = new Random();

            for(int i = 0; i < _image.Rows; i++)
            {
                for(int j =0; j < _image.Cols; j++)
                {
                    int matchs = 0;
                    int count = 0;
                    double dist = 0.0;

                    while(matchs < MIN_MATCHES && count < NUM_SAMPLES)
                    {
                        dist = Math.Abs((m_samples[count] as Byte[,,])[i, j, 0] - _image[i, j].Intensity);
                        if(dist < RADIUS)
                        {
                            matchs++;
                        }

                        count++;
                    }

                    if(matchs >= MIN_MATCHES)
                    {
                        m_foreMatchCount[i, j] = 0;
                        m_mask[i, j, 0] = 0;

                        int rdNum = rd.Next(0, SUBSAMPLE_FACTOR);
                        if(rdNum == 0)
                        {
                            rdNum = rd.Next(0, NUM_SAMPLES);
                            (m_samples[rdNum] as Byte[,,])[i, j, 0] = _image.Data[i, j, 0];
                        }

                        rdNum = rd.Next(0, SUBSAMPLE_FACTOR);
                        if(rdNum == 0)
                        {
                            rdNum = rd.Next(0, 9);
                            int row = 0;
                            int col = 0;

                            row = i + c_yoff[rdNum];
                            if (row < 0) row = 0;
                            if (row >= _image.Rows) row = _image.Rows - 1;

                            col = j + c_xoff[rdNum];
                            if (col < 0) col = 0;
                            if (col >= _image.Cols) col = _image.Cols - 1;


                            rdNum = rd.Next(0, NUM_SAMPLES);
                            (m_samples[rdNum] as Byte[,,])[i, j, 0] = _image.Data[i, j, 0];

                        }
                    }
                    else
                    {
                        m_foreMatchCount[i, j]++;
                        m_mask[i, j, 0] = 255;

                        if (m_foreMatchCount[i, j] > 60)
                        {
                            int rdNum = rd.Next(0, SUBSAMPLE_FACTOR);
                            if(rdNum == 0)
                            {
                                rdNum = rd.Next(0, NUM_SAMPLES);
                                (m_samples[rdNum] as Byte[,,])[i, j, 0] = _image.Data[i, j, 0];
                            }
                        }
                    }
                }
            }
        }

        public Image<Gray, Byte> getMask()
        {
            return new Image<Gray, Byte>(m_mask);
        }

        //private Image<Gray, Byte>[] m_samples;
        private ArrayList m_samples;
        //private Image<Gray, Byte> m_foreMatchCount;
        private Byte[,] m_foreMatchCount;
        private Byte[,,] m_mask;
    }
}
