﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using OlegEngine;
using OlegEngine.Entity;
using OlegEngine.GUI;

using Gravity_Car.Entity;

namespace OlegEngine
{
    enum EditMode
    {
        CreatePhys,
        CreateEnt,
        Transform
    }

    class Editor
    {
        public static bool Active = false;
        public static float Zoom = 1.0f;
        public static EditMode CurrentMode = EditMode.CreatePhys;
        public static Vector3 MousePos = new Vector3();
        public static BaseEntity SelectedEnt;
        public static ent_cursor Cursor;
        public static Vector3 ViewPosition = new Vector3();

        private static Text CurrentModeText;
        private static float goalZoom = 5.0f;
        private static float multiplier = 8;

        //Editor GUI
        private static Toolbar TopControl;

        private static List<Vector2> Points = new List<Vector2>();
        
        
        public static void Init()
        {
            ViewPosition = new Vector3(View.Player.Position);

            Cursor = EntManager.Create<ent_cursor>();
            Cursor.Spawn();
            Cursor.SetPos(new Vector3(0, 0, 0));

            Input.LockMouse = false;
            //Cursor.Scale = Vector3.One * 0.25f;


            //Slap some text on the screen
            CurrentModeText = new GUI.Text("debug", "Mode: " + CurrentMode.ToString());
            CurrentModeText.SetPos(Utilities.engine.Width - 200, Utilities.engine.Height - CurrentModeText.GetTextHeight() );
            GUI.GUIManager.PostDrawHUD += new GUI.GUIManager.OnDrawHUD(GUIManager_PostDrawHUD);

            Utilities.engine.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.engine.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
            Utilities.engine.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
            View.CalcView += new Action(View_CalcView);

            //Create our GUI stuff, if neccessary
            if (TopControl == null)
            {
                TopControl = GUIManager.Create<Toolbar>();
                TopControl.SetWidth(Utilities.engine.Width);

                ContextMenu dd = TopControl.AddButtonDropDown("File");
                dd.AddButton("Load");
                dd.AddButton("Save").SetEnabled(false);
                dd.AddButton("Exit").OnButtonPress += new Button.OnButtonPressDel(exit_OnButtonPress);


                TopControl.AddButton("Edit");
                Button help = TopControl.AddButton("Help...");
                help.OnButtonPress += new Button.OnButtonPressDel(help_OnButtonPress);

                Button tests = TopControl.AddButton("Panel Tests");
                tests.OnButtonPress += new Button.OnButtonPressDel(tests_OnButtonPress);

            }

            TopControl.IsVisible = true;
        }

        static void tests_OnButtonPress(Panel sender)
        {
            Window mainwin = GUIManager.Create<Window>();
            mainwin.SetPos(300, 200);
            mainwin.SetWidth(500);
            mainwin.SetHeight(450);
            mainwin.SetTitle(Utilities.Time.ToString());

            Window subWin = GUIManager.Create<Window>(mainwin);
            subWin.SetPos(20, 30);
            subWin.SetWidth(330);
            subWin.SetTitle("I'm a window within a window!");

            subWin = GUIManager.Create<Window>(subWin);
            subWin.SetPos(20, 30);
            subWin.SetHeight(180);
            subWin.SetWidth(120);
            subWin.SetTitle("I'm another!");

            subWin = GUIManager.Create<Window>(subWin);
            subWin.SetPos(20, 30);
            subWin.SetHeight(40);
            subWin.SetWidth(80);
            subWin.SetTitle("Budding!");

        }

        static void exit_OnButtonPress(Panel sender)
        {
            Window exitMessageBox = GUIManager.Create<Window>();
            exitMessageBox.SetTitle("Hold the fucking phone");
            exitMessageBox.ClipChildren = true;
            exitMessageBox.SetPos((Utilities.engine.Width / 2) - (exitMessageBox.Width / 2), (Utilities.engine.Height / 2) - (exitMessageBox.Height / 2));
            exitMessageBox.SetWidth(205);
            exitMessageBox.SetHeight(75);

            Label label = GUIManager.Create<Label>(exitMessageBox);
            label.Autosize = true;
            label.SetText("Are you sure you'd like to leave?");
            label.SetPos(15, 10);

            Button button = GUIManager.Create<Button>(exitMessageBox);
            button.SetText("Yes");
            button.SizeToText(15);
            button.DockPadding(20, 20, 20, 20);
            button.SetHeight(20);
            button.SetPos(20, 40);
            button.SetAnchorStyle(Panel.Anchors.Bottom);
            button.OnButtonPress += new Button.OnButtonPressDel(button_OnYesButtonPress);

            button = GUIManager.Create<Button>(exitMessageBox);
            button.SetText("Cancel");
            button.SizeToText(15);
            button.DockPadding(20, 20, 20, 20);
            button.SetHeight(20);
            button.SetPos(exitMessageBox.Width - button.Width - 20, 40);
            button.SetAnchorStyle(Panel.Anchors.Right | Panel.Anchors.Bottom );
            button.OnButtonPress += new Button.OnButtonPressDel(button_OnNoButtonPress);
        }

        static void button_OnYesButtonPress(Panel sender)
        {
            Utilities.engine.Exit();
        }

        static void button_OnNoButtonPress(Panel sender)
        {
            sender.Parent.Remove();
        }

        static void help_OnButtonPress(Panel sender)
        {
            Window w = GUIManager.Create<Window>();
            w.SetTitle("So you need help?");
            w.ClipChildren = true;
            w.SetHeight(200);

            Button button = GUIManager.Create<Button>();
            button.SetText("Clip test!");
            button.SizeToText(15);
            button.SetParent(w);
            button.SetPos(new Vector2((w.Width / 2) - (button.Width / 2), w.Height - 50));
            button.DockPadding(20, 20, 20, 20);
            button.SetHeight(70);
            button.Dock(Panel.DockStyle.TOP);

            Slider slider = GUIManager.Create<Slider>(w);
            slider.SetMinMax(-10, 100);
            slider.SetValue(40);
            slider.SetWidthHeight(w.Width - 60, 20);
            slider.SetPos(40, 0);
            slider.Below(button, 5);
            slider.SetAnchorStyle(Panel.Anchors.Left | Panel.Anchors.Right);
            slider.OnValueChanged += new Action<Panel, float>(slider_OnValueChanged);

            Label l = GUIManager.Create<Label>(w);
            l.SetText("0");
            l.SetPos(10, slider.Position.Y);
            l.SetWidthHeight(25, 20);
            l.SetAlignment(Label.TextAlign.MiddleLeft);
            slider.Userdata = l;

            Button disabled = GUIManager.Create<Button>(w);
            disabled.SetText("I'm disabled :(");
            disabled.SetWidthHeight(slider.Width, slider.Height);
            disabled.Below(slider, 5);
            disabled.DockPadding(20, 20, 20, 20);
            disabled.Dock(Panel.DockStyle.BOTTOM);
            disabled.SetEnabled(false);
        }

        static void slider_OnValueChanged(Panel sender, float newval)
        {
            if (sender.Userdata is Label)
            {
                Label l = (Label)sender.Userdata;
                l.SetText(newval.ToString());
            }
        }

        static void GUIManager_PostDrawHUD(EventArgs e)
        {
            CurrentModeText.Draw();
        }

        static void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            switch (CurrentMode)
            {
                case EditMode.CreateEnt:
                    CreateEnt.KeyDown(e);
                    break;

                case EditMode.CreatePhys:
                    CreatePhys.KeyDown(e);
                    break;

                case EditMode.Transform:
                    Transform.KeyDown(e);
                    break;
            }

            if (e.Key == Key.Left)
            {
                CurrentMode--;
                if (CurrentMode < 0) CurrentMode = (EditMode)Enum.GetNames(typeof(EditMode)).Length-1;

                CurrentModeText.SetText("Mode: " + CurrentMode.ToString());
            }
            if (e.Key == Key.Right)
            {
                CurrentMode++;
                if ((int)CurrentMode > Enum.GetNames(typeof(EditMode)).Length-1) CurrentMode = EditMode.CreatePhys;

                CurrentModeText.SetText("Mode: " + CurrentMode.ToString());
            }
        }

        static void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentMode)
            {
                case EditMode.CreateEnt:
                    CreateEnt.MouseUp(e);
                    break;

                case EditMode.CreatePhys:
                    CreatePhys.MouseUp(e);
                    break;

                case EditMode.Transform:
                    Transform.MouseUp(e);
                    break;
            }
        }

        static void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GUIManager.IsMouseOverElement) return;

            switch (CurrentMode)
            {
                case EditMode.CreateEnt:
                    CreateEnt.MouseDown(e);
                    break;

                case EditMode.CreatePhys:
                    CreatePhys.MouseDown(e);
                    break;

                case EditMode.Transform:
                    Transform.MouseDown(e);
                    break;
            }
        }

        public static void SetSelected(BaseEntity e)
        {
            //Reset the previous selected ents color
            if (SelectedEnt != null)
            {
                SelectedEnt.Color = Vector3.One;
            }
            SelectedEnt = e;

            //Set the new entity's color
            if (SelectedEnt != null)
            {
                SelectedEnt.Color = new Vector3(0, 1.0f, 0);
            }
        }

        public static void Think()
        {
            Input.LockMouse = false;
            //curve dat zoom mmm girl u fine
            if (Utilities.engine.Keyboard[OpenTK.Input.Key.PageDown]) goalZoom -= ((float)Utilities.ThinkTime * 100);
            if (Utilities.engine.Keyboard[OpenTK.Input.Key.PageUp]) goalZoom += ((float)Utilities.ThinkTime * 100);


            goalZoom += Input.deltaZ;
            Zoom += (goalZoom - Zoom) / 4;
            ViewPosition = new Vector3(ViewPosition.X, ViewPosition.Y, Zoom);

            //How fast should we move
            multiplier = 7;
            if (Utilities.engine.Keyboard[Key.LShift])
            {
                multiplier = 25;
            }
            else if (Utilities.engine.Keyboard[Key.LControl])
            {
                multiplier = 1.0f;
            }
            

            //I SAID MOVE
            if (Utilities.engine.Keyboard[Key.W])
            {
                ViewPosition += new Vector3(0.0f, (float)Utilities.ThinkTime, 0.0f) * multiplier;
            }
            if (Utilities.engine.Keyboard[Key.A])
            {
                ViewPosition += new Vector3(-(float)Utilities.ThinkTime, 0.0f, 0.0f) * multiplier;
            }
            if (Utilities.engine.Keyboard[Key.S])
            {
                ViewPosition += new Vector3(0.0f, -(float)Utilities.ThinkTime, 0.0f) * multiplier;
            }
            if (Utilities.engine.Keyboard[Key.D])
            {
                ViewPosition += new Vector3((float)Utilities.ThinkTime, 0.0f, 0.0f) * multiplier;
            }

            if (Cursor != null)
            {
                Vector3 mousePos = new Vector3((Utilities.engine.Mouse.X - (Utilities.engine.Width / 2)), -(Utilities.engine.Mouse.Y - (Utilities.engine.Height / 2)), 0);
                MousePos = ViewPosition + Utilities.Get2Dto3D(Utilities.engine.Mouse.X, Utilities.engine.Mouse.Y, -ViewPosition.Z);
                Cursor.SetPos( MousePos);
            }

            //Tell the current mode to think
            switch (CurrentMode)
            {
                case EditMode.CreateEnt:
                    CreateEnt.Think();
                    break;

                case EditMode.CreatePhys:
                    CreatePhys.Think();
                    break;

                case EditMode.Transform:
                    Transform.Think();
                    break;
            }
        }

        static void View_CalcView()
        {
            //Set the camera matrix itself
            View.SetAngles(new Angle(0, -90, 0));
            View.SetPos(ViewPosition);

            if (Cursor != null)
            {
                Cursor.SetAngle((float)Utilities.Time);
            }
        }

        public static void Stop()
        {
            if (Cursor != null)
            {
                Cursor.Remove();
            }

            Utilities.engine.Mouse.ButtonDown -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.engine.Mouse.ButtonUp -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);

            GUI.GUIManager.PostDrawHUD -= new GUI.GUIManager.OnDrawHUD(GUIManager_PostDrawHUD);
            Utilities.engine.Keyboard.KeyDown -= new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
            View.CalcView -= new Action(View_CalcView);


            TopControl.IsVisible = false;
        }



        #region SPECIFIC MODE FUNCTIONALITY

        private class CreatePhys
        {
            private static ent_editor_build TempEnt;

            public static void MouseDown(MouseButtonEventArgs e)
            {
                if (TempEnt == null)
                {
                    TempEnt = EntManager.Create<ent_editor_build>();
                    TempEnt.Spawn();
                }
                TempEnt.AddPoint(Editor.MousePos);
                //TempEnt.Points.Add(MousePos);
                //Points.Add(MousePos);
            }

            public static void MouseUp(MouseButtonEventArgs e)
            {

            }

            public static void KeyDown(KeyboardKeyEventArgs e)
            {
                if (e.Key == Key.Enter && TempEnt != null && TempEnt.Points.Count > 1)
                {
                    TempEnt.Build();

                    TempEnt = null;
                }
            }

            public static void KeyUp(KeyboardKeyEventArgs e)
            {
            }

            public static void Think()
            {

            }
        }
        private class Transform
        {
            static float dist = (float)Math.Pow(5, 2); //use the distance squared to save on sqrt calls
            static bool dragging = false;
            static Vector3 offset = Vector3.Zero;
            public static void MouseDown(MouseButtonEventArgs e)
            {
                float curDist = dist;
                BaseEntity closest = null;
                //Try to find an entity
                BaseEntity[] ents = EntManager.GetAll();
                for (int i = 0; i < ents.Length; i++)
                {
                    Vector3 dif = ents[i].Position - Editor.MousePos;
                    Vector3 hitPos = new Vector3();
                    if (ents[i].Model != null && dif.LengthSquared < curDist && !(ents[i] is ent_cursor) && ents[i].Model.LineIntersectsBox(Editor.MousePos - new Vector3(0, 0, -100), Editor.MousePos + new Vector3(0, 0, -100), ref hitPos))
                    {
                        curDist = dif.LengthSquared;
                        closest = ents[i];
                    }
                }
                Editor.SetSelected(closest);

                if (closest != null)
                {
                    dragging = true;
                    offset = MousePos - closest.Position;
                }
                else
                {

                }
            }

            public static void MouseUp(MouseButtonEventArgs e)
            {
                if (dragging && Editor.SelectedEnt != null)
                {
                    Editor.SelectedEnt.SetPos(MousePos - offset);
                    dragging = false;
                }
            }

            public static void KeyDown(KeyboardKeyEventArgs e)
            {

            }

            public static void KeyUp(KeyboardKeyEventArgs e)
            {
            }

            public static void Think()
            {
                if (dragging && Editor.SelectedEnt != null)
                {
                    Editor.SelectedEnt.SetPos(MousePos - offset);
                }
            }
        }
        private class CreateEnt
        {
            public static void MouseDown(MouseButtonEventArgs e)
            {

            }

            public static void MouseUp(MouseButtonEventArgs e)
            {

            }

            public static void KeyDown(KeyboardKeyEventArgs e)
            {

            }

            public static void KeyUp(KeyboardKeyEventArgs e)
            {
            }

            public static void Think()
            {
            }
        }

        #endregion
    }

}
