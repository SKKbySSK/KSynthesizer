namespace KSynthesizer.Linux.InputEventEnum
{
    /// <summary>
    /// Relative code when InputEvent.Type == EV_ABS
    /// Converted from https://github.com/torvalds/linux/blob/master/include/uapi/linux/input-event-codes.h
    /// </summary>
    public enum InputRelativeCodes
    {
        REL_X = 0x00,
        REL_Y = 0x01,
        REL_Z = 0x02,
        REL_RX = 0x03,
        REL_RY = 0x04,
        REL_RZ = 0x05,
        REL_HWHEEL = 0x06,
        REL_DIAL = 0x07,
        REL_WHEEL = 0x08,
        REL_MISC = 0x09,
        REL_RESERVED = 0x0a,
        REL_WHEEL_HI_RES = 0x0b,
        REL_HWHEEL_HI_RES = 0x0c,
        REL_MAX = 0x0f,
        REL_CNT = REL_MAX + 1,
    }
}