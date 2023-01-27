﻿namespace MareSynchronos.MareConfiguration;

[Serializable]
public class Authentication
{
    public string CharacterName { get; set; } = string.Empty;
    public uint WorldId { get; set; } = 0;
    public int SecretKeyIdx { get; set; } = -1;
}
