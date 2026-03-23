using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Chip_SO", menuName = "Chip_SO")]
public class ChipDatalist_SO : ScriptableObject
{
    public List<Chip> chipList;
}