using DG.Tweening;
using UnityEngine;

public class HighlightedCell : GridCell {
    #region editor
    [SerializeField] private MeshRenderer mesh = default;
    [SerializeField] private GameObject highlightPs = default;
    #endregion

    private Material meshMaterial;

    //private ParticleSystem circlePs;

    #region private
    private new void Awake() {
        base.Awake();

        meshMaterial = mesh.material;

        //circlePs = highlightPs.transform.GetChild(0).GetComponent<ParticleSystem>();
    }
    #endregion

    #region public virtual
    public override void select(bool state) {
        base.select(state);

        highlightPs.SetActive(false);
    }

    public override void setColor(Color color, bool immediately = false) {
        ParticleSystem[] particles = highlightPs.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particles) {
            ParticleSystem.MainModule psMainModule = ps.main;
            psMainModule.startColor = color;
            ps.Play();
        }

        if (immediately) meshMaterial.color = color;
        else meshMaterial.DOColor(color, .5f);
    }
    #endregion
}
