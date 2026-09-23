using UnityEngine;

namespace SimpracTest
{
    // Plásticos mates y tapicería gris de un acabado de acceso. Sin cromados
    // brillantes ni inserts de competición.
    public sealed class CabinMaterials
    {
        public readonly Material Rubber;
        public readonly Material PlasticDark;
        public readonly Material Plastic;
        public readonly Material PlasticSoft;
        public readonly Material Trim;
        public readonly Material Fabric;
        public readonly Material Body;
        public readonly Material Skin;
        public readonly Material Sleeve;
        public readonly Material Jeans;
        public readonly Material Navy;
        public readonly Material Khaki;
        public readonly Material ExaminerCloth;
        public readonly Material Hair;
        public readonly Material Belt;
        public readonly Material Paper;
        public readonly Material Screen;

        public CabinMaterials(CabinResources bin)
        {
            Rubber = Lit(bin, new Color(0.045f, 0.046f, 0.048f), 0.06f, 0f);
            PlasticDark = Lit(bin, new Color(0.07f, 0.075f, 0.08f), 0.08f, 0f);
            Plastic = Lit(bin, new Color(0.13f, 0.14f, 0.15f), 0.12f, 0f);
            PlasticSoft = Lit(bin, new Color(0.20f, 0.21f, 0.22f), 0.1f, 0f);
            Trim = Lit(bin, new Color(0.55f, 0.58f, 0.60f), 0.38f, 0.15f);
            Fabric = Lit(bin, new Color(0.14f, 0.15f, 0.16f), 0.08f, 0f);
            Body = Lit(bin, new Color(0.24f, 0.26f, 0.28f), 0.42f, 0.08f);
            Skin = Lit(bin, new Color(0.72f, 0.55f, 0.44f), 0.16f, 0f);
            Sleeve = Lit(bin, new Color(0.10f, 0.14f, 0.20f), 0.12f, 0f);
            Jeans = Lit(bin, new Color(0.20f, 0.26f, 0.36f), 0.14f, 0f);
            Navy = Lit(bin, new Color(0.11f, 0.16f, 0.24f), 0.12f, 0f);
            Khaki = Lit(bin, new Color(0.62f, 0.56f, 0.45f), 0.16f, 0f);
            ExaminerCloth = Lit(bin, new Color(0.18f, 0.20f, 0.24f), 0.14f, 0f);
            Hair = Lit(bin, new Color(0.12f, 0.10f, 0.09f), 0.1f, 0f);
            Belt = Lit(bin, new Color(0.45f, 0.48f, 0.50f), 0.3f, 0f);
            Paper = Lit(bin, new Color(0.86f, 0.87f, 0.84f), 0.12f, 0f);
            Screen = Lit(bin, new Color(0.01f, 0.015f, 0.02f), 0.55f, 0f);
        }

        public Material UnlitTexture(CabinResources bin, Texture texture, bool mirror)
        {
            Shader shader = Shader.Find("HDRP/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null) shader = Shader.Find("UI/Default");
            var material = bin.Track(new Material(shader));
            material.name = mirror ? "Espejo · imagen" : "Pantalla · imagen";
            if (material.HasProperty("_UnlitColor")) material.SetColor("_UnlitColor", Color.white);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            if (material.HasProperty("_EmissiveExposureWeight"))
                material.SetFloat("_EmissiveExposureWeight", 0f);
            string[] maps = { "_UnlitColorMap", "_BaseColorMap", "_MainTex", "_BaseMap" };
            for (int i = 0; i < maps.Length; i++)
            {
                if (!material.HasProperty(maps[i]) || texture == null) continue;
                material.SetTexture(maps[i], texture);
                if (!mirror) continue;
                material.SetTextureScale(maps[i], new Vector2(-1f, 1f));
                material.SetTextureOffset(maps[i], new Vector2(1f, 0f));
            }
            return material;
        }

        Material Lit(CabinResources bin, Color color, float smoothness, float metallic)
        {
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = bin.Track(new Material(shader));
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_DoubleSidedEnable")) material.SetFloat("_DoubleSidedEnable", 1f);
            if (material.HasProperty("_CullMode")) material.SetFloat("_CullMode", 0f);
            material.EnableKeyword("_DOUBLESIDED_ON");
            material.doubleSidedGI = true;
            return material;
        }
    }
}
