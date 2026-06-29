namespace HorreumStack.MiddleEnd.Core.Constants
{
    public static class AppConstants
    {
        public static object UbicacionesAlmacenes { get; internal set; }


        public static class Roles
        {
            public const string Owner = "Owner";
            public const string Invited = "Invited";
        }

        public static class Claims
        {
            public const string UserId = "UserId";
            public const string Email = "Email";
        }

        public static class Almacenes
        {
            public const string PrefixCodigo = "ALM-";
        }
        public static class Ubicaciones
        {
            public const string PrefixCodigo = "UBI-";
        }
        public static class Proyectos
        {
            public const string PrefixCodigo = "PRY-";
        }
        public static class UbicacionesTipos
        {
            public const string PrefixCodigo = "UTI-";
        }


    }
}
