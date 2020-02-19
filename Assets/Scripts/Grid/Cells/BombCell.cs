using UnityEngine;

public class BombCell : GridCell {
    #region editor
    [SerializeField] private GameObject smokePs = default;
    #endregion

    #region private
    private new void Awake() {
        base.Awake();
    }
    #endregion

    #region public
    #endregion

    #region public virtual
    public override void select(bool state) {
        base.select(state);

        smokePs.SetActive(false);
    }

    public override void setColor(Color color, bool immediately = false) {
        
    }
    #endregion
}
