using UnityEngine;

[CreateAssetMenu(fileName = "CookingResult", menuName = "Cooking/Cooking Result")]
public class CookingResult : ScriptableObject
{
    public string resultName;
    public Sprite resultIcon;
    [TextArea(3, 5)]
    public string description;
    [TextArea(2, 4)]
    public string buffDescription;
    public bool isSuccess; // true = успешное блюдо, false = провал
}