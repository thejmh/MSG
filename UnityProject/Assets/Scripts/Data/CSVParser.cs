#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class CSVParser
{
    [MenuItem("MSG/Parse Acupoints CSV")]
    public static void ParseCSV()
    {
        string path = "Assets/Resources/Acupoints.csv";
        if (!File.Exists(path))
        {
            Debug.LogError("Acupoints.csv not found in Resources.");
            return;
        }

        AcupointDB db = ScriptableObject.CreateInstance<AcupointDB>();

        string[] lines = File.ReadAllLines(path);
        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            // Simple split by comma. Assumes no commas inside quoted fields.
            string[] cols = lines[i].Split(',');
            if (cols.Length >= 8)
            {
                Acupoint p = new Acupoint
                {
                    id = int.TryParse(cols[0], out int parsedId) ? parsedId : 0,
                    meridian = cols[1],
                    pointName = cols[2],
                    hanja = cols[3],
                    page = int.TryParse(cols[4], out int parsedPage) ? parsedPage : 0,
                    symptoms = cols[5],
                    priority = cols[6],
                    location = cols[7]
                };
                db.acupoints.Add(p);
            }
        }

        AssetDatabase.CreateAsset(db, "Assets/Resources/AcupointDB.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("AcupointDB successfully created with " + db.acupoints.Count + " entries.");
    }
}
#endif
