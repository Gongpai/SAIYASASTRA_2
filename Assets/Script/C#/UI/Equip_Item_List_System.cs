using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Equip_Item_List_System : MonoBehaviour
{
    [SerializeField] public Structs_Libraly.Item_Data itemData;
    private TextMeshProUGUI TextNumber;

    public int IndexEquip = 0;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        //print("------ 11 ----- Name : " + gameObject);
        TextNumber = gameObject.transform.GetChild(4).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        TextNumber.SetText(GameInstance.inventoryData[IndexEquip].Number.ToString());
    }
}