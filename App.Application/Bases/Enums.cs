namespace App.Application.Bases
{

    public enum eAccessParent
    {
        None = 0,
        Person = 1,
        DataLog = 2,
        Driver = 3,
        Camion = 4,
        InsuranceCost = 5,
        Cmr = 6,
        Cmrinsurance = 7,
        Cabotage = 8,
        CmrRequest = 9,
        AppSetting = 11,
        Role = 12,
        RoleSection = 13,
        RoleSectionAccess = 14,
        AppLog = 15,
        Language = 16,
        User = 17,
        UserRequest = 18,
        Customer = 19,
        CmrPrint = 20,
        Bank = 21,
        PersonAccount = 22,
        Place = 23,
        Personnel = 23,
        Contract = 23,
        City = 24,
        Province = 25,
        Country = 26,
        CamionType = 27,
    }
    public enum eAccessChild
    {
        None = 0,
        View = 1,
        Add = 2,
        Edit = 3,
        Delete = 4,
        Detail = 5,
        DataLog = 6,
        ConfirmPrimitive = 7,
        Allocation = 8,
        Send = 9,
        Recive = 10,
        Cancellation = 11,
        Contact = 12,
        CmrPrintDetail = 13,
    }
    public enum eFilterType
    {
        Equal = 0,
        NotEqual = 1,
        GreaterThan = 2,
        LessThan = 3,
        GreaterThanOrEqual = 4,
        LessThanOrEqual = 5,
        Contains = 6,
        IsNull = 7,
        NotNull = 8
    }

    public enum EntityActionMode
    {
        None = 0,
        View = 1,
        Insert = 2,
        Update = 3,
        Delete = 4,
        Exist = 5
    }
    public enum eLanguage
    {
        Fa = 1,
        En = 2,
        Krd = 3,
        Ar = 4
    }

    public enum eTextAlighn
    {
        end = 1,
        start = 2,
        center = 3
    }

    public enum eUserType
    {
        Admin = 1,
        User = 2
    }

    public enum eRequestState
    {
        Order = 1,
        ConfirmPrimitive = 2,
        Allocation = 3,
        Send = 4,
        Recive = 5,
        CancelUser = 6,
        Cancellation = 7,
    }
    public enum eActionButton
    {
        Delete = 1,
        Edit = 2,
        Detail = 3,
        DataLog = 4,
    }

}
