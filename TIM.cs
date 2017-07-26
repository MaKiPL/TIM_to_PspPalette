using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

/*
 * TIM C# class by Marcin Gomulak (MaKiPL)
 * Taken from Rinoa's toolset
 * https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/FF8_Core/TIM.cs
 * modified for this software
 * See Rinoa's toolset license
 */

namespace TIMpal_to_JASC
{
    class TIM
    {
        private static byte[] _8BPP = { 0x10, 0x00, 0x00, 0x00, 0x09 };
        private static byte[] _4BPP = { 0x10, 0x00, 0x00, 0x00, 0x08 };
        private static byte[] _16BPP = { 0x10, 0x00, 0x00, 0x00, 0x02 };
        private static byte[] _24BPP = { 0x10, 0x00, 0x00, 0x00, 0x03 };
        private FileStream fs;
        private System.IO.BinaryReader br;
        private Texture texture;
        private Color[] colors;
        private sbyte bpp = -1;
        private bool arg0 = false;
        private string path;
        public static int _5bitColor = 255 / 31;

        private List<String> PS_format;

        public struct Texture
        {
            public ushort PaletteX;
            public ushort PaletteY;
            public ushort NumOfCluts;
            public byte[] ClutData;
            public ushort ImageOrgX;
            public ushort ImageOrgY;
            public ushort Width;
            public ushort Height;
        }

        public Texture GetParameters => texture;

        public string[] ConvertToPS => PS_format.ToArray();

        public struct Color
        {
            public byte R;
            public byte G;
            public byte B;
        }

        public TIM(string path, byte arg0 = 0)
        {
            bool paramsOnly = true;
            this.arg0 = arg0 != 0;
            this.path = path;
            fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(fs);
            texture = new Texture();
            bpp = RecognizeBPP();
            if (bpp == -1 && arg0 == 0)
            {
                Console.WriteLine("TIM: This is not TIM texture!");
                return;
            }
            if (arg0 == 0 && paramsOnly)
            {
                ReadParameters(bpp);
                PS_format = DrawTexture();
            }
            br.Dispose();
            fs.Dispose();
        }

        private List<String> DrawTexture()
        {
            if (texture.ClutData != null || bpp == 16 || bpp == 24)
            {
                if (bpp == 16 || bpp == 24) throw new Exception("NO PALETTES!");
                if (bpp == 8)
                {
                    colors = new Color[texture.NumOfCluts * 256];
                    int col = 0;
                    for (int i = 0; i != texture.ClutData.Length; i += 2)
                    {
                        byte[] b = BitConverter.GetBytes(BitConverter.ToUInt16(texture.ClutData, i));
                        BitArray ba = new BitArray(b);
                        BitArray B = new BitArray(5);
                        BitArray R = new BitArray(5);
                        BitArray G = new BitArray(5);
                        BitArray a = new BitArray(1);
                        B[0] = ba[10]; B[1] = ba[11]; B[2] = ba[12]; B[3] = ba[13]; B[4] = ba[14]; //R
                        R[0] = ba[0]; R[1] = ba[1]; R[2] = ba[2]; R[3] = ba[3]; R[4] = ba[4]; //G
                        G[0] = ba[5]; G[1] = ba[6]; G[2] = ba[7]; G[3] = ba[8]; G[4] = ba[9]; //B
                        a[0] = ba[15]; //Alpha if 0
                        int[] b_ = new int[1]; B.CopyTo(b_, 0);
                        int[] r_ = new int[1]; R.CopyTo(r_, 0);
                        int[] g_ = new int[1]; G.CopyTo(g_, 0);
                        int[] aa = new int[1]; a.CopyTo(aa, 0);
                        double bb = Math.Round((double)((b_[0]) * (256 / 32)));
                        double rr = Math.Round((double)((r_[0]) * 256 / 32));
                        double gg = Math.Round((double)((g_[0]) * 256 / 32));
                        if (bb > 255)
                            bb--;
                        if (rr > 255)
                            rr--;
                        if (gg > 255)
                            gg--;
                        colors[col].R = (byte)bb;
                        colors[col].G = (byte)rr;
                        colors[col].B = (byte)gg;
                        col++;
                    }
                    PS_format = new List<string>();

                    for (int k = 0; k < texture.NumOfCluts; k++)
                    {
                        PS_format.Add("JASC-PAL");
                        PS_format.Add("0100");
                        PS_format.Add("16");
                        for (int i = 0; i < 256; i++)
                            PS_format.Add($"{colors[i + k * 256].R} {colors[i + k * 256].G} {colors[i + k * 256].B}");
                    }
                    return PS_format;
                }
                if (bpp == 4)
                {
                    colors = new Color[texture.NumOfCluts * 16];
                    int col = 0;
                    for (int i = 0; i != texture.ClutData.Length; i += 2)
                    {
                        byte[] b = BitConverter.GetBytes(BitConverter.ToUInt16(texture.ClutData, i));
                        BitArray ba = new BitArray(b);
                        BitArray B = new BitArray(5);
                        BitArray R = new BitArray(5);
                        BitArray G = new BitArray(5);
                        BitArray a = new BitArray(1);
                        B[0] = ba[10]; B[1] = ba[11]; B[2] = ba[12]; B[3] = ba[13]; B[4] = ba[14]; //R
                        R[0] = ba[0]; R[1] = ba[1]; R[2] = ba[2]; R[3] = ba[3]; R[4] = ba[4]; //G
                        G[0] = ba[5]; G[1] = ba[6]; G[2] = ba[7]; G[3] = ba[8]; G[4] = ba[9]; //B
                        a[0] = ba[15]; //Alpha if 0
                        int[] b_ = new int[1]; B.CopyTo(b_, 0);
                        int[] r_ = new int[1]; R.CopyTo(r_, 0);
                        int[] g_ = new int[1]; G.CopyTo(g_, 0);
                        int[] aa = new int[1]; a.CopyTo(aa, 0);
                        double bb = Math.Round((double)((b_[0]) * (256 / 32)));
                        double rr = Math.Round((double)((r_[0]) * 256 / 32));
                        double gg = Math.Round((double)((g_[0]) * 256 / 32));
                        if (bb > 255)
                            bb--;
                        if (rr > 255)
                            rr--;
                        if (gg > 255)
                            gg--;
                        colors[col].R = (byte)bb;
                        colors[col].G = (byte)rr;
                        colors[col].B = (byte)gg;
                        col++;
                    }
                    //PS_format = new string[16 * texture.NumOfCluts + 3*texture.NumOfCluts];
                    PS_format = new List<string>();
                    /*for (int k = 0; k < texture.NumOfCluts; k++)
                    {
                        PS_format[0+k*16+3*k] = "JASC-PAL";
                        PS_format[1+k*16 + 3 * k] = "0100";
                        PS_format[2+k*16 + 3 * k] = "16";
                        for (int i = 0; i < 16; i++)
                            PS_format[3 * k + i + k * 16] = $"{colors[i + k * 16].R} {colors[i + k * 16].G} {colors[i + k * 16].B}";
                    }*/
                    for (int k = 0; k < texture.NumOfCluts; k++)
                    {
                        PS_format.Add("JASC-PAL");
                        PS_format.Add("0100");
                        PS_format.Add("16");
                        for (int i = 0; i < 16; i++)
                            PS_format.Add($"{colors[i + k * 16].R} {colors[i + k * 16].G} {colors[i + k * 16].B}");
                    }
                    return PS_format;
                }
            }
            return null;
        }

        
        private void ReadParameters(sbyte bpp)
        {
            if (bpp == 4)
            {
                fs.Seek(4, SeekOrigin.Current);
                texture.PaletteX = br.ReadUInt16();
                texture.PaletteY = br.ReadUInt16();
                fs.Seek(2, SeekOrigin.Current);
                texture.NumOfCluts = br.ReadUInt16();
                byte[] buffer = new byte[texture.NumOfCluts * 32];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = br.ReadByte();
                texture.ClutData = buffer;
                fs.Seek(4, SeekOrigin.Current);
                texture.ImageOrgX = br.ReadUInt16();
                texture.ImageOrgY = br.ReadUInt16();
                //Console.WriteLine($"TIM: OrigX: {texture.ImageOrgX}\tOrigY:{texture.ImageOrgY}");
                texture.Width = (ushort)(br.ReadUInt16() * 4);
                texture.Height = br.ReadUInt16();
                return;
            }
            if (bpp == 8)
            {
                fs.Seek(4, SeekOrigin.Current);
                texture.PaletteX = br.ReadUInt16();
                texture.PaletteY = br.ReadUInt16();
                fs.Seek(2, SeekOrigin.Current);
                texture.NumOfCluts = br.ReadUInt16();
                byte[] buffer = new byte[texture.NumOfCluts * 512];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = br.ReadByte();
                texture.ClutData = buffer;
                fs.Seek(4, SeekOrigin.Current);
                texture.ImageOrgX = br.ReadUInt16();
                texture.ImageOrgY = br.ReadUInt16();
                texture.Width = (ushort)(br.ReadUInt16() * 2);
                texture.Height = br.ReadUInt16();
                return;
            }
            if (bpp == 16)
            {
                fs.Seek(4, SeekOrigin.Current);
                texture.ImageOrgX = br.ReadUInt16();
                texture.ImageOrgY = br.ReadUInt16();
                texture.Width = br.ReadUInt16();
                texture.Height = br.ReadUInt16();
                return;
            }
            if (bpp != 24) return;
            fs.Seek(4, SeekOrigin.Current);
            texture.ImageOrgX = br.ReadUInt16();
            texture.ImageOrgY = br.ReadUInt16();
            texture.Width = (ushort)(br.ReadUInt16() / 1.5);
            texture.Height = br.ReadUInt16();
        }

        private sbyte RecognizeBPP()
        {
            byte[] buffer = br.ReadBytes(5);
            fs.Seek(3, SeekOrigin.Current);
            if (buffer.Equals(_4BPP))
                return 4;
            if (buffer[4] == 0x08)
                return 4;
            if (buffer[4] == 0x09)
                return 8;
            if (buffer[4] == 0x02)
                return 16;
            if (buffer[4] == 0x03)
                return 24;
            return -1;

        }
    }
}
