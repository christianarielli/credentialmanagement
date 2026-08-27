namespace CredentialManagement
{
    [System.Obsolete("Use PersistenceType instead. The legacy name is retained for source compatibility.")]
    public enum PersistanceType : uint
    {
        Session = 1,
        LocalComputer = 2,
        Enterprise = 3
    }
}
