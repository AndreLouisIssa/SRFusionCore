using SRML.Config.Attributes;

namespace FusionCore
{
    [ConfigFile("settings")]
    public static class Config
    {
        // @MagicGonads
        public static string exclude = "GLITCH_SLIME; TARR_SLIME; GLITCH_TARR_SLIME";
    }
}