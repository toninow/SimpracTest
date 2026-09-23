using System.Collections.Generic;
using UnityEngine;

namespace SimpracTest
{
    // Materiales, mallas y texturas creados en Play se liberan al salir.
    public sealed class CabinResources : MonoBehaviour
    {
        readonly List<Object> owned = new List<Object>();

        public T Track<T>(T asset) where T : Object
        {
            if (asset != null) owned.Add(asset);
            return asset;
        }

        void OnDestroy()
        {
            for (int i = 0; i < owned.Count; i++)
            {
                Object asset = owned[i];
                if (asset == null) continue;
                if (asset is RenderTexture texture) texture.Release();
                Destroy(asset);
            }
            owned.Clear();
        }
    }
}
