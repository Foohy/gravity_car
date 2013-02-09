﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;

namespace Two_and_a_Half_Dimensions
{
    class Editor
    {
        public static bool Active = false;

        public static float Zoom = 1.0f;
        private static float goalZoom = -20f;

        private static Vector3 Pos = new Vector3();
        private static float multiplier = 8;

        private static float halfPI = 1.570796326794896f;
        public static void Init()
        {
            Pos = new Vector3(Player.ply.Pos.X, Player.ply.Pos.Y, -20.0f);
        }

        public static void Think(FrameEventArgs e)
        {
            Input.LockMouse = false;
            //curve dat zoom mmm girl u fine
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageDown]) goalZoom -= ((float)Utilities.Frametime * 1000);
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageUp]) goalZoom += ((float)Utilities.Frametime * 1000);


            goalZoom += Input.deltaZ;
            Zoom += (goalZoom - Zoom) / 4;
            Pos = new Vector3(Pos.X, Pos.Y, Zoom);

            //How fast should we move
            multiplier = 8;
            if (Utilities.window.Keyboard[Key.LShift])
            {
                multiplier = 15;
            }
            else if (Utilities.window.Keyboard[Key.LControl])
            {
                multiplier = 3;
            }
            

            //I SAID MOVE
            if (Utilities.window.Keyboard[Key.W])
            {
                Pos += new Vector3(0.0f, (float)e.Time, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.A])
            {
                Pos += new Vector3((float)e.Time, 0.0f, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.S])
            {
                Pos += new Vector3(0.0f, -(float)e.Time, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.D])
            {
                Pos += new Vector3(-(float)e.Time, 0.0f, 0.0f) * multiplier;
            }
        }

        public static void Draw(FrameEventArgs e)
        {
            //Set the camera matrix itself
            Player.ply.CamAngle = new Vector2d(halfPI, 0.0f); //Clamp it because I can't math correctly

            //find the point where we'll be facing
            Vector3 point = new Vector3((float)Math.Cos(halfPI), (float)Math.Sin(0), (float)Math.Sin(halfPI));

            Player.ply.ViewNormal = point;
            Player.ply.ViewNormal.Normalize();
            Player.ply.camMatrix = Matrix4.LookAt(Pos, (Pos + point), Vector3.UnitY);

            Utilities.ProjectionMatrix = Player.ply.camMatrix;

            Player.ply.SetPos(Pos);
        }

        public static void Stop()
        {

        }
    }
}
