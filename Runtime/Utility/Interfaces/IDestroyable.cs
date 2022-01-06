
/// <summary>
/// Used to free pooled objects or self managed destruction of root transform
/// </summary>
public interface IDestroyable
{
    void Destroy();
}
