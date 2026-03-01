namespace MyApi.Application.Permissions;

public class MyApiPermissions
{
    public const string GroupName = "MyApi";

    public static class Roles
    {
        public const string Default = GroupName + ".Roles";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ManagePermissions = Default + ".ManagePermissions";
        public const string ManageMenus = Default + ".ManageMenus";
    }
}