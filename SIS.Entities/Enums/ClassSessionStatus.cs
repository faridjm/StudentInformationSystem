namespace SIS.Entities.Enums
{
    /// <summary>
    /// Dərs sessiyasının statusu. Hər sessiya bu 4 vəziyyətdən birində olur.
    /// </summary>
    public enum ClassSessionStatus
    {
        /// <summary>Planlaşdırılıb, hələ keçməyib.</summary>
        Scheduled = 0,

        /// <summary>Sessiya keçirilib, davamiyyət qeyd edilib.</summary>
        Completed = 1,

        /// <summary>Ləğv olunub (müəllim xəstə, bayram, və s.). CancellationReason yazılır.</summary>
        Cancelled = 2,

        /// <summary>Əvəzedici dərs (makeup). OriginalSessionId əvəz etdiyi sessiyanı göstərir.</summary>
        Makeup = 3
    }
}
