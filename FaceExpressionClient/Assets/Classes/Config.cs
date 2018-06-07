using System.Collections.Generic;

public class EmofaniConfig {

    private bool loaded = false;
    private static EmofaniConfig instance;
    private Dictionary<string, string> loadedData;
    private string path = "emofani.config";

    private EmofaniConfig()
    {
        loadedData = new Dictionary<string, string>();
    }

    public static EmofaniConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EmofaniConfig();
            }
            return instance;
        }
    }

    private void Load()
    {
        if (System.IO.File.Exists(path)) {
            string[] contents = System.IO.File.ReadAllLines(path);
            foreach (string entry in contents)
            {
                string[] keyValue = entry.Split('=');
                if (keyValue.Length == 2)
                {
                    if (!loadedData.ContainsKey(keyValue[0])) {
                        loadedData.Add(keyValue[0], keyValue[1]);
                    } else
                    {
                        loadedData[keyValue[0]] = keyValue[1];
                    }
                    
                }
            }
        }
    }

    public void Save()
    {
        List<string> contents = new List<string>();
        foreach (KeyValuePair<string, string> entry in loadedData)
        {
            // save every element to a new line in this format: key=value
            contents.Add(entry.Key + "=" + entry.Value);
        }
        System.IO.File.WriteAllLines(path, contents.ToArray());
    }

    public string GetValue(string name, string defaultValue)
    {
        string value = defaultValue;
        if (!loaded)
        {
            this.Load();
        }
        if (this.loadedData.ContainsKey(name))
        {
            value = this.loadedData[name];
        }
        return value;
    }

    public float GetFloat(string name, float defaultValue)
    {
        return float.Parse(this.GetValue(name, defaultValue.ToString()));
    }

    public int GetInt(string name, int defaultValue)
    {
        return int.Parse(this.GetValue(name, defaultValue.ToString()));
    }

    public void SetValue(string name, string value)
    {
        this.loadedData[name] = value;
    }

    public void SetInt(string name, int value)
    {
        this.SetValue(name, value.ToString());
    }

    public void SetFloat(string name, float value)
    {
        this.SetValue(name, value.ToString());
    }
}
