namespace KioskBrains.Common.KioskState
{
    public static class ComponentStatusCodeExtensions
    {
        public static bool IsOperational(this ComponentStatusCodeEnum code)
        {
            switch (code)
            {
                case ComponentStatusCodeEnum.Ok:
                case ComponentStatusCodeEnum.Warning:
                    return true;
                case ComponentStatusCodeEnum.Undefined:
                case ComponentStatusCodeEnum.Error:
                case ComponentStatusCodeEnum.Disabled:
                default:
                    return false;
            }
        }
    }
}