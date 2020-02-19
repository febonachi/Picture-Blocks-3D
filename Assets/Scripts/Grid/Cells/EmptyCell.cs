using DG.Tweening;
using UnityEngine;

public class EmptyCell : GridCell {
    #region editor
    [SerializeField] private MeshRenderer mesh = default;
    #endregion

    private Material meshMaterial;

    #region private
    private new void Awake() {
        base.Awake();

        meshMaterial = mesh.material;
    }
    #endregion

    #region public virtual
    public override void setColor(Color color, bool immediately = false) {
        if (immediately) meshMaterial.color = color;
        else meshMaterial.DOColor(color, .5f);
    }
    #endregion
}
