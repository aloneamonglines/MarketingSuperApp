namespace MKS.Shared.Domain.Shared.Constants
{
    public static class SystemConstants
    {
        public class SystemUser
        {
            public static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
            public const string Name = "System.User.Name";
            public const string Thumbnail = "System.User.Thumbnail";
        }

        public static class MasterStatus
        {
            public const string None = "System.MasterStatus.None";
            public const string Active = "System.MasterStatus.Active";
            public const string Inactive = "System.MasterStatus.Inactive";
        }
    }
}