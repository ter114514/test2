/// <summary>
/// 可被暫停行動的單位（吸血技能時停用）。
/// </summary>
public interface IPausable
{
    /// <summary>暫停行動</summary>
    void PauseActions();

    /// <summary>恢復行動</summary>
    void ResumeActions();

    /// <summary>是不是 Boss（Boss 不被時停影響）</summary>
    bool IsBoss { get; }
}