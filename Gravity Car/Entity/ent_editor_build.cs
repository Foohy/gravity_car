﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using OlegEngine.Entity;
using OlegEngine;

namespace Gravity_Car.Entity
{
    class ent_editor_build : BaseEntity 
    {
        public List<Vector3> Points = new List<Vector3>();
        public bool Built = false;
        private List<Vector3> _meshPoints = new List<Vector3>();
        private Mesh previewMesh;
        public override void Init()
        {
            previewMesh = new Mesh();
            this.Model = Resource.GetMesh("engine/ball.obj");
            this.Material = Resource.GetMaterial("engine/white");
            //this.Mat.SetShader(Resource.GetProgram("default"));
            this.Scale = Vector3.One * 0.05f;
            this.Color = new Vector3(0, 1.0f, 0);

            previewMesh.DrawMode = BeginMode.Lines;
            previewMesh.mat = Resource.GetMaterial("engine/white_simple");
            previewMesh.UsageHint = BufferUsageHint.StreamDraw;
            previewMesh.Color = new Vector3(1.0f,0, 0);
            previewMesh.ShouldDrawDebugInfo = false;
        }

        public void AddPoint(Vector3 point)
        {
            Points.Add(point);
            //_meshPoints.Add(new Vector3(point.X, point.Y, this.Position.Z));
            Vertex[] verts = GenerateVerts();
            int[] elements = GenerateElements(verts);

            if (elements.Length > 1)
            {
                previewMesh.UpdateMesh(verts, elements);
            }
        }

        public void Build()
        {
            this.Built = true;
            previewMesh.Color = new Vector3(1.0f, 1.0f, 1.0f);
        }
        private Vertex[] GenerateVerts()
        {
            List<Vertex> verts = new List<Vertex>();
            for (int i = 0; i + 1 < Points.Count; i++)
            {
                verts.Add(new Vertex( new Vector3(Points[i].X, Points[i].Y, Points[i].Z)));
                verts.Add(new Vertex( new Vector3(Points[i + 1].X, Points[i + 1].Y, Points[i+1].Z)));
            }

            verts.Add(new Vertex(Points[Points.Count-1]));
            verts.Add( new Vertex(Points[0]));

            return verts.ToArray();
        }

        private int[] GenerateElements(Vertex[] verts)
        {
            int[] elements = new int[verts.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = i;
            }

            return elements;
        }
        public override void Draw()
        {
            if (Built)
            {
                //base.Draw();
                previewMesh.Draw();
            }
            else
            {
                Vector3 oldPos = this.Position;
                GL.LineWidth(3.0f);
                this.previewMesh.Draw();
                for (int i = 0; i < Points.Count; i++)
                {
                    this.SetPos(Points[i]);
                    base.Draw();
                }
                this.SetPos(oldPos);
            }
        }
    }
}
