using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Inventory_SO", menuName = "Inventory/Inventory_SO")]
public class Inventory_SO : ScriptableObject
{
    public List<Inventory> Bag;
}