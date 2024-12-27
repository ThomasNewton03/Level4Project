using System;
using System.Linq;
using UnityEngine;

namespace Visometry.Helpers
{
    public static class MeshHelper
    {
        public static byte[] GetVerticesAsByteArray(Mesh mesh)
        {
            return mesh.vertices.SelectMany(
                vertex =>
                {
                    var bytes = new byte[3 * sizeof(float)];
                    Buffer.BlockCopy(new float[] {vertex.x, vertex.y, vertex.z}, 0, bytes, 0, 12);
                    return bytes;
                }).ToArray();
        }
    }
}
