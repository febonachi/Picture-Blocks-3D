using Utils;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public class DefaultCell : GridCell {
    #region editor
    [SerializeField] private MeshRenderer mesh = default;
    [SerializeField] private Transform meshHolder = default;
    [SerializeField] private ParticleSystem explosionPs = default;
    #endregion

    #region public properties
    public Color color => meshMaterial.color;
    public List<Color> colors => meshes.Select(m => m.material.color).ToList();
    public Transform platform => mesh.transform;
    public List<Transform> meshTransforms => meshes.Select(m => m.transform).ToList();
    #endregion

    private Material meshMaterial;
    private Material explosionPsMaterial;

    private List<MeshRenderer> meshes = new List<MeshRenderer>(4);

    #region private
    private new void Awake() {
        base.Awake();

        for(int i = 0; i < meshHolder.childCount; i++) meshes.Add(meshHolder.GetChild(i).GetComponent<MeshRenderer>());

        meshes.ForEach(mesh => mesh.gameObject.SetActive(false));

        meshMaterial = mesh.material;
        ParticleSystemRenderer renderer = explosionPs.GetComponent<ParticleSystemRenderer>();
        explosionPsMaterial = renderer.material;

        gameObject.SetActive(false);
    }
    #endregion

    #region public virtual
    public override void select(bool state) {
        base.select(state);

        if (!state) {
            if(Utility.maybe) {
                explosionPsMaterial.color = meshMaterial.color;
                explosionPs.Play();
            }
            setColor(Color.white, immediately: true);
        } else gameObject.SetActive(true);

        mesh.gameObject.SetActive(state);
    }

    public override void setColor(Color color, bool immediately = false) {
        if (immediately) {
            meshMaterial.DOKill();
            meshMaterial.color = color;
        } else meshMaterial.DOColor(color, .5f);
    }

    public void setColor(List<Color> colors, bool immediately = false) {
        mesh.gameObject.SetActive(false);

        for(int i = 0; i < colors.Count; i++){
            meshes[i].gameObject.SetActive(true);
            if (immediately) {
                meshes[i].material.DOKill();
                meshes[i].material.color = colors[i];
            }else meshes[i].material.DOColor(colors[i], .5f);
        }
    }
    #endregion
}
