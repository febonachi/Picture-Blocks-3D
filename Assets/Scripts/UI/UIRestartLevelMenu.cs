using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class UIRestartLevelMenu : MonoBehaviour, IUIElement {
    #region editor    
    [SerializeField] private Button restartLevelButton = default;
    #endregion

    private Animator animator;

    #region private
    private void Awake() {
        animator = GetComponent<Animator>();

        restartLevelButton.onClick.AddListener(() => GameController.instance.restartLevel());
    }
    #endregion

    #region IUIElement
    public void show() {
        if(!gameObject.activeSelf) gameObject.SetActive(true);

        animator.SetTrigger("show");
    }

    public void hide() { 
        
    }

    public void reset() {

    }
    #endregion
}
