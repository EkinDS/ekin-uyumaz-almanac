using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Assets.Core
{
   public class ButtonAnimator : MonoBehaviour
   {
      private CanvasGroup thisCanvasGroup;
      private Button thisButton;


      public void OnClicked()
      {
         thisButton.interactable = false;
      }
      
      
      public void Appear()
      {
         gameObject.SetActive(true);
         
         thisCanvasGroup = GetComponent<CanvasGroup>();
         thisButton = GetComponent<Button>();
         
         thisButton.interactable = true;
         thisCanvasGroup.alpha = 0F;
         thisCanvasGroup.DOFade(1F, 0.25F).OnComplete((() =>
         {
            thisButton.interactable = true;
         }));
      }


      private void OnDestroy()
      {
         DOTween.Kill(gameObject);
      }
   }
}