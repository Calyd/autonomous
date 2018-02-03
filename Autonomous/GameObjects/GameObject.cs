﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameTry.GameObjects
{
    public abstract class GameObject
    {
        public float X { get; protected set; }
        public float Y { get; protected set; }
        public float VY { get; protected set; }
        public float VX { get; protected set; }

        protected BoundingBox boundingBox { get; set; }
        public float Width { get; protected set; }

        public float Height { get; private set; }

        public Model Model { get; protected set;  }

        protected float ModelRotate { get; set; }

        public GameObject()
        {
            ModelRotate = 0;
        }
        public abstract void Update(TimeSpan elapsed);
        public abstract void Draw(TimeSpan elapsed, Matrix view, Matrix projection, GraphicsDevice device);

        public virtual void Initialize()
        {
            boundingBox = GetBounds();
            float scaleX = Width / (boundingBox.Max.X - boundingBox.Min.X);
            Height = (boundingBox.Max.Z - boundingBox.Min.Z)*scaleX;
        }

        protected virtual Matrix TransformModelToWorld()
        {
            float scaleX = Width / (boundingBox.Max.X - boundingBox.Min.X) ;
            float translateZ = (boundingBox.Min.Y);

            var worldToView = Matrix.CreateRotationY(MathHelper.ToRadians(ModelRotate)) *  Matrix.CreateScale(scaleX) * Matrix.CreateTranslation(new Vector3(X, -translateZ * scaleX, -Y));
            return worldToView;
        }

        public BoundingBox GetBounds()
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            
            foreach (ModelMesh mesh in this.Model.Meshes)
            {
                if (this.GetType() == typeof(Car))
                    System.Diagnostics.Debug.WriteLine(mesh.Name);

                if (mesh.Name == "platform")
                    continue;
                
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    int vertexDataSize = vertexBufferSize / sizeof(float);
                    float[] vertexData = new float[vertexDataSize];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    for (int i = 0; i < vertexDataSize; i += vertexStride / sizeof(float))
                    {
                        Vector3 vertex = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]) ;

                        vertex = Vector3.Transform(vertex, Matrix.CreateRotationY(MathHelper.ToRadians(-ModelRotate)));
                        min = Vector3.Min(min, vertex);
                        max = Vector3.Max(max, vertex);
                    }
                }
            }

            return new BoundingBox(min, max);
        }
    }
}
