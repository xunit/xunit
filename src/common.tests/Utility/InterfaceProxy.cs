/// <summary>
/// Used in substitutes, so that ToString() will just show the type name, and not try to JSONify
/// the Castle Proxy (which is useless and verbose).
/// </summary>
public class InterfaceProxy<T>
{
    public override string ToString() { return typeof(T).Name; }
}
