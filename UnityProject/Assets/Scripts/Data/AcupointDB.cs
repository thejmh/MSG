using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AcupointDB", menuName = "MSG/AcupointDB")]
public class AcupointDB : ScriptableObject
{
    public List<Acupoint> acupoints = new List<Acupoint>();

    public Acupoint GetAcupoint(int id)
    {
        return acupoints.Find(a => a.id == id);
    }
}
