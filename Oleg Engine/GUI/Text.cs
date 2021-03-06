﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine.GUI
{
    public class Text
    {
        public struct CharDescriptor
        {
            public ushort x, y;
            public ushort Width, Height;
            public ushort XOffset, YOffset;
            public ushort XAdvance;
        }

        public class Charset
        {
            public ushort LineHeight;
            public ushort Base;
            public ushort Width, Height;
            public CharDescriptor[] Chars;
            public Material CharsetMaterial;
            public string FontName;

            public Charset()
            {
                Chars = new CharDescriptor[256];
            }

            public static implicit operator bool(Charset p)
            {
                return p != null && p.CharsetMaterial != null && p.Width > 0;
            }
        }

        public const string FontPath = "Resources/Fonts/";
        public const string FontmapPath = "gui/fontmaps/"; //Relative to the directory for materials
        public string CurrentText;

        public static void ConstructVertexArray(Charset ch, string str, out Vertex[] verts, out int[] elements )
        {
            ushort CharX;
            ushort CharY;
            ushort Width;
            ushort Height;
            ushort OffsetX;
            ushort OffsetY;
            int CurX = 0;

            //vert stuff
            verts = new Vertex[str.Length * 4];
            elements = new int[str.Length * 4];

            for (int i = 0; i < str.Length; i++)
            {
                char curChar = str[i];
                if (curChar > ch.Chars.Length) continue;

                CharX = ch.Chars[curChar].x;
                CharY = ch.Chars[curChar].y;
                Width = ch.Chars[curChar].Width;
                Height = ch.Chars[curChar].Height;
                OffsetX = ch.Chars[curChar].XOffset;
                OffsetY = ch.Chars[curChar].YOffset;

                //upper left
                verts[i * 4].UV.X = (float)CharX / (float)ch.Width;
                verts[i * 4].UV.Y = (float)CharY / (float)ch.Height;
                verts[i * 4].Position.X = (float)CurX + OffsetX;
                verts[i * 4].Position.Y = (float)OffsetY;
                verts[i * 4].Color = (Vector3h)Vector3.One;
                elements[i*4] = i*4;

                //upper right
                verts[i * 4 + 1].UV.X = (float)(CharX + Width) / (float)ch.Width;
                verts[i * 4 + 1].UV.Y = (float)CharY / (float)ch.Height;
                verts[i * 4 + 1].Position.X = (float)Width + CurX + OffsetX;
                verts[i * 4 + 1].Position.Y = (float)OffsetY;
                verts[i * 4 + 1].Color = (Vector3h)Vector3.One;
                elements[i*4 + 1] = i*4 + 1;

                //lower right
                verts[i * 4 + 2].UV.X = (float)(CharX + Width) / (float)ch.Width;
                verts[i * 4 + 2].UV.Y = (float)(CharY + Height) / (float)ch.Height;
                verts[i * 4 + 2].Position.X = (float)Width + CurX + OffsetX;
                verts[i * 4 + 2].Position.Y = (float)Height + OffsetY;
                verts[i * 4 + 2].Color = (Vector3h)Vector3.One;
                elements[i*4 + 2] = i*4 + 2;

                //lower left
                verts[i * 4 + 3].UV.X = (float)CharX / (float)ch.Width;
                verts[i * 4 + 3].UV.Y = (float)(CharY + Height) / (float)ch.Height;
                verts[i * 4 + 3].Position.X = (float)CurX + OffsetX;
                verts[i * 4 + 3].Position.Y = (float)Height + OffsetY;
                verts[i * 4 + 3].Color = (Vector3h)Vector3.One;
                elements[i*4 + 3] = i*4 + 3;


                CurX += ch.Chars[curChar].XAdvance;
            }
        }

        public static Charset ParseFont(string FNT)
        {
            Charset charset = new Charset();
            charset.FontName = FNT;

            if (!File.Exists(Resource.FontDir + FNT + ".fnt"))
            {
                Utilities.Print("Failed to load font '{0}'", Utilities.PrintCode.ERROR, FNT);
                return charset;
            }
            //Load the texture
            if (!File.Exists(Resource.FontDir + FNT + ".png"))
            {
                Utilities.Print("Failed to load fontmap '{0}'", Utilities.PrintCode.ERROR, FNT);
                return charset;
            }
            var mapbmp = new System.Drawing.Bitmap(Resource.FontDir + FNT + ".png");
            charset.CharsetMaterial = new Material(Utilities.LoadTexture(mapbmp), Resource.GetProgram("hud"));
            mapbmp.Dispose();

            StreamReader sr = new StreamReader(Resource.FontDir + FNT + ".fnt");

            string Read, Key, Value;
            string FirstWord;

            while (!sr.EndOfStream)
            {
                Read = sr.ReadLine();
                FirstWord = GetFirstWord(Read);
                switch (FirstWord)
                {
                    case "common":
                        string[] sections = Read.Split(' ');
                        for (int i = 0; i < sections.Length; i++)
                        {
                            Key = GetFirstWord(sections[i]);
                            Value = sections[i].Substring(sections[i].IndexOf('=') + 1);
                            switch (Key)
                            {
                                case "lineHeight":
                                    charset.LineHeight = StringToShort(Value);
                                    break;

                                case "base":
                                    charset.Base = StringToShort(Value);
                                    break;

                                case "scaleW":
                                    charset.Width = StringToShort(Value);
                                    break;

                                case "scaleH":
                                    charset.Height = StringToShort(Value);
                                    break;
                            }
                        }

                        break;

                    case "char":
                        ushort CharID = 0;
                        string[] keys = Read.Split(' ');
                        for (int i = 0; i < keys.Length; i++)
                        {
                            Key = GetFirstWord(keys[i]);
                            Value = keys[i].Substring(keys[i].IndexOf('=') + 1);
                            if (CharID > charset.Chars.Length) continue;
                            switch (Key)
                            {
                                case "id":
                                    CharID = StringToShort(Value);
                                    break;

                                case "x":
                                    charset.Chars[CharID].x = StringToShort(Value);
                                    break;

                                case "y":
                                    charset.Chars[CharID].y = StringToShort(Value);
                                    break;

                                case "width":
                                    charset.Chars[CharID].Width = StringToShort(Value);
                                    break;

                                case "height":
                                    charset.Chars[CharID].Height = StringToShort(Value);
                                    break;

                                case "xoffset":
                                    charset.Chars[CharID].XOffset = StringToShort(Value);
                                    break;

                                case "yoffset":
                                    charset.Chars[CharID].YOffset = StringToShort(Value);
                                    break;

                                case "xadvance":
                                    charset.Chars[CharID].XAdvance = StringToShort(Value);
                                    break;

                            }

                        }
                        break;
                }

            }

            sr.Dispose();

            return charset;
        }

        private static ushort StringToShort(string str, ushort def = 1)
        {
            ushort.TryParse(str, out def);
            return def;
        }

        private static string GetFirstWord( string str )
        {
            char[] limit = {' ', '='};
            if (str.IndexOfAny(limit) <= 0) return "";
            return str.Substring(0, str.IndexOfAny(limit));
        }


        //instanced class
        public Charset charset { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public float ScaleW { get; private set; }
        public float ScaleH { get; private set; }
        public Vector3 Color = Vector3.One;

        private Matrix4 view;
        private Mesh textMesh;
        public Text( string font, string text )
        {
            this.charset = Resource.GetCharset(font);

            //Create a blank vertex buffer which we'll update later with our text info
            textMesh = new Mesh(new Vertex[0], new int[0]);
            textMesh.DrawMode = BeginMode.Quads;
            textMesh.ShouldDrawDebugInfo = false;

            this.SetText(text);
            this.textMesh.mat = this.charset.CharsetMaterial;

            this.ScaleH = 1;
            this.ScaleW = 1;

            this.UpdateMatrix(); //Update the model matrix
        }

        public void SetPos(float x, float y)
        {
            this.X = (int)x;
            this.Y = (int)y;

            this.UpdateMatrix();
        }

        public void SetScale(float Width, float Height)
        {
            this.ScaleW = Width;
            this.ScaleH = Height;

            this.UpdateMatrix();
        }

        public void SetColor(Vector3 colorvec)
        {
            this.Color = colorvec;
            this.textMesh.Color = this.Color;
        }

        public void SetColor(float R, float G, float B)
        {
            this.Color = new Vector3(R, G, B);
            this.textMesh.Color = this.Color;
        }

        public void UpdateMatrix()
        {
            view = Matrix4.CreateTranslation(Vector3.Zero);
            view *= Matrix4.Scale(this.ScaleW, this.ScaleH, 1.0f);
            view *= Matrix4.CreateTranslation(this.X, this.Y, 3.0f);
        }

        /// <summary>
        /// Get the length, in pixels, of the given text string/charset combo
        /// </summary>
        /// <returns>The length in pixels of the string</returns>
        public float GetTextLength( string str )
        {
            if (string.IsNullOrEmpty(str)) return 0;

            float CurX = 0;
            float StartX = 0;
            float EndX = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] > this.charset.Chars.Length) continue;

                if (i == 0) StartX = this.charset.Chars[str[i]].XOffset;
                if (i == str.Length - 1) EndX = this.charset.Chars[str[i]].Width + CurX + this.charset.Chars[str[i]].XOffset;

                CurX += this.charset.Chars[str[i]].XAdvance;
            }

            return (EndX - StartX);
        }

        /// <summary>
        /// Get the length, in pixels, of the given text string/charset combo
        /// </summary>
        /// <returns>The length in pixels of the string</returns>
        public float GetTextLength()
        {
            return GetTextLength(this.CurrentText);
        }

        /// <summary>
        /// Get the height, in pixels, of the text's font
        /// </summary>
        /// <returns></returns>
        public float GetTextHeight()
        {
            return this.charset.LineHeight * ScaleH;
        }

        /// <summary>
        /// Given an x value, return the character index closest to that value. Useful for text selection.
        /// </summary>
        /// <param name="x">X value of the 'pointer' or to be compared to.</param>
        /// <param name="str">The string to test against</param>
        /// <returns>Character index of the string</returns>
        public int GetClosestCharacterIndex(float x, string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;

            float CurX = 0;
            float StartX = 0;
            float EndX = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] > this.charset.Chars.Length) continue;

                if (i == 0) StartX = this.charset.Chars[str[i]].XOffset;
                if (i == str.Length - 1) EndX = this.charset.Chars[str[i]].Width + CurX + this.charset.Chars[str[i]].XOffset;

                CurX += this.charset.Chars[str[i]].XAdvance;
                if (CurX > (x - this.X)) return i;
            }

            return (str.Length);
        }

        /// <summary>
        /// Given an x value, return the character index closest to that value. Useful for text selection.
        /// </summary>
        /// <param name="x">X value of the 'pointer' or to be compared to.</param>
        /// <returns>Character index of the string</returns>
        public int GetClosestCharacterIndex(float x)
        {
            return this.GetClosestCharacterIndex(x, this.CurrentText);
        }

        public bool GetCharExists(char character)
        {
            return character < this.charset.Chars.Length;
        }

        public void SetText(string text)
        {
            if (text == null || text == this.CurrentText) return;
            this.CurrentText = text;

            Vertex[] verts;
            int[] elements;
            ConstructVertexArray(this.charset, text, out verts, out elements);
            if (textMesh == null)
            {
                textMesh = new Mesh(verts, elements);
                textMesh.DrawMode = BeginMode.Quads;
            }
            else
            {
                textMesh.UpdateMesh(verts, elements);
            }
        }

        public void SetCharset(Charset ch)
        {
            this.charset = ch;
            this.textMesh.mat = ch.CharsetMaterial;
        }

        public void Draw()
        {
            if (this.textMesh == null) return;

            this.SetColor(this.Color); //HACK HACK: make some sort of render.SetColor
            this.textMesh.DrawSimple(view);
        }
    }
}
