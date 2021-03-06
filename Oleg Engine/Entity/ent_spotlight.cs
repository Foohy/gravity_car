﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine.Entity
{
    public class ent_spotlight : BaseEntity 
    {
        public bool Enabled { get; set; }
        public bool ExpensiveShadows { get; set; }
        public float Cutoff { get; set; }
        public float Constant { get; set; }
        public float AmbientIntensity { get; set; }
        public float DiffuseIntensity { get; set; }

        public ShadowInfo shadowInfo;
        private SpotLight cheapLight;

        public override void Init()
        {
            shadowInfo = new ShadowInfo(Position, this.Angles.Forward(), Resource.GetTexture("effects/flashlight.png"), 1.0f );
            shadowInfo.Linear = 0.01f;
            cheapLight = new SpotLight();
            cheapLight.Linear = 0.01f;

            ShadowTechnique.SetLights += new Action(ShadowTechnique_SetLights);

            AmbientIntensity = 0.0f;
            DiffuseIntensity = 1.0f;

            this.Enabled = true;
            this.ExpensiveShadows = true;
            this.ShouldDraw = false;
        }

        void ShadowTechnique_SetLights()
        {
            if (this.Enabled)
            {
                shadowInfo.AmbientIntensity = 0.0f;
                shadowInfo.DiffuseIntensity = 1.0f;
                shadowInfo.Color = Color;
                shadowInfo.Constant = Constant;
                shadowInfo.Cutoff = Cutoff;
                shadowInfo.Direction = this.Angles.Forward();
                shadowInfo.Position = Position;
                shadowInfo.Cheap = !this.ExpensiveShadows;

                ShadowTechnique.AddLightsource(shadowInfo);
            }
        }

        public override void Remove()
        {
            base.Remove();
            ShadowTechnique.SetLights -= ShadowTechnique_SetLights;
        }
    }
}
