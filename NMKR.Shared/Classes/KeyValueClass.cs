namespace NMKR.Shared.Classes;

public delegate void Notify();
public class KeyValueClass
{
  
    public event Notify OnValueChanged;
    public string Key { get; set; }
    private string _value = "";
    public string Value {
        get => _value;
        set
        {
            _value=value;
            OnValueChanged?.Invoke(); 
        } }
    public int Id { get; set; }

    public KeyValueClass(int id, string key, string value)
    {
        Id = id;
        Key = key;
        Value = value;
    }
    public KeyValueClass(string key, string value)
    {
        Key = key;
        Value = value;
    }
    public KeyValueClass()
    {
    }
}