using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;


/// <summary>
/// 主要用于网格面反转
/// </summary>
public class reverseMesh : MonoBehaviour {
    [Tooltip("Set whether or not you want this interactible to highlight when hovering over it")]
    public bool highlightOnHover = true;
    protected MeshRenderer[] highlightRenderers;
    protected MeshRenderer[] existingRenderers;
    protected GameObject highlightHolder;
    protected SkinnedMeshRenderer[] highlightSkinnedRenderers;
    protected SkinnedMeshRenderer[] existingSkinnedRenderers;
    protected static Material highlightMat;
    public bool isHovering { get; protected set; }
    public bool wasHovering { get; protected set; }
    public List<Hand> hoveringHands = new List<Hand>();
    [Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
    public GameObject[] hideHighlight = new GameObject[10];
    // Use this for initialization
    void Start () {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        List<int> tri = new List<int>(mesh.triangles);
        tri.Reverse();
        mesh.triangles = tri.ToArray();
        //CreateHighlightRenderers();
    }
    /// <summary>
    /// Called when a Hand starts hovering over this object
    /// </summary>
    protected virtual void OnHandHoverBegin(Hand hand)
    {
        wasHovering = isHovering;
        isHovering = true;

        hoveringHands.Add(hand);

        if (highlightOnHover == true && wasHovering == false)
        {
            CreateHighlightRenderers();
            //UpdateHighlightRenderers();
        }
    }
    // Update is called once per frame
    void Update () {
        

    }
    protected virtual bool ShouldIgnoreHighlight(Component component)
    {
        return ShouldIgnore(component.gameObject);
    }
    protected virtual bool ShouldIgnore(GameObject check)
    {
        for (int ignoreIndex = 0; ignoreIndex < hideHighlight.Length; ignoreIndex++)
        {
            if (check == hideHighlight[ignoreIndex])
                return true;
        }

        return false;
    }
    /// <summary>
    /// 轮廓高亮
    /// </summary>
    protected virtual void CreateHighlightRenderers()
    {
        existingSkinnedRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        highlightHolder = new GameObject("Highlighter");
        highlightSkinnedRenderers = new SkinnedMeshRenderer[existingSkinnedRenderers.Length];

        for (int skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
        {
            SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];

            if (ShouldIgnoreHighlight(existingSkinned))
                continue;

            GameObject newSkinnedHolder = new GameObject("SkinnedHolder");
            newSkinnedHolder.transform.parent = highlightHolder.transform;
            SkinnedMeshRenderer newSkinned = newSkinnedHolder.AddComponent<SkinnedMeshRenderer>();
            Material[] materials = new Material[existingSkinned.sharedMaterials.Length];
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                materials[materialIndex] = highlightMat;
            }

            newSkinned.sharedMaterials = materials;
            newSkinned.sharedMesh = existingSkinned.sharedMesh;
            newSkinned.rootBone = existingSkinned.rootBone;
            newSkinned.updateWhenOffscreen = existingSkinned.updateWhenOffscreen;
            newSkinned.bones = existingSkinned.bones;

            highlightSkinnedRenderers[skinnedIndex] = newSkinned;
        }

        MeshFilter[] existingFilters = this.GetComponentsInChildren<MeshFilter>(true);
        existingRenderers = new MeshRenderer[existingFilters.Length];
        highlightRenderers = new MeshRenderer[existingFilters.Length];

        for (int filterIndex = 0; filterIndex < existingFilters.Length; filterIndex++)
        {
            MeshFilter existingFilter = existingFilters[filterIndex];
            MeshRenderer existingRenderer = existingFilter.GetComponent<MeshRenderer>();

            if (existingFilter == null || existingRenderer == null || ShouldIgnoreHighlight(existingFilter))
                continue;

            GameObject newFilterHolder = new GameObject("FilterHolder");
            newFilterHolder.transform.parent = highlightHolder.transform;
            MeshFilter newFilter = newFilterHolder.AddComponent<MeshFilter>();
            newFilter.sharedMesh = existingFilter.sharedMesh;
            MeshRenderer newRenderer = newFilterHolder.AddComponent<MeshRenderer>();

            Material[] materials = new Material[existingRenderer.sharedMaterials.Length];
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                materials[materialIndex] = highlightMat;
            }
            newRenderer.sharedMaterials = materials;

            highlightRenderers[filterIndex] = newRenderer;
            existingRenderers[filterIndex] = existingRenderer;
        }
    }
}
