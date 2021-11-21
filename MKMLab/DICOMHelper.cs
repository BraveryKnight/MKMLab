using EvilDICOM.Core;
using EvilDICOM.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MKMLab
{
    
    struct data// структура для хранения информации о каждом файле из серии
    {
        public ushort bitsAllocated;//сколько бит выделено
        public ushort bitsStored;//кол-во бит в изображении
        public ushort rows;//кол-во строк
        public ushort colums;//кол-во столбцов
        public ushort pixelRepresentation;//пиксельное представление
        public List<byte> pixelData;//список пикселей для изображения 
        public ushort mask;//маска
        public double maxval;//максимальное значение
        public double width; //тег windowwidth
        public double center; //тег windowcenter
    }

    class DICOMHelper
    {
       
        static public Image MakeMIP(string[] files, string mode)
        {
            List<data> dataList = new List<data>();
           
            foreach (string name in files)//читаем серию файлов и записываем данные
            {
                var dcm = DICOMObject.Read(name);//читаем файл
                data d = new data();
                d.bitsAllocated = (ushort)dcm.FindFirst(TagHelper.BitsAllocated).DData;
                d.bitsStored = (ushort)dcm.FindFirst(TagHelper.BitsStored).DData;
                d.rows = (ushort)dcm.FindFirst(TagHelper.Rows).DData;
                d.colums = (ushort)dcm.FindFirst(TagHelper.Columns).DData;
                d.pixelRepresentation = (ushort)dcm.FindFirst(TagHelper.PixelRepresentation).DData;
                d.pixelData = (List<byte>)dcm.FindFirst(TagHelper.PixelData).DData_;
                d.mask = (ushort)(ushort.MaxValue >> (d.bitsAllocated - d.bitsStored));
                d.maxval = Math.Pow(2, d.bitsStored);
                d.width = (double)dcm.FindFirst(TagHelper.WindowWidth).DData;
                d.center = -(double)dcm.FindFirst(TagHelper.WindowCenter).DData;
                dataList.Add(d);
            }

            byte[] outPixelData = new byte[dataList[0].rows * dataList[0].colums * 4];//создаем массив для пикселей
            int index = 0;
            for (int i = 0; i < dataList[0].pixelData.Count; i += 2)
            {
                List<byte> resList = new List<byte>();

                foreach(data d in dataList) //расчитываем интенсивность каждого бита и загоням с список
                {
                    ushort gray = (ushort)(d.pixelData[i] + (ushort)(d.pixelData[i + 1] << 8));
                    double valgray = gray & d.mask;//удаление неиспользуемых битов

                    if (d.pixelRepresentation == 1)
                    {
                        //последний бит - знак, дополняем до 2
                        if (valgray > (d.maxval / 2))
                            valgray -= d.maxval;
                    }

                    double half = ((d.width - 1) / 2.0) - 0.5;

                    valgray = ((valgray - (d.center - 0.5)) / (d.width - 1) + 0.5) * 255;
                    resList.Add((byte)valgray);
                }
                byte b;
                switch (mode)
                {
                    case "MIP": b = getMax(resList); break;
                    case "mIP": b = getMin(resList); break;
                    default: b = getAvg(resList); break;
                }
                outPixelData[index] = b; //создание массива битов в зависимости от задания
                outPixelData[index + 1] = outPixelData[index];
                outPixelData[index + 2] = outPixelData[index];
                outPixelData[index + 3] = 255;
         
                index += 4;
            }

            return ImageFromRawBgraArray(outPixelData, dataList[0].colums, dataList[0].rows);
        }

        static public byte getMax(List<byte> lst)
        {
            byte res = 0;
            foreach(byte b in lst)
            {
                res = b > res ? b : res;
            }
            return res;
        }

        static public byte getMin(List<byte> lst)
        {
            byte res = 255;
            foreach (byte b in lst)
            {
                res = b < res ? b : res;
            }
            return res;
        }

        static public byte getAvg(List<byte> lst)
        {
            int res = 0;
            foreach (byte b in lst)
            {
                res += b;
            }
            return (byte)(res / lst.Count);
        }

        static public Image ImageFromRawBgraArray( //метод для создания картинки из массива байтов
           byte[] arr, int width, int height)
        {
            var output = new Bitmap(width, height);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = output.LockBits(rect,
                ImageLockMode.ReadWrite, output.PixelFormat);
            var ptr = bmpData.Scan0;
            Marshal.Copy(arr, 0, ptr, arr.Length);
            output.UnlockBits(bmpData);
            return output;
        }
    }
}
