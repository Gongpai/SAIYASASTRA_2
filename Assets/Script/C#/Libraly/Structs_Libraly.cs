using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structs_Libraly : MonoBehaviour
{
    [System.Serializable]
    public struct Item_Data
    {
        public string Name;
        public int Number;
        public Sprite itemSprite;
    }
}