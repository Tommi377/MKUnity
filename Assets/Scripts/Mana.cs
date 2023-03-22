using UnityEngine;

public class Mana {
    public enum Types {
        Red, Green, Blue, White, Gold, Black
    }

    public Types Type {  get; private set; }
    public bool Crystal { get; private set; }

    public Mana(Types type, bool crystal) {
        Type = type;
        Crystal = crystal;
    }

    public static Types GetRandomType() => (Types)Random.Range(0, 6);

    private static readonly Color[] colors = { Color.red, Color.green, Color.blue, Color.white, Color.yellow, Color.black };
    public static Color GetColor(Types type) {
        return colors[(int)type];
    }
}
