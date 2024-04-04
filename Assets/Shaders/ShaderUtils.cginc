float map(float value, float fromMin, float fromMax, float toMin, float toMax)
{
    return (value-fromMin)/(fromMax-fromMin) * (toMax-toMin) + toMin;
}

bool equal(float x, float y, float tolerance)
{
    return abs(x-y) < tolerance;
}

//Encode a value between 0 and one into a 4 chanel vector in order to increase precision during data transmission using textures
float4 encode1To4Chanels(float value)
{
    float4 col;
    uint r = value * 0xffffffffU;

    col.x = r / 0x01000000U;
    r = r % 0x01000000U;

    col.y = r / 0x00010000U;
    r = r % 0x00010000U;
    
    col.z = r / 0x00000100U;
    r = r % 0x00000100U;
    
    col.a = r;

    return col / 255;
}

//Decode a 1 channel value that was encoded using 1To4Chanels
float decode4To1Chanels(float4 col)
{
    uint uintValue = (col.x * 0x01000000U + col.y * 0x00010000U + col.z * 0x00000100U + col.a) * 255;
    return (float)uintValue / 0xffffffffU;
}
