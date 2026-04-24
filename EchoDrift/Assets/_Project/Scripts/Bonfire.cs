using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Bonfire : MonoBehaviour, IInteractable
{
    [SerializeField] private string hintText = "Нажмите E, чтобы зажечь факел";
    //[SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private Light2D bonfireLight;

    private bool IsInteractable = true;

    public void Interact(PlayerController player)
    {
        if (!IsInteractable) return;

        player.InginteTorch();

        UIManager.Instance.ShowHint("Факел зажжен!", 1.5f);     // DEBUG
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player") && IsInteractable) UIManager.Instance.ShowHint(hintText, 0f);
        Debug.Log("in collider");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) UIManager.Instance.HideHint();
    }
}
