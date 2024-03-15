float map(float value, float fromMin, float fromMax, float toMin, float toMax)
{
    return (value-fromMin)/(fromMax-fromMin) * (toMax-toMin) + toMin;
}


