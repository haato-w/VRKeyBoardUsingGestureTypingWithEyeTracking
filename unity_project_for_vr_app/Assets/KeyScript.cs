using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyScript : MonoBehaviour
{
    public string keyCharacter;
    public string uppercaseKeyCharacter;
    public string secondaryKeyCharacter;
    public string uppercaseSecondaryKeyCharacter;

    public TMP_Text textComponent;
    public GameObject keyMesh;
    private bool isSelected = false;

    private bool isUsingUppercase = false;
    private bool isUsingSecondary = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelected(bool selected) {
        this.isSelected = selected;
        UpdateAppearance();
        
        if (keyCharacter == "123" || keyCharacter == "Enter" || keyCharacter == "Shift") hideKey();
    }

    public void SetUseSecondary(bool useSecondary) {
        this.isUsingSecondary = useSecondary;
        UpdateText();
    }

    public void SetUseUppercase(bool useUppercase) {
        this.isUsingUppercase = useUppercase;
        UpdateText();
    }

    private void UpdateAppearance() {
        if (isSelected) {
            keyMesh.GetComponent<Renderer>().material.color = new Color(1, 1, 0, 1);
            Debug.Log("recolored");
        } else {
            keyMesh.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 1);
        }
    }

    private void UpdateText() {
        if (isUsingUppercase) {
            if (isUsingSecondary) {
                textComponent.text = uppercaseSecondaryKeyCharacter;
            } else {
                textComponent.text = uppercaseKeyCharacter;
            }
        } else {
            if (isUsingSecondary) {
                textComponent.text = secondaryKeyCharacter;
            } else {
                textComponent.text = keyCharacter;
            }
        }

        if (keyCharacter == "123" || keyCharacter == "Enter" || keyCharacter == "Shift") hideKey();
    }

    public Vector3 GetKeyWorldPos() {
        return gameObject.transform.position;
    }

    public void hideKey() {
        textComponent.text = "";
        keyMesh.GetComponent<Renderer>().material.color = Color.white;
    }
}
