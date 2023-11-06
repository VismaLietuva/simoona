namespace Shrooms.Contracts.Enums
{
    /// <summary>
    /// Strategy for building email messages when multiple recipients are provided:
    ///  - AllTo [default]: All recipients are put into "To" field.
    ///  - AllBcc: All recipients are put into "Bcc" field and sender's email (noreply@simoona.com) is put into "To" field.
    ///  - SingleTo: Single email per-recipient is sent with recipient being in "To" field.
    /// </summary>
    public enum EmailBuildingStrategy
    {
        AllTo,
        AllBcc,
        SingleTo
    }
}
